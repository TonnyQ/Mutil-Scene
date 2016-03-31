using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate QuadNode QuadCreateNode(Rect bnd);
public delegate void QuadForeachLeafNode(QuadLeafNode leaf);


public class QuadDebug
{
    public static void Assert(bool expr)
    {
        System.Diagnostics.Debug.Assert(expr);
    }

    public static void DrawRect(Rect r, float y, Color c, float padding = 0.0f)
    {
        Debug.DrawLine(new Vector3(r.xMin + padding, y, r.yMin + padding), new Vector3(r.xMin + padding, y, r.yMax - padding), c);
        Debug.DrawLine(new Vector3(r.xMin + padding, y, r.yMin + padding), new Vector3(r.xMax - padding, y, r.yMin + padding), c);
        Debug.DrawLine(new Vector3(r.xMax - padding, y, r.yMax - padding), new Vector3(r.xMin + padding, y, r.yMax - padding), c);
        Debug.DrawLine(new Vector3(r.xMax - padding, y, r.yMax - padding), new Vector3(r.xMax - padding, y, r.yMin + padding), c);
    }
}


public static class QuadUtility
{
    public static bool Intersects(Rect nodeBound, QuadData userData)
    {
        Rect r = new Rect(
            userData.GetCenter().x - userData.GetExtends().x,
            userData.GetCenter().z - userData.GetExtends().z,
            userData.GetExtends().x * 2.0f,
            userData.GetExtends().z * 2.0f);
        return nodeBound.Overlaps(r);
    }

    public static bool Intersects(Rect nodeBound, Vector2 targetCenter, float targetRadius)
    {
        bool xOutside = targetCenter.x + targetRadius < nodeBound.xMin || targetCenter.x - targetRadius > nodeBound.xMax;
        bool yOutside = targetCenter.y + targetRadius < nodeBound.yMin || targetCenter.y - targetRadius > nodeBound.yMax;
        bool outside = xOutside || yOutside;
        return !outside;
    }

    public static void BuildRecursively(QuadNode node)
    {
        // parameters
        float subWidth = node.Bound.width * 0.5f;
        float subHeight = node.Bound.height * 0.5f;
        bool isPartible = subWidth >= QuadSetting.CellSizeThreshold && subHeight >= QuadSetting.CellSizeThreshold;

        // create subnodes
        QuadCreateNode _nodeCreator = (bnd) => { return new QuadNode(bnd); };
        QuadCreateNode _leafCreator = (bnd) => { return new QuadLeafNode(bnd); };
        QuadCreateNode creator = isPartible ? _nodeCreator : _leafCreator;
        node.SetSubNodes(new QuadNode[QuadNode.SubCount] {
            creator(new Rect(node.Bound.xMin,             node.Bound.yMin,                subWidth, subHeight)),
            creator(new Rect(node.Bound.xMin + subWidth,  node.Bound.yMin,                subWidth, subHeight)),
            creator(new Rect(node.Bound.xMin,             node.Bound.yMin + subHeight,    subWidth, subHeight)),
            creator(new Rect(node.Bound.xMin + subWidth,  node.Bound.yMin + subHeight,    subWidth, subHeight)),
        });

        // do it recursively
        if (isPartible)
        {
            foreach (var sub in node.SubNodes)
            {
                BuildRecursively(sub);
            }
        }
    }

    public static void TraverseAllLeaves(QuadNode node, QuadForeachLeafNode func)
    {
        if (node is QuadLeafNode)
            func(node as QuadLeafNode);
        else
            foreach (var sub in node.SubNodes)
                TraverseAllLeaves(sub, func);
    }

    public static QuadLeafNode FindLeafRecursively(QuadNode node, Vector2 point)
    {
        if (!node.Bound.Contains(point))
            return null;

        if (node is QuadLeafNode)
            return node as QuadLeafNode;

        foreach (var sub in node.SubNodes)
        {
            QuadLeafNode leaf = FindLeafRecursively(sub, point);
            if (leaf != null)
                return leaf;
        }

        QuadDebug.Assert(false);  // should never reaches here 
        return null;
    }

    public static void GenerateSwappingLeaves(QuadNode node, QuadLeafNode active, List<QuadLeafNode> holdingLeaves, out List<QuadLeafNode> inLeaves, out List<QuadLeafNode> outLeaves)
    {
        List<QuadLeafNode> inList = new List<QuadLeafNode>();
        GenerateLeavesByDist(node, active, QuadSetting.CellSwapInDist, ref inList);
        inList.RemoveAll((item) => holdingLeaves.Contains(item));
        inLeaves = inList;

        List<QuadLeafNode> outList = new List<QuadLeafNode>();
        GenerateLeavesByDist(node, active, QuadSetting.CellSwapOutDist, ref outList);
        List<QuadLeafNode> outFilteredList = new List<QuadLeafNode>();
        foreach (var leaf in holdingLeaves)
        {
            if (!outList.Contains(leaf))
            {
                outFilteredList.Add(leaf);
            }
        }       
        outLeaves = outFilteredList;
    }

    private static void GenerateLeavesByDist(QuadNode node, QuadLeafNode active, float dist, ref List<QuadLeafNode> leaves)
    {
        if (!Intersects(node.Bound, active.Bound.center, dist))
            return;

        if (node is QuadLeafNode)
            leaves.Add(node as QuadLeafNode);
        else
            foreach (var sub in node.SubNodes)
                GenerateLeavesByDist(sub, active, dist, ref leaves);
    }
}

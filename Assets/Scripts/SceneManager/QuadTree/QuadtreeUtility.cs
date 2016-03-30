using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate QtNode UQtCreateNode(Rect bnd);
public delegate void QTreeForeachLeafNode(QtLeafNode leaf);


public class QtDebug
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


public static class QtUtility
{
    public static bool Intersects(Rect nodeBound, QtData userData)
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

    public static void BuildRecursively(QtNode node)
    {
        // parameters
        float subWidth = node.Bound.width * 0.5f;
        float subHeight = node.Bound.height * 0.5f;
        bool isPartible = subWidth >= QtSetting.CellSizeThreshold && subHeight >= QtSetting.CellSizeThreshold;

        // create subnodes
        UQtCreateNode _nodeCreator = (bnd) => { return new QtNode(bnd); };
        UQtCreateNode _leafCreator = (bnd) => { return new QtLeafNode(bnd); };
        UQtCreateNode creator = isPartible ? _nodeCreator : _leafCreator;
        node.SetSubNodes(new QtNode[QtNode.SubCount] {
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

    public static void TraverseAllLeaves(QtNode node, QTreeForeachLeafNode func)
    {
        if (node is QtLeafNode)
            func(node as QtLeafNode);
        else
            foreach (var sub in node.SubNodes)
                TraverseAllLeaves(sub, func);
    }

    public static QtLeafNode FindLeafRecursively(QtNode node, Vector2 point)
    {
        if (!node.Bound.Contains(point))
            return null;

        if (node is QtLeafNode)
            return node as QtLeafNode;

        foreach (var sub in node.SubNodes)
        {
            QtLeafNode leaf = FindLeafRecursively(sub, point);
            if (leaf != null)
                return leaf;
        }

        QtDebug.Assert(false);  // should never reaches here 
        return null;
    }

    public static void GenerateSwappingLeaves(QtNode node, QtLeafNode active, List<QtLeafNode> holdingLeaves, out List<QtLeafNode> inLeaves, out List<QtLeafNode> outLeaves)
    {
        List<QtLeafNode> inList = new List<QtLeafNode>();
        GenerateLeavesByDist(node, active, QtSetting.CellSwapInDist, ref inList);
        inList.RemoveAll((item) => holdingLeaves.Contains(item));
        inLeaves = inList;

        List<QtLeafNode> outList = new List<QtLeafNode>();
        GenerateLeavesByDist(node, active, QtSetting.CellSwapOutDist, ref outList);
        List<QtLeafNode> outFilteredList = new List<QtLeafNode>();
        foreach (var leaf in holdingLeaves)
        {
            if (!outList.Contains(leaf))
            {
                outFilteredList.Add(leaf);
            }
        }
        outLeaves = outFilteredList;
    }

    private static void GenerateLeavesByDist(QtNode node, QtLeafNode active, float dist, ref List<QtLeafNode> leaves)
    {
        if (!Intersects(node.Bound, active.Bound.center, dist))
            return;

        if (node is QtLeafNode)
            leaves.Add(node as QtLeafNode);
        else
            foreach (var sub in node.SubNodes)
                GenerateLeavesByDist(sub, active, dist, ref leaves);
    }
}

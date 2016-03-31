using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void CellChanged(QuadLeafNode left, QuadLeafNode entered);
public delegate void CellSwapIn(QuadLeafNode leaf);
public delegate void CellSwapOut(QuadLeafNode leaf);

public static class QuadSetting
{
    // this value determines the smallest cell size
    // the space-partition process would stop dividing if cell size is smaller than this value
    public static float CellSizeThreshold = 64.0f;

    // swap-in distance of cells
    public static float CellSwapInDist = 50.0f;
    // swap-out distance of cells 
    //  (would be larger than swap-in to prevent poping)
    public static float CellSwapOutDist = 80.0f;

    // time interval to update the focus point,
    // so that a new swap would potentially triggered (in seconds)
    public static float SwapTriggerInterval = 0.5f;
    // time interval to update the in/out swapping queues (in seconds)
    public static float SwapProcessInterval = 0.2f;
}

// user data stored in quadtree leaves
public interface QuadData
{
    Vector3 GetCenter();
    Vector3 GetExtends();

    void SwapIn();
    void SwapOut();

    bool IsSwapInCompleted();
    bool IsSwapOutCompleted();
}

/// <summary>
/// QuadTree Node Class
/// </summary>
public class QuadNode
{
    public QuadNode(Rect bound)
    {
        _bound = bound;
    }

    public Rect Bound { get { return _bound; } }
    protected Rect _bound;

    public virtual void SetSubNodes(QuadNode[] subNodes)
    {
        _subNodes = subNodes;
    }

    public virtual void Receive(QuadData userData)
    {
        if (!QuadUtility.Intersects(Bound, userData))
        {
            return;
        }

        foreach (var sub in SubNodes)
        {
            sub.Receive(userData);
        }
    }

    public QuadNode[] SubNodes { get { return _subNodes; } }
    public const int SubCount = 4;
    protected QuadNode[] _subNodes = null;
}

/// <summary>
/// QuadTree Leaf Node Class
/// </summary>
public class QuadLeafNode : QuadNode
{
    public List<QuadData> AffectedObjects { get { return _affectedObjects; } }
    private List<QuadData> _ownedObjects = null;
    private List<QuadData> _affectedObjects = null;

    public QuadLeafNode(Rect bound) : base(bound)
    {
        _ownedObjects = new List<QuadData>();
        _affectedObjects = new List<QuadData>();
    }

    public override void SetSubNodes(QuadNode[] subNodes)
    {
        QuadDebug.Assert(false);
    }

    public override void Receive(QuadData userData)
    {
        if (!QuadUtility.Intersects(Bound, userData))
            return;

        if (Bound.Contains(new Vector2(userData.GetCenter().x, userData.GetCenter().z)))
        {
            _ownedObjects.Add(userData);            
        }
        else
        {
            _affectedObjects.Add(userData);
        }
    }

    public bool Contains(QuadData userData)
    {
        if (_ownedObjects.Contains(userData))
            return true;
        if (_affectedObjects.Contains(userData))
            return true;
        return false;
    }

    public void SwapIn()
    {
        foreach (var obj in _ownedObjects)
            obj.SwapIn();
        foreach (var obj in _affectedObjects)
            obj.SwapIn();
    }

    public void SwapOut()
    {
        foreach (var obj in _ownedObjects)
            obj.SwapOut();
    }

    public bool IsSwapInCompleted()
    {
        foreach (var obj in _ownedObjects)
        {
            if (!obj.IsSwapInCompleted())
                return false;
        }
        foreach (var obj in _affectedObjects)
        {
            if (!obj.IsSwapInCompleted())
                return false;
        }
        return true;
    }

    public bool IsSwapOutCompleted()
    {
        foreach (var obj in _ownedObjects)
        {
            if (!obj.IsSwapOutCompleted())
                return false;
        }
        return true;
    }
}

public class QuadTree
{
    /// <summary>
    /// Quad Tree root node.
    /// </summary>
    private QuadNode _root;

    /// <summary>
    /// current focus point.
    /// </summary>
    private Vector3 _focusPoint;

    /// <summary>
    /// curreny focus leaf node
    /// </summary>
    private QuadLeafNode _focusLeaf;

    private List<QuadLeafNode> _holdingLeaves = new List<QuadLeafNode>();
    private List<QuadLeafNode> _swapInQueue = new List<QuadLeafNode>();
    private List<QuadLeafNode> _swapOutQueue = new List<QuadLeafNode>();

    private float _lastSwapTriggeredTime = 0.0f;
    private float _lastSwapProcessedTime = 0.0f;

    public QuadTree(Rect bound)
    {
        _root = new QuadNode(bound);
        QuadUtility.BuildRecursively(_root);
    }

    public void Update(Vector2 focusPoint)
    {
        if (EnableDebugLines)
        {
            DrawDebugLines();
        }

        if (Time.time - _lastSwapTriggeredTime > QuadSetting.SwapTriggerInterval)
        {
            if (UpdateFocus(focusPoint))
                PerformSwapInOut(_focusLeaf);
            _lastSwapTriggeredTime = Time.time;
        }

        if (Time.time - _lastSwapProcessedTime > QuadSetting.SwapProcessInterval)
        {
            ProcessSwapQueues();
            _lastSwapProcessedTime = Time.time;
        }
    }

    public void Receive(QuadData qud)
    {
        _root.Receive(qud);
    }

    public event CellChanged FocusCellChanged;
    public event CellSwapIn CellSwapIn;
    public event CellSwapOut CellSwapOut;

    public Rect SceneBound { get { return _root.Bound; } }
    public Vector3 FocusPoint { get { return _focusPoint; } }
    public bool EnableDebugLines { get; set; }

    private void DrawDebugLines()
    {
        QuadUtility.TraverseAllLeaves(_root, (leaf) => {
            Color c = Color.gray;

            if (leaf == _focusLeaf)
            {
                c = Color.blue;
            }
            else if (_swapInQueue.Contains(leaf))
            {
                c = Color.green;
            }
            else if (_swapOutQueue.Contains(leaf))
            {
                c = Color.red;
            }
            else if (_holdingLeaves.Contains(leaf))
            {
                c = Color.white;
            }
            QuadDebug.DrawRect(leaf.Bound, 0.1f, c, 0.1f); 
        });
    }

    private bool UpdateFocus(Vector2 focusPoint)
    {
        _focusPoint = focusPoint;

        QuadLeafNode newLeaf = QuadUtility.FindLeafRecursively(_root, _focusPoint);
        if (newLeaf == _focusLeaf)
            return false;

        if (FocusCellChanged != null)
            FocusCellChanged(_focusLeaf, newLeaf);

        _focusLeaf = newLeaf;
        return true;
    }

    private void PerformSwapInOut(QuadLeafNode activeLeaf)
    {
        // 1. the initial in/out leaves generation
        List<QuadLeafNode> inLeaves;
        List<QuadLeafNode> outLeaves;
        QuadUtility.GenerateSwappingLeaves(_root, activeLeaf, _holdingLeaves, out inLeaves, out outLeaves);

        // 2. filter out leaves which are already in the ideal states
        inLeaves.RemoveAll((leaf) => { return _swapInQueue.Contains(leaf); });
        
        // 3. append these new items to in/out queue
        SwapIn(inLeaves);
        SwapOut(outLeaves);
    }

    private void SwapIn(List<QuadLeafNode> inLeaves)
    {
        foreach (var leaf in inLeaves)
        {
            leaf.SwapIn();
            _swapInQueue.Add(leaf);
          
            if (CellSwapIn != null)
                CellSwapIn(leaf);
        }
    }

    private void SwapOut(List<QuadLeafNode> outLeaves)
    {
        foreach (var leaf in outLeaves)
        {
            leaf.SwapOut();
            _holdingLeaves.Remove(leaf);

            var affected = leaf.AffectedObjects;
            foreach (var item in affected)
            {
                if (!IsHoldingUserData(item))
                    item.SwapOut();
            }

            _swapOutQueue.Add(leaf);
            if (CellSwapOut != null)
                CellSwapOut(leaf);
        }
    }

    private bool IsHoldingUserData(QuadData userData)
    {
        foreach (var hLeaf in _holdingLeaves)
        {
            if (hLeaf.Contains(userData))
            {
                return true;
            }
        }

        return false;
    }

    private void ProcessSwapQueues()
    {
        foreach (var leaf in _swapInQueue)
        {
            if (leaf.IsSwapInCompleted())
            {
                _holdingLeaves.Add(leaf);
            }
        }

        _swapInQueue.RemoveAll((leaf) => { return _holdingLeaves.Contains(leaf); });
        _swapOutQueue.RemoveAll((leaf) => { return leaf.IsSwapOutCompleted(); });
    }
}

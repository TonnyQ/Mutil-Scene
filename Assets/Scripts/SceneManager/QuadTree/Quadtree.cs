using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void CellChanged(QtLeafNode left, QtLeafNode entered);
public delegate void CellSwapIn(QtLeafNode leaf);
public delegate void CellSwapOut(QtLeafNode leaf);

public static class QtSetting
{
    // this value determines the smallest cell size
    // the space-partition process would stop dividing if cell size is smaller than this value
    public static float CellSizeThreshold = 32.0f;

    // swap-in distance of cells
    public static float CellSwapInDist = 100.0f;
    // swap-out distance of cells 
    //  (would be larger than swap-in to prevent poping)
    public static float CellSwapOutDist = 150.0f;

    // time interval to update the focus point,
    // so that a new swap would potentially triggered (in seconds)
    public static float SwapTriggerInterval = 0.5f;
    // time interval to update the in/out swapping queues (in seconds)
    public static float SwapProcessInterval = 0.2f;
}

// user data stored in quadtree leaves
public interface QtData
{
    Vector3 GetCenter();
    Vector3 GetExtends();

    void SwapIn();
    void SwapOut();

    bool IsSwapInCompleted();
    bool IsSwapOutCompleted();
}

public class QtNode
{
    public QtNode(Rect bound)
    {
        _bound = bound;
    }

    public Rect Bound { get { return _bound; } }
    protected Rect _bound;

    public virtual void SetSubNodes(QtNode[] subNodes)
    {
        _subNodes = subNodes;
    }

    public virtual void Receive(QtData userData)
    {
        if (!QtUtility.Intersects(Bound, userData))
        {
            return;
        }

        foreach (var sub in SubNodes)
        {
            sub.Receive(userData);
        }
    }

    public QtNode[] SubNodes { get { return _subNodes; } }
    public const int SubCount = 4;
    protected QtNode[] _subNodes = null;
}

public class QtLeafNode : QtNode
{
    public List<QtData> AffectedObjects { get { return _affectedObjects; } }
    private List<QtData> _ownedObjects = new List<QtData>();
    private List<QtData> _affectedObjects = new List<QtData>();

    public QtLeafNode(Rect bound) : base(bound)
    {
    }

    public override void SetSubNodes(QtNode[] subNodes)
    {
        QtDebug.Assert(false);
    }

    public override void Receive(QtData userData)
    {
        if (!QtUtility.Intersects(Bound, userData))
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

    public bool Contains(QtData userData)
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
    private QtNode _root;

    /// <summary>
    /// current focus point.
    /// </summary>
    private Vector3 _focusPoint;

    /// <summary>
    /// curreny focus leaf node
    /// </summary>
    private QtLeafNode _focusLeaf;

    private List<QtLeafNode> _holdingLeaves = new List<QtLeafNode>();
    private List<QtLeafNode> _swapInQueue = new List<QtLeafNode>();
    private List<QtLeafNode> _swapOutQueue = new List<QtLeafNode>();

    private float _lastSwapTriggeredTime = 0.0f;
    private float _lastSwapProcessedTime = 0.0f;

    public QuadTree(Rect bound)
    {
        _root = new QtNode(bound);
        QtUtility.BuildRecursively(_root);
    }

    public void Update(Vector2 focusPoint)
    {
        if (EnableDebugLines)
        {
            DrawDebugLines();
        }

        if (Time.time - _lastSwapTriggeredTime > QtSetting.SwapTriggerInterval)
        {
            if (UpdateFocus(focusPoint))
                PerformSwapInOut(_focusLeaf);
            _lastSwapTriggeredTime = Time.time;
        }

        if (Time.time - _lastSwapProcessedTime > QtSetting.SwapProcessInterval)
        {
            ProcessSwapQueues();
            _lastSwapProcessedTime = Time.time;
        }
    }

    public void Receive(QtData qud)
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
        QtUtility.TraverseAllLeaves(_root, (leaf) => {
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
            QtDebug.DrawRect(leaf.Bound, 0.1f, c, 1.0f); 
        });
    }

    private bool UpdateFocus(Vector2 focusPoint)
    {
        _focusPoint = focusPoint;

        QtLeafNode newLeaf = QtUtility.FindLeafRecursively(_root, _focusPoint);
        if (newLeaf == _focusLeaf)
            return false;

        if (FocusCellChanged != null)
            FocusCellChanged(_focusLeaf, newLeaf);

        _focusLeaf = newLeaf;
        return true;
    }

    private void PerformSwapInOut(QtLeafNode activeLeaf)
    {
        // 1. the initial in/out leaves generation
        List<QtLeafNode> inLeaves;
        List<QtLeafNode> outLeaves;
        QtUtility.GenerateSwappingLeaves(_root, activeLeaf, _holdingLeaves, out inLeaves, out outLeaves);

        // 2. filter out leaves which are already in the ideal states
        inLeaves.RemoveAll((leaf) => { return _swapInQueue.Contains(leaf); });

        // 3. append these new items to in/out queue
        SwapIn(inLeaves);
        SwapOut(outLeaves);
    }

    private void SwapIn(List<QtLeafNode> inLeaves)
    {
        foreach (var leaf in inLeaves)
        {
            leaf.SwapIn();
            _swapInQueue.Add(leaf);
            if (CellSwapIn != null)
                CellSwapIn(leaf);
        }
    }

    private void SwapOut(List<QtLeafNode> outLeaves)
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

    private bool IsHoldingUserData(QtData userData)
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

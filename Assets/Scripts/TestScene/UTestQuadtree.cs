using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UTestQtUserData : IQtUserData
{
    public string resPath;
    public GameObject gameObject;
    public Bounds bounds;

    public Vector3 GetCenter() { return gameObject.transform.position; }
    public Vector3 GetExtends() { return bounds.extents; }

    public void SwapIn() { Renderer r = GetRenderer(); r.enabled = true; }
    public void SwapOut() { Renderer r = GetRenderer(); r.enabled = false; }

    public bool IsSwapInCompleted() { Renderer r = GetRenderer(); return r.enabled; }
    public bool IsSwapOutCompleted() { Renderer r = GetRenderer(); return !r.enabled; }

    public Renderer GetRenderer() { return gameObject.GetComponent<Renderer>() as Renderer; }
}

public class UTestPrototypes
{
    public static List<UTestQtUserData> Prototypes = new List<UTestQtUserData>();

    public static void Init()
    {
        GameObject go = GameObject.Find("Prototypes");
        if (go != null)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                GameObject prototype = go.transform.GetChild(i).gameObject;
                if (prototype != null)
                {
                    UTestQtUserData ud = new UTestQtUserData();
                    ud.resPath = go.name + '/' + prototype.name;
                    Prototypes.Add(ud);
                }
            }

            Debug.Log(go.transform.childCount.ToString() + " prototypes found.");
        }
    }

    public static UTestQtUserData NewRandom()
    {
        int i = Random.Range(0, Prototypes.Count - 1);
        GameObject go = GameObject.Find(Prototypes[i].resPath);
        if (go == null)
            return null;

        UTestQtUserData ud = new UTestQtUserData();
        ud.gameObject = Object.Instantiate(go) as GameObject;
        ud.resPath = Prototypes[i].resPath;
        ud.bounds = ud.gameObject.GetComponent<Renderer>().bounds; 
        return ud;
    }
}

public class UTestQuadtree
{
    public Vector2 Min = new Vector2(-300, -300);
    public Vector2 Max = new Vector2(300, 300);
    public Rect Bound { get { return new Rect(Min.x, Min.y, Max.x - Min.x, Max.y - Min.y); } }

    public bool Init()
    {
        UTestPrototypes.Init();

        m_quadtree = new UQuadtree(Bound);

        m_instRoot = GameObject.Find("Instances");
        if (m_instRoot == null)
            return false;

        for (int i = 0; i < 5000; i++)
        {
            UTestQtUserData ud = UTestPrototypes.NewRandom();
            if (ud != null)
            {
                ud.gameObject.transform.localPosition = NewRandomPoint();
                ud.gameObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                ud.gameObject.transform.localRotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
                ud.gameObject.transform.parent = m_instRoot.transform;
                ud.SwapOut();
                m_quadtree.Receive(ud);
            }
        }

        return true;
    }

    public void Update(Vector3 position)
    {
        m_quadtree.Update(new Vector2(position.x, position.z));
    }

    public Vector3 NewRandomPoint(float magnitude = 1.0f)
    {
        return new Vector3(
            Random.Range(Min.x, Max.x) * magnitude,
            0,
            Random.Range(Min.y, Max.y) * magnitude);
    }

    public UQuadtree Quadtree { get { return m_quadtree; } }

    GameObject m_instRoot = null;
    UQuadtree m_quadtree;
}


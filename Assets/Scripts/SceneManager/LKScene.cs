using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using SceneManager;

public class UTestQtUserData : IQtUserData
{
    public string resPath;
	public SceneItemData data;
    //public Bounds bounds;

	public UTestQtUserData(SceneItemData itemData)
	{
		data = itemData;
	}

	public Vector3 GetCenter() { return data.Position; }
	public Vector3 GetExtends() { return data.Position; }

    public void SwapIn() 
	{ 
		//Renderer r = GetRenderer(); r.enabled = true; 
	}
    public void SwapOut() 
	{ 
		//Renderer r = GetRenderer(); r.enabled = false; 
	}

    public bool IsSwapInCompleted() 
	{ 
		return false;
	}
    public bool IsSwapOutCompleted() 
	{ 
		return true;
	}

    public Renderer GetRenderer() 
	{
		return null;
	}
}

/// <summary>
/// LK scene class.
/// </summary>
public class LKScene
{
	private Scene curScene;
    public Vector2 Min = new Vector2(-256, -256);
    public Vector2 Max = new Vector2(256, 256);
    public Rect Bound { get { return new Rect(Min.x, Min.y, Max.x - Min.x, Max.y - Min.y); } }

    public bool Init()
    {
        m_quadtree = new UQuadtree(Bound);

        m_instRoot = GameObject.Find("Instances");
        if (m_instRoot == null)
            return false;

		List<SceneItemData> sceneItemDatas = ParseSceneConfig ("");
		int itemSize = sceneItemDatas.Count;
		for (int i = 0; i < itemSize; i++)
        {
			UTestQtUserData ud = new UTestQtUserData(sceneItemDatas[i]);
            if (ud != null)
            {
                //ud.SwapOut();
                m_quadtree.Receive(ud);
            }
        }

        return true;
    }

	/// <summary>
	/// Parses the scene config.
	/// </summary>
	/// <returns>The scene config.</returns>
	/// <param name="xmlStr">Xml string.</param>
	private List<SceneItemData> ParseSceneConfig(string xmlStr)
	{
		return null;
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


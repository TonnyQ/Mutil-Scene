using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using SceneManager;
using System.Xml;

public class UTestQtUserData : QuadData
{
	private GameObject parent;
	private GameObject testObj;
	private SceneItemData data;

	public UTestQtUserData(SceneItemData itemData,GameObject parent)
	{
		this.parent = parent;
		data = itemData;
	}

	public Vector3 GetCenter() { return data.Position; }
	public Vector3 GetExtends() { return data.Bound; }

    public void SwapIn() 
	{ 
		Load ();
	}
    public void SwapOut() 
	{ 
		Unload ();
	}

    public bool IsSwapInCompleted() 
	{ 
		if (testObj != null) {
			return true;
		}
		return false;
	}
    public bool IsSwapOutCompleted() 
	{ 
		if (testObj == null) {
			return true;
		}
		return false;
	}

	private void Load()
	{
		testObj = GameObject.Instantiate(Resources.Load (data.Path)) as GameObject;
        testObj.transform.localPosition = data.Position;
        testObj.transform.localRotation = data.Rotation;
        testObj.transform.localScale = data.Scale;
		testObj.transform.parent = parent.transform;
	}

	private void Unload()
	{
		if (testObj != null) {
			GameObject.Destroy (testObj);
		}
	}

}

/// <summary>
/// LK scene class.
/// </summary>
public class LKScene
{
	//private Scene curScene;
    public Vector2 Min = new Vector2(-256, -256);
    public Vector2 Max = new Vector2(256, 256);
    public Rect Bound { get { return new Rect(Min.x, Min.y, Max.x - Min.x, Max.y - Min.y); } }

    public bool Init()
    {
        m_quadtree = new QuadTree(Bound);

        m_instRoot = GameObject.Find("Environment");
        if (m_instRoot == null)
            return false;

		List<SceneItemData> sceneItemDatas = ParseSceneConfig ("");
		int itemSize = sceneItemDatas.Count;
		for (int i = 0; i < itemSize; i++)
        {
			UTestQtUserData ud = new UTestQtUserData(sceneItemDatas[i],m_instRoot);
            if (ud != null)
            {
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
		List<SceneItemData> sceneItemDatas = new List<SceneItemData> ();
		{
			string xmlPath = Application.dataPath + "/Config/scene_data_config.xml";

			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load (xmlPath);

			// 使用 XPATH 获取所有 gameObject 节点
			XmlNodeList xmlNodeList = xmlDocument.SelectNodes("//gameObject");
			foreach(XmlNode xmlNode in xmlNodeList)
			{
				SceneItemData itemData = new SceneItemData ();
				itemData.Name = xmlNode.Attributes["objectName"].Value;
				itemData.Path = xmlNode.Attributes["objectAsset"].Value;

				XmlNode boundXmlNode = xmlNode.SelectSingleNode ("descendant::bound");
				if (boundXmlNode != null) {
					itemData.Bound = new Vector3 (
						float.Parse(boundXmlNode.Attributes["x"].Value), 
						float.Parse(boundXmlNode.Attributes["y"].Value), 
						float.Parse(boundXmlNode.Attributes["z"].Value));
				}

				// 使用 XPATH 获取 位置、旋转、缩放数据
				XmlNode positionXmlNode = xmlNode.SelectSingleNode("descendant::position");
				XmlNode rotationXmlNode = xmlNode.SelectSingleNode("descendant::rotation");
				XmlNode scaleXmlNode = xmlNode.SelectSingleNode("descendant::scale");

				if(positionXmlNode != null && rotationXmlNode != null && scaleXmlNode != null)
				{
					itemData.Position = new Vector3(
						float.Parse(positionXmlNode.Attributes["x"].Value), 
						float.Parse(positionXmlNode.Attributes["y"].Value), 
						float.Parse(positionXmlNode.Attributes["z"].Value));
					itemData.Rotation = Quaternion.Euler(new Vector3(
						float.Parse(rotationXmlNode.Attributes["x"].Value), 
						float.Parse(rotationXmlNode.Attributes["y"].Value), 
						float.Parse(rotationXmlNode.Attributes["z"].Value)));
					itemData.Scale = new Vector3(
						float.Parse(scaleXmlNode.Attributes["x"].Value), 
						float.Parse(scaleXmlNode.Attributes["y"].Value), 
						float.Parse(scaleXmlNode.Attributes["z"].Value));
				}
				sceneItemDatas.Add (itemData);
			}
			xmlDocument = null;
		}
		return sceneItemDatas;
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

    public QuadTree Quadtree { get { return m_quadtree; } }

    GameObject m_instRoot = null;
    QuadTree m_quadtree;
}


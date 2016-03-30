using UnityEngine;        
using UnityEditor;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using Utility;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class ExportSceneData : Editor {

    [MenuItem("SceneTools/ExportXML")]
    static void ExportXML()
    {
        string path = EditorUtility.SaveFilePanel("Save Resource", "", "New Resource", "xml");
        if (string.IsNullOrEmpty(path))
        {
            EditorUtility.DisplayDialog("Error!!!","please select a valid file save path...","Retry","Cancel");
            return;
        }
        Scene curScene = EditorSceneManager.GetActiveScene(); 
        // 场景名称
        string sceneName = curScene.name;
        // 如果存在场景文件，删除
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        XmlDocument xmlDocument = new XmlDocument();  
        // 创建XML根标志
        XmlElement rootXmlElement = xmlDocument.CreateElement("root");
        rootXmlElement.SetAttribute("version", "1.0");
        // 创建场景标志
        XmlElement sceneXmlElement = xmlDocument.CreateElement("scene");
        sceneXmlElement.SetAttribute("sceneName", sceneName);

        List<GameObject> packObjs = FindAllNeedPackObjs();
        foreach (GameObject sceneObject in packObjs)
        {           
            // 判断是否是预设
            if (PrefabUtility.GetPrefabType(sceneObject) == PrefabType.PrefabInstance)
            {
                // 获取引用预设对象
                Object prefabObject = PrefabUtility.GetPrefabParent(sceneObject);
                if (prefabObject != null)
                {
                    XmlElement gameObjectXmlElement = xmlDocument.CreateElement("gameObject");
                    gameObjectXmlElement.SetAttribute("objectName", sceneObject.name);
                    gameObjectXmlElement.SetAttribute("objectAsset", prefabObject.name);

					//XmlElement boundXmlElement = xmlDocument.CreateElement("bounds")
						

					//transform
                    XmlElement transformXmlElement = xmlDocument.CreateElement("transform");

                    // 位置信息
                    XmlElement positionXmlElement = xmlDocument.CreateElement("position");
                    positionXmlElement.SetAttribute("x", sceneObject.transform.position.x.ToString());
                    positionXmlElement.SetAttribute("y", sceneObject.transform.position.y.ToString());
                    positionXmlElement.SetAttribute("z", sceneObject.transform.position.z.ToString());

                    // 旋转信息
                    XmlElement rotationXmlElement = xmlDocument.CreateElement("rotation");
                    rotationXmlElement.SetAttribute("x", sceneObject.transform.rotation.eulerAngles.x.ToString());
                    rotationXmlElement.SetAttribute("y", sceneObject.transform.rotation.eulerAngles.y.ToString());
                    rotationXmlElement.SetAttribute("z", sceneObject.transform.rotation.eulerAngles.z.ToString());

                    // 缩放信息
                    XmlElement scaleXmlElement = xmlDocument.CreateElement("scale");
                    scaleXmlElement.SetAttribute("x", sceneObject.transform.localScale.x.ToString());
                    scaleXmlElement.SetAttribute("y", sceneObject.transform.localScale.y.ToString());
                    scaleXmlElement.SetAttribute("z", sceneObject.transform.localScale.z.ToString());

                    transformXmlElement.AppendChild(positionXmlElement);
                    transformXmlElement.AppendChild(rotationXmlElement);
                    transformXmlElement.AppendChild(scaleXmlElement);

                    gameObjectXmlElement.AppendChild(transformXmlElement);
                    sceneXmlElement.AppendChild(gameObjectXmlElement);
                }
            }
        }
        rootXmlElement.AppendChild(sceneXmlElement);
        xmlDocument.AppendChild(rootXmlElement);
        // 保存场景数据
        xmlDocument.Save(path);
        // 刷新Project视图
        AssetDatabase.Refresh();

        //CreateRuntimeScene(curScene,curScene.path, Application.dataPath + @"/Scenes/" + curScene.name + ".unity");
    }

    [MenuItem("SceneTools/PutRandomScene")]
    static void PutRandomScene()
    {
        GameObject obj = null;
        Vector2 Min = new Vector2(-256, -256);
        Vector2 Max = new Vector2(256, 256);
        string[] prefabs = { "Cube", "Sphere", "Capsule", "Cylinder" };
        GameObject go = GameObject.Find("Environment");
        for (int i = 0; i < 500; i++)
        {
            if (go != null)
            {
                int index = i % 4;
                obj = PrefabUtility.InstantiatePrefab(Resources.Load(prefabs[index])) as GameObject;
                obj.transform.localPosition = NewRandomPoint(Min, Max);
                obj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                obj.transform.localRotation = Quaternion.Euler(0.0f, UnityEngine.Random.Range(0.0f, 360.0f), 0.0f);
                obj.transform.parent = go.transform;

            }
        }
    }

    static List<GameObject> FindAllNeedPackObjs()
	{
        List<GameObject> allObjs = new List<GameObject>();
        //StopTime分析工具	
        using (StopTime time = new StopTime("find"))
        {	
            GameObject go = GameObject.Find("Environment");
            if (go != null)
            {
                for (int i = 0; i < go.transform.childCount; i++)
                {
                    GameObject prototype = go.transform.GetChild(i).gameObject;
                    allObjs.Add(prototype);
                }
            }
        }
        return allObjs;
    }	

	static Vector3 NewRandomPoint(Vector2 min,Vector2 max,float magnitude = 1.0f)
	{
		return new Vector3(
			Random.Range(min.x, max.x) * magnitude,
			0,
			Random.Range(min.y, max.y) * magnitude);
	}

    static void CreateRuntimeScene(Scene curScene,string originFilePath,string copyFilePath)
    {
        //AssetDatabase.CopyAsset(originFilePath, copyFilePath);
        //AssetDatabase.Refresh();
        //EditorSceneManager.CloseScene(curScene, true);
        //EditorSceneManager.OpenScene(copyFilePath);
        //GameObject go = GameObject.Find("Environment");
        //GameObject.DestroyImmediate(go);
        //new GameObject("Environment");
        //EditorSceneManager.SaveOpenScenes();
    }
}

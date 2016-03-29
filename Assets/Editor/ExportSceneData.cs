using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using SceneManager;

public class ExportSceneData : Editor {

	private static SceneData sceneData = new SceneData();

	//将所有游戏场景导出为XML格式
	[MenuItem ("SceneTools/ExportXML")]
	static void ExportXML ()
		{
			string filepath = Application.dataPath + @"/StreamingAssets/my.xml";
			if(!File.Exists (filepath))
			{
				File.Delete(filepath);
			}
			XmlDocument xmlDoc = new XmlDocument();
			XmlElement root = xmlDoc.CreateElement("gameObjects");
			//遍历所有的游戏场景
			foreach (UnityEditor.EditorBuildSettingsScene S in UnityEditor.EditorBuildSettings.scenes)
			{
				//当关卡启用
				if (S.enabled)
				{
					//得到关卡的名称
					string name = S.path;
					//打开这个关卡
					EditorApplication.OpenScene(name);
					XmlElement scenes = xmlDoc.CreateElement("scenes");
					scenes.SetAttribute("name",name);
					foreach (GameObject obj in Object.FindObjectsOfType(typeof(GameObject)))
					{
						if (obj.transform.parent == null)
						{
							XmlElement gameObject = xmlDoc.CreateElement("gameObjects");
							gameObject.SetAttribute("name",obj.name);

							gameObject.SetAttribute("asset",obj.name + ".prefab");
							XmlElement transform = xmlDoc.CreateElement("transform");
							XmlElement position = xmlDoc.CreateElement("position");
							XmlElement position_x = xmlDoc.CreateElement("x");
							position_x.InnerText = obj.transform.position.x+"";
							XmlElement position_y = xmlDoc.CreateElement("y");
							position_y.InnerText = obj.transform.position.y+"";
							XmlElement position_z = xmlDoc.CreateElement("z");
							position_z.InnerText = obj.transform.position.z+"";
							position.AppendChild(position_x);
							position.AppendChild(position_y);
							position.AppendChild(position_z);

							XmlElement rotation = xmlDoc.CreateElement("rotation");
							XmlElement rotation_x = xmlDoc.CreateElement("x");
							rotation_x.InnerText = obj.transform.rotation.eulerAngles.x+"";
							XmlElement rotation_y = xmlDoc.CreateElement("y");
							rotation_y.InnerText = obj.transform.rotation.eulerAngles.y+"";
							XmlElement rotation_z = xmlDoc.CreateElement("z");
							rotation_z.InnerText = obj.transform.rotation.eulerAngles.z+"";
							rotation.AppendChild(rotation_x);
							rotation.AppendChild(rotation_y);
							rotation.AppendChild(rotation_z);

							XmlElement scale = xmlDoc.CreateElement("scale");
							XmlElement scale_x = xmlDoc.CreateElement("x");
							scale_x.InnerText = obj.transform.localScale.x+"";
							XmlElement scale_y = xmlDoc.CreateElement("y");
							scale_y.InnerText = obj.transform.localScale.y+"";
							XmlElement scale_z = xmlDoc.CreateElement("z");
							scale_z.InnerText = obj.transform.localScale.z+"";

							scale.AppendChild(scale_x);
							scale.AppendChild(scale_y);
							scale.AppendChild(scale_z);

							transform.AppendChild(position);
							transform.AppendChild(rotation);
							transform.AppendChild(scale);	

							gameObject.AppendChild(transform);
							scenes.AppendChild(gameObject);
							root.AppendChild(scenes);
							xmlDoc.AppendChild(root);
							xmlDoc.Save(filepath);

						}
					}
				}
			}
			//刷新Project视图， 不然需要手动刷新哦
			AssetDatabase.Refresh();
		}

	//将所有游戏场景导出为JSON格式
	[MenuItem ("SceneTools/ExportJSON")]
	static void ExportJSON ()
		{
			string filepath = Application.dataPath + @"/StreamingAssets/json.txt";
			FileInfo t = new FileInfo(filepath);
			if(!File.Exists (filepath))
			{
				File.Delete(filepath);
			}
			StreamWriter sw = t.CreateText();

			StringBuilder sb = new StringBuilder ();
			//JsonUtility. writer = new JsonWriter (sb);
			//writer.WriteObjectStart ();
			//writer.WritePropertyName ("GameObjects");
			//writer.WriteArrayStart ();

			foreach (UnityEditor.EditorBuildSettingsScene S in UnityEditor.EditorBuildSettings.scenes)
			{
				if (S.enabled)
				{
					string name = S.path;
					EditorApplication.OpenScene(name);
//					writer.WriteObjectStart();
//					writer.WritePropertyName("scenes");
//					writer.WriteArrayStart ();
//					writer.WriteObjectStart();
//					writer.WritePropertyName("name");
//					writer.Write(name);
//					writer.WritePropertyName("gameObject");
//					writer.WriteArrayStart ();

					foreach (GameObject obj in Object.FindObjectsOfType(typeof(GameObject)))
					{
						if (obj.transform.parent == null)
						{
//							writer.WriteObjectStart();
//							writer.WritePropertyName("name");
//							writer.Write(obj.name);
//
//							writer.WritePropertyName("position");
//							writer.WriteArrayStart ();
//							writer.WriteObjectStart();
//							writer.WritePropertyName("x");
//							writer.Write(obj.transform.position.x.ToString("F5"));
//							writer.WritePropertyName("y");
//							writer.Write(obj.transform.position.y.ToString("F5"));
//							writer.WritePropertyName("z");
//							writer.Write(obj.transform.position.z.ToString("F5"));
//							writer.WriteObjectEnd();
//							writer.WriteArrayEnd();
//
//							writer.WritePropertyName("rotation");
//							writer.WriteArrayStart ();
//							writer.WriteObjectStart();
//							writer.WritePropertyName("x");
//							writer.Write(obj.transform.rotation.eulerAngles.x.ToString("F5"));
//							writer.WritePropertyName("y");
//							writer.Write(obj.transform.rotation.eulerAngles.y.ToString("F5"));
//							writer.WritePropertyName("z");
//							writer.Write(obj.transform.rotation.eulerAngles.z.ToString("F5"));
//							writer.WriteObjectEnd();
//							writer.WriteArrayEnd();
//
//							writer.WritePropertyName("scale");
//							writer.WriteArrayStart ();
//							writer.WriteObjectStart();
//							writer.WritePropertyName("x");
//							writer.Write(obj.transform.localScale.x.ToString("F5"));
//							writer.WritePropertyName("y");
//							writer.Write(obj.transform.localScale.y.ToString("F5"));
//							writer.WritePropertyName("z");
//							writer.Write(obj.transform.localScale.z.ToString("F5"));
//							writer.WriteObjectEnd();
//							writer.WriteArrayEnd();
//
//							writer.WriteObjectEnd();
						}
					}

//					writer.WriteArrayEnd();
//					writer.WriteObjectEnd();
//					writer.WriteArrayEnd();
//					writer.WriteObjectEnd();
				}
			}
			//writer.WriteArrayEnd();
			//writer.WriteObjectEnd ();

			sw.WriteLine(sb.ToString());
			sw.Close();
			sw.Dispose();
			AssetDatabase.Refresh();
		}

	static void ExportSceneDataJson(string sceneName)
	{
		SceneData curScene = new SceneData ();

	}

	static List<GameObject> FindAllNeedPackObjs()
	{
		List<GameObject> allObjs = new List<GameObject> ();
		GameObject go = GameObject.Find("Envirnoment");
		if (go != null)
		{
			for (int i = 0; i < go.transform.childCount; i++)
			{
				GameObject prototype = go.transform.GetChild(i).gameObject;
				allObjs.Add(prototype);
			}
		}
		return allObjs;
	}

	[MenuItem("SceneTools/PutRandomScene")]
	static void PutRandomScene()
	{
		GameObject obj = null;
		Vector2 Min = new Vector2(-256, -256);
		Vector2 Max = new Vector2(256, 256);
		string[] prefabs = { "Cube", "Sphere", "Capsule", "Cylinder" };
		GameObject go = GameObject.Find ("Environment");
		for (int i = 0; i < 3000; i++)
		{
			if (go != null)
			{
				int index = i % 4;
				obj = GameObject.Instantiate(Resources.Load (prefabs [index])) as GameObject;
				obj.transform.localPosition = NewRandomPoint(Min,Max);
				obj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
				obj.transform.localRotation = Quaternion.Euler(0.0f, UnityEngine.Random.Range(0.0f, 360.0f), 0.0f);
				obj.transform.parent = go.transform;

			}
		}
	}

	public static Vector3 NewRandomPoint(Vector2 min,Vector2 max,float magnitude = 1.0f)
	{
		return new Vector3(
			Random.Range(min.x, max.x) * magnitude,
			0,
			Random.Range(min.y, max.y) * magnitude);
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace SceneManager
{
	public sealed class SceneData
	{
		private string name;

		public string Name {
			get {
				return name;
			}
			set {
				name = value;
			}
		}

		private string type;

		public string Type {
			get {
				return type;
			}
			set {
				type = value;
			}
		}

		private List<SceneItemData> sceneItemDatas;

		public List<SceneItemData> SceneItemDatas {
			get {
				return sceneItemDatas;
			}
			set {
				sceneItemDatas = value;
			}
		}
	}

	public sealed class SceneItemData
	{
		private string name;

		public string Name {
			get {
				return name;
			}
			set {
				name = value;
			}
		}

		private string path;

		public string Path {
			get {
				return path;
			}
			set {
				path = value;
			}
		}

		private string type;

		public string Type {
			get {
				return type;
			}
			set {
				type = value;
			}
		}

		private int weight;

		public int Weight {
			get {
				return weight;
			}
			set{
				weight = value;
			}
		}

		private Vector3 position;

		public Vector3 Position {
			get {
				return position;
			}
			set {
				position = value;
			}
		}

		private Quaternion rotation;

		public Quaternion Rotation {
			get {
				return rotation;
			}
			set {
				rotation = value;
			}
		}

		private Vector3 scale;

		public Vector3 Scale {
			get {
				return scale;
			}
			set {
				scale = value;
			}
		}

		public SceneItemData ()
		{
			
		}
	}
}


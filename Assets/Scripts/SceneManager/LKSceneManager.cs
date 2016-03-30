using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using SceneManager;

public class LKSceneManager : Singleton<LKSceneManager>
{
	public delegate bool LoadSceneComplete();
	private LoadSceneComplete loadComplete = null;

	private bool isLoadingScene = false;

	/// <summary>
	/// Gets the scene config.
	/// </summary>
	/// <returns>The scene config.</returns>
	/// <param name="scene">Scene.</param>
	public IList<SceneItemData> GetSceneConfig(string scene)
	{
		return null;
	}

	/// <summary>
	/// Loads all scene config.
	/// </summary>
	/// <returns><c>true</c>, if all scene config was loaded, <c>false</c> otherwise.</returns>
	/// <param name="config">Config.</param>
	public bool LoadAllSceneConfig(string config)
	{
		return false;
	}

	public bool LoadScene(string name,LoadSceneComplete callback)
	{
		return false;
	}

	public bool LoadSceneAsync(string name,LoadSceneComplete callback)
	{
		loadComplete = callback;
		return true;
	}
}

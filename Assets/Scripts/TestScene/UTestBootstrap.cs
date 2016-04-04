using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class UTestBootstrap : MonoBehaviour
{
	LKScene m_testbed = new LKScene();

    public GameObject m_player = null;         

    bool _drawDebugLines = false;

    void Start()
    {
        m_testbed.Init();                  
    }

    void Update()
    {
        if (_drawDebugLines)
            QuadDebug.DrawRect(m_testbed.Bound, 0.1f, Color.white);

        m_testbed.Update(m_player.transform.position);
    }

    void OnGUI()
    {
        _drawDebugLines = GUI.Toggle(new Rect(50, Screen.height * 0.25f, 100, 20), _drawDebugLines, "Debug Lines");

        if (_drawDebugLines != m_testbed.Quadtree.EnableDebugLines)
            m_testbed.Quadtree.EnableDebugLines = _drawDebugLines;
    }
}

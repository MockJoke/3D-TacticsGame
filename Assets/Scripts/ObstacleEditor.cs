using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ObstacleEditor : EditorWindow
{
    private static int cols = 10;
    private static int rows = 10;

    [MenuItem("Tools/Obstacle Spawner")]
    public static void ShowWindow()
    {
        GetWindow(typeof(ObstacleEditor));
    }

    private void OnGUI()
    {
        GUILayout.Label("Generate Obstacle", EditorStyles.boldLabel);

        for (int i = cols - 1; i >= 0; i--)
        {
            GUILayout.BeginHorizontal();
            for (int j = 0; j < rows; j++)
            {
                if (GUILayout.Button(j.ToString() + "," + i.ToString()))
                {
                    ObstacleManager.instance.GenerateObstacle(j,i);
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}

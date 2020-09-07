﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RoomWindow : EditorWindow
{
    Room currentRoom = null;
    int selected = 0;
    Vector2 scrollPosition;
    int settingsHeight = 60;
    int widthBuffer = 5;
    int heightBuffer = 5;
    Color sectionColor = new Color(.7f, .7f, .7f);
    Color subsectionColor = new Color(.8f, .8f, .8f);

    [MenuItem("Tools/Layout Editor")]
    public static void DrawWindow()
    {
        GetWindow<RoomWindow>("Room Editor");
    }

    private void Awake()
    {
        minSize = new Vector2(520 + widthBuffer*2, 200 + heightBuffer*3);

        selected = Selection.transforms.Length;

        if (selected > 0 && Selection.transforms[0].root.GetComponent<Room>())
        {
            currentRoom = Selection.transforms[0].root.GetComponent<Room>();
        }

        Repaint();
    }

    private void OnSelectionChange()
    {
        selected = Selection.transforms.Length;

        if (selected > 0 && Selection.transforms[0].root.GetComponent<Room>())
        {
            currentRoom = Selection.transforms[0].root.GetComponent<Room>();
        }
        
        Repaint();
    }

    private void OnGUI()
    {
        DrawRoomEditor();
    }

    /// <summary>
    /// Draws the overall editor window
    /// </summary>
    private void DrawRoomEditor()
    {
        if (currentRoom != null)
        {
            DrawRoomSettings();
            DrawLayoutSettings();
        }
    }

    /// <summary>
    /// Draws the room settings area
    /// </summary>
    private void DrawRoomSettings()
    {
        GUILayout.BeginArea(new Rect(widthBuffer, heightBuffer, position.width - widthBuffer * 2, settingsHeight + heightBuffer));
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, sectionColor);
        texture.Apply();
        GUI.DrawTexture(new Rect(0, 0, position.width, settingsHeight), texture);

        GUILayout.Box("Room Settings", EditorStyles.boldLabel);
        currentRoom.name = EditorGUILayout.TextField(currentRoom.name);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Entrance: " + (currentRoom.Entrance == null ? "Not Set" : currentRoom.Entrance.name)))
        {
            if (selected == 1)
                currentRoom.Entrance = Selection.transforms[0];
        }
        if (GUILayout.Button("Exit: " + (currentRoom.Exit == null ? "Not Set" : currentRoom.Exit.name)))
        {
            if (selected == 1)
                currentRoom.Exit = Selection.transforms[0];
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    /// <summary>
    /// Draws the overall layout settings and layouts area
    /// </summary>
    private void DrawLayoutSettings()
    {
        GUILayout.BeginArea(new Rect(widthBuffer, settingsHeight + heightBuffer * 2, position.width - widthBuffer * 2, position.height + heightBuffer * 2));
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, sectionColor);
        texture.Apply();
        GUI.DrawTexture(new Rect(0, 0, position.width, position.height - settingsHeight - heightBuffer*3), texture);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Box("Layouts", EditorStyles.boldLabel);
        if (GUILayout.Button("Add New Layout"))
        {
            currentRoom.Layouts.Add(new Room.Layout());
        }
        EditorGUILayout.EndHorizontal();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width - widthBuffer*2), GUILayout.Height(position.height - settingsHeight - heightBuffer*7));
        foreach (Room.Layout layout in currentRoom.Layouts)
        {
            if (DrawLayout(layout)) break;
            EditorGUILayout.Space(4);
        }
        EditorGUILayout.EndScrollView();

        GUILayout.EndArea();
    }

    /// <summary>
    /// Draws everything needed for an individual layout
    /// </summary>
    /// <param name="layout"></param>
    /// <returns> break out of loop? </returns>
    private bool DrawLayout(Room.Layout layout)
    {
        /*Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, subsectionColor);
        texture.Apply();
        GUI.DrawTexture(new Rect(0, 0, position.width, position.height - settingsHeight - heightBuffer * 3), texture);*/

        EditorGUILayout.BeginHorizontal();
        layout.dropdownInEditor = EditorGUILayout.BeginFoldoutHeaderGroup(layout.dropdownInEditor, ""+layout.objects.Count);
        layout.name = EditorGUILayout.TextField(layout.name);

        if (GUILayout.Button("Add Selected Objects to Layout"))
        {
            currentRoom.AddObjectsToLayout(currentRoom.Layouts.IndexOf(layout), Selection.transforms);
        }

        if (GUILayout.Button(layout.hidden ? "Shown" : "Hidden"))
        {
            layout.hidden = !layout.hidden;
            foreach (Transform obj in layout.objects)
            {
                obj.gameObject.SetActive(layout.hidden);
            }
        }

        if (GUILayout.Button("Select All"))
        {
            Object[] objs = new Object[layout.objects.Count];
            for (int i = 0; i < layout.objects.Count; i++)
            {
                objs[i] = layout.objects[i].gameObject;
            }
            Selection.objects = objs;
        }

        if (GUILayout.Button("Delete Layout"))
        {
            currentRoom.Layouts.Remove(layout);
            return true;
        }

        EditorGUILayout.EndHorizontal();

        if (layout.dropdownInEditor)
        {
            layout.difficulty = EditorGUILayout.IntSlider("Difficulty", layout.difficulty, -3, 15);

            List<Transform> toRemove = new List<Transform>();
            foreach (Transform obj in layout.objects)
            {
                if (obj == null)
                {
                    toRemove.Add(obj);
                    continue;
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Box(obj.name, GUILayout.Width(100));
                if (GUILayout.Button("Select"))
                {
                    Object[] selected = { obj.gameObject };
                    Selection.objects = selected;
                }
                if (GUILayout.Button("Remove"))
                {
                    toRemove.Add(obj);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(2);
            }
            if (toRemove.Count > 0)
            {
                layout.objects.RemoveAll(obj => toRemove.Contains(obj));
            }
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
        return false;
    }
}
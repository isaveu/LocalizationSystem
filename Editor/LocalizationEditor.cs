﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Subtegral.Localization.Controller;
using System.IO;
using UnityEditorInternal;
using System.Linq;
namespace Subtegral.Localization.EditorScripts
{
    public class LocalizationEditor : EditorWindow
    {
        [MenuItem("Localization/Editor")]
        public static void OpenWindow()
        {
            GetWindow<LocalizationEditor>("Localization Editor");
        }

        private void OnEnable()
        {
            if (manager != null)
                return;
            manager = Resources.Load<LocalizationManager>("Localization/Manager");
            if (manager == null)
            {
                if (!Directory.Exists("Assets/Resources"))
                    Directory.CreateDirectory("Assets/Resources");
                manager = ScriptableObject.CreateInstance<LocalizationManager>();
                AssetDatabase.CreateAsset(manager, "Assets/Resources/Localization/Manager.asset");
                AssetDatabase.SaveAssets();
            }


        }
        private void OnFocus()
        {
            CreateLanguageReordableList();
            RegisterReordableListCallbacks();
        }


        private void OnDisable()
        {
            if (dataWindow != null)
                dataWindow.Close();
        }

        private void OnGUI()
        {
            WaitForReordableListSpritePickCommands();
            EditorGUILayout.BeginVertical();
            DrawLanguageUI();
            DrawWordUI();
            EditorGUILayout.EndVertical();
        }

        #region GUI Drawers
        private void DrawWordUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            if (GUILayout.Button("NEW WORD"))
            {
                dataWindow = GetWindow<LocalizationDataWindow>("Add Word");
                dataWindow.SetWindowMode(WindowMode.AddWord);
            }

            EditorGUILayout.EndHorizontal();
            foreach (var oneKey in manager.FetchKeyList())
            {
                EditorGUILayout.LabelField(oneKey);
            }
        }

        private void DrawLanguageUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("LANGUAGE CONTROLS", EditorStyles.largeLabel);
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("NEW LANGUAGE"))
            {
                dataWindow = GetWindow<LocalizationDataWindow>("Add Language");
                dataWindow.SetWindowMode(WindowMode.AddLang);
            }
            EditorGUILayout.Space();

            list.DoLayoutList();
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region Reordable list helpers
        private void CreateLanguageReordableList()
        {
            list = new ReorderableList(manager.localizationLangs.Select((x) => x.Name).ToList(), typeof(string), true, true, false, true);
        }

        private void RegisterReordableListCallbacks()
        {
            list.onRemoveCallback = (ReorderableList list) =>
            {
                manager.localizationLangs.RemoveAt(list.index);
                CreateLanguageReordableList();
                RegisterReordableListCallbacks();
            };
            list.onReorderCallbackWithDetails = (ReorderableList list, int oldIndex, int newIndex) =>
            {
                //TO-DO REFATOR FOR CLASS STRUCTURE
                Language baseLang = manager.localizationLangs[oldIndex];
                Language targetLang = manager.localizationLangs[newIndex];
                manager.localizationLangs[oldIndex] = targetLang;
                manager.localizationLangs[newIndex] = baseLang;
            };
            list.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Languages");
            };
            list.drawElementCallback = (rect, index, isActive, isFocus) =>
            {
                float gapValue = 0f;
                if (manager.localizationLangs[index].Icon != null)
                {
                    EditorGUI.DrawPreviewTexture(new Rect(rect.x, rect.y, 20, 20), manager.localizationLangs[index].Icon.texture);
                    gapValue = 21f;
                }
                if (editLangIndex != -1 && editLangIndex == index)
                {
                    manager.localizationLangs[index].Name = EditorGUI.TextField(new Rect(rect.x + gapValue, rect.y, rect.width - 121 - gapValue, rect.height), manager.localizationLangs[index].Name);
                }
                else
                    EditorGUI.LabelField(new Rect(rect.x + gapValue, rect.y, rect.width, rect.height), manager.localizationLangs[index].Name);
                if (editLangIndex == index)
                    GUI.color = Color.gray;
                if (GUI.Button(new Rect(rect.x + rect.width - 120, rect.y, 60, 15), "Edit Text"))
                {
                    if (editLangIndex == index)
                        editLangIndex = -1;
                    else
                        editLangIndex = index;
                }
                GUI.color = Color.white;
                if (GUI.Button(new Rect(rect.x + rect.width - 59, rect.y, 60, 15), "Edit Icon"))
                {
                    EditorGUIUtility.ShowObjectPicker<Sprite>(manager.localizationLangs[index].Icon, false, "", 0);
                    editLangIconIndex = index;
                }
            };

        }

        private void WaitForReordableListSpritePickCommands()
        {
            if (editLangIconIndex != -1)
            {
                switch (Event.current.commandName)
                {
                    case "ObjectSelectorUpdated":
                        manager.localizationLangs[editLangIconIndex].Icon = (Sprite)EditorGUIUtility.GetObjectPickerObject();
                        break;
                    case "ObjectSelectorClosed":
                        editLangIconIndex = -1;
                        break;
                }
            }
        }
        #endregion

        ReorderableList list;
        LocalizationManager manager;
        LocalizationDataWindow dataWindow;
        int editLangIconIndex = -1;
        int editLangIndex = -1;
        bool isWordListOpen = false;
    }

}
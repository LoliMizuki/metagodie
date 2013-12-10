using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using MZSprites;

[CustomEditor(typeof(MZSpritesheetsViewerInEditor))]
public class MZSpritesheetsViewerInEditorInsceptor : Editor {

    public override void OnInspectorGUI() {
		LayoutManage();
        LayoutAtlasesList();
    }

	void LayoutManage() {
		if(GUILayout.Button("Refresh")) {
			_spritesheetsViewerInEditor.ResetEditorState();
			_spritesheetsViewerInEditor.CreateSpritesheetsFramesFromDefaultFolder();
		}

		if(GUILayout.Button("Clear Test Nodes")) {
			_spritesheetsViewerInEditor.ClearAllTestNode();
		}
	}

    void LayoutAtlasesList() {
		GUILayout.Label("Atlases");

        if(_spritesheetsViewerInEditor.framesSetByTextureName == null) {
            return;
        }

		EditorGUILayout.BeginVertical();

        foreach(string textureName in _spritesheetsViewerInEditor.framesSetByTextureName.Keys) {

			LayoutAtlasInfo(textureName);

			_spritesheetsViewerInEditor.expandsByName[textureName] = 
				EditorGUILayout.Foldout(_spritesheetsViewerInEditor.expandsByName[textureName], "Frames");

            LayoutSpritesFrameInTexture(textureName);
        }

		EditorGUILayout.EndVertical();
    }

	void LayoutAtlasInfo(string textureName) {
		Texture2D texture = _spritesheetsViewerInEditor.framesSetByTextureName[textureName].texture;
		if(texture == null) {
			return;
		}

		EditorGUILayout.BeginHorizontal();

		float largeSize = 60;
		MZGUIDrawHelper.DrawTextureOnCurrentLayout(texture, largeSize, largeSize, 2, "");

		EditorGUILayout.HelpBox(string.Format("Name: {0}\nSize: {1}x{2}\nFormat: {3}\nFilterMode: {4}\nWrapMode: {5}\nNumber of Frames: {6}",
		                                      textureName, 
		                                      texture.width,
		                                      texture.height,
		                                      texture.format,
		                                      texture.filterMode,
		                                      texture.wrapMode,
		                                      _spritesheetsViewerInEditor.framesSetByTextureName[textureName].count),
		                        MessageType.Info);

		EditorGUILayout.EndHorizontal();
	}

    void LayoutSpritesFrameInTexture(string textureName) {
        if(_spritesheetsViewerInEditor.expandsByName[textureName] == false) {
            return;
        }

        float edge = 25;
        float offset = 4;
        
        MZFramesSet framesSet = _spritesheetsViewerInEditor.framesSetByTextureName[textureName];

        foreach(string frameInfoName in framesSet.frameInfoNames) {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("|-");
            MZFrameInfo frameInfo = framesSet[frameInfoName];
            MZGUIDrawHelper.DrawFrameOnCurrentLayout(frameInfo, edge, edge, offset, "" );


            if(GUILayout.Button("Put Node")) {
				NewSpriteFromFrameInfo(frameInfo);
			}

			EditorGUILayout.LabelField(frameInfoName + "{"+ frameInfo.textureRect.ToString() +"}");

            EditorGUILayout.EndHorizontal();
        }
    }
    
    MZSpritesheetsViewerInEditor _spritesheetsViewerInEditor;
    
    void OnEnable() {
        _spritesheetsViewerInEditor = target as MZSpritesheetsViewerInEditor;
    }

	void NewSpriteFromFrameInfo(MZFrameInfo frameInfo) {
		GameObject spriteObject = new GameObject();

		spriteObject.transform.parent = _spritesheetsViewerInEditor.SpriteNodesParent;
		spriteObject.transform.localScale = new Vector3(100, 100, 0);
		spriteObject.name = frameInfo.name;
	
		SpriteRenderer spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
		spriteRenderer.sprite = frameInfo.sprite;
	}
}

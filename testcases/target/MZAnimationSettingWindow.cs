#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using MZSprites;

public class MZAnimationSettingWindow : EditorWindow {
    
    public MZAnimationSet animationSet;
    
    public void ShowWithAnimationSet(MZAnimationSet aniSet, Action<MZAnimationSet> lostFocusCallback) {
        if(aniSet == null) {
            return;
        }
        
        animationSet = aniSet;
        title = "Animation Setting";

		_lostFocusCallback = lostFocusCallback;
        
        _scrollPositionOfAnimationFrames = Vector2.zero;
        _foldoutAnimationFrames = true;
		_currentFocusAnimationFrameIndex = -1;

        _scrollPositionOfFramesInTexture = Vector2.zero;
        _foldoutAllFramesInTexture = true;

		// init cursorOutline texture
		CreateCursorOutlineTextureOnce();

        Show();
        Repaint();
    }

	void OnLostFocus() {
		if(_lostFocusCallback != null) {
			_lostFocusCallback(animationSet);
		}

		_lostFocusCallback = null;
	}
    
    #region - priavte
	Action<MZAnimationSet> _lostFocusCallback;

	// Animation Frames
	Vector2 _scrollPositionOfAnimationFrames;
    bool _foldoutAnimationFrames;
	int _currentFocusAnimationFrameIndex;
	
	// Frames In Texture
	Vector2 _scrollPositionOfFramesInTexture;
    bool _foldoutAllFramesInTexture;
    Texture2D _cursorOutline = null;

	void CreateCursorOutlineTextureOnce() {
		_cursorOutline = new Texture2D(64, 64);
		float thickness = 2;
		for(int x = 0; x < _cursorOutline.width; x++) {
			for(int y = 0; y < _cursorOutline.height; y++) {
				if(x <= thickness || x >= _cursorOutline.width - thickness ||
				   y <= thickness || y >= _cursorOutline.height - thickness) {
					_cursorOutline.SetPixel(x, y, Color.green);
				} else {
					_cursorOutline.SetPixel(x, y, new Color(0,0,0,0));
				}
			}
		}
		_cursorOutline.Apply();
	}
    
    void OnGUI() {
        if(animationSet == null) {
            return;
        }
        
        animationSet.name = EditorGUILayout.TextField("Name", animationSet.name);
        EditorGUILayout.Space();
        
        LayoutSpritesheetSelect();
        EditorGUILayout.Space();
        
        animationSet.oneLoopTime = EditorGUILayout.FloatField("One Loop Time", animationSet.oneLoopTime);
        EditorGUILayout.Space();
        
		LayoutAllFramesInTexture();
        EditorGUILayout.Space();

		LayoutAnimationFrames();
    }
    
    void LayoutSpritesheetSelect() {
        EditorGUILayout.BeginVertical();
        
        GUILayout.Label("Source Texture: " + ((animationSet.textureName != null)? animationSet.textureName : "<null>"));
        
        MZGUIDrawHelper.DrawTextureOnCurrentLayout(animationSet.texture, 80, 80, 0, "");
        
        Texture2D selectedTexture = EditorGUILayout.ObjectField(animationSet.texture, typeof(Texture2D), false, GUILayout.Width(80)) as Texture2D;
        if(selectedTexture != animationSet.texture) {
			if(MZFramesCollection.instance.framesSetByTextureName.ContainsKey(selectedTexture.name) == false) {
				EditorUtility.DisplayDialog("Oh No!!! This texture seem don't have spritesheet define", 
				                            "Make sure and pick up corrent texture :D", "Ok");
				animationSet.textureName = null;
			} else {
				animationSet.textureName = selectedTexture.name;
			}
        }
        
        EditorGUILayout.EndVertical();
    }
    
    void LayoutAnimationFrames() {
        _foldoutAnimationFrames = EditorGUILayout.Foldout(_foldoutAnimationFrames, "Animation");
        if(_foldoutAnimationFrames == false) {
            return;
        }

		EditorGUILayout.BeginHorizontal();

		GUILayoutOption buttonWidth = GUILayout.Width(60);

		if(GUILayout.Button("Delete", buttonWidth) ) {
			if(0<= _currentFocusAnimationFrameIndex && _currentFocusAnimationFrameIndex < animationSet.frameInfos.Count) {
				animationSet.frameInfos.RemoveAt(_currentFocusAnimationFrameIndex);
				_currentFocusAnimationFrameIndex = _currentFocusAnimationFrameIndex - 1;
				return;
			}
		}

		if(GUILayout.Button("<<", buttonWidth)) { Debug.Log("Coming Soon"); }
		if(GUILayout.Button(">>", buttonWidth)) { Debug.Log("Coming Soon"); }

		EditorGUILayout.EndHorizontal();
        
        if(animationSet.frameInfos == null || animationSet.frameInfos.Count <= 0) {
            return;
        }
        
        _scrollPositionOfAnimationFrames = EditorGUILayout.BeginScrollView(_scrollPositionOfAnimationFrames, true, false);
		LayuotFrameButtons(animationSet.frameInfos.ToArray(), 6, new Vector2(60, 60),  
		                   (MZFrameInfo frame, int index) => { return index == _currentFocusAnimationFrameIndex; }, // define when to draw outline. 	
						   (MZFrameInfo frame, int index) => { _currentFocusAnimationFrameIndex = index; }); // set focus.
                        
        EditorGUILayout.EndScrollView();
    }
    
    void LayoutAllFramesInTexture() {
        _foldoutAllFramesInTexture = EditorGUILayout.Foldout(_foldoutAllFramesInTexture, "Frames");
        if(_foldoutAllFramesInTexture == false) {
            return;
        }
		if(animationSet.textureName == null || MZFramesCollection.instance.framesSetByTextureName.ContainsKey(animationSet.textureName) == false) {
			return;
        }
        
        MZFramesSet framesSet = MZFramesCollection.instance.framesSetByTextureName[animationSet.textureName];
        List<MZFrameInfo> frames = new List<MZFrameInfo>();
        foreach(string frameName in framesSet.frameInfoNames) {
            frames.Add(framesSet.FrameInfoWithName(frameName));
        }

        int maxRow = 6;
        Vector2 size = new Vector2(40, 40);
        Action<MZFrameInfo, int> addNewAnimationFrameAction = (MZFrameInfo frame, int index) => {
			if(animationSet.frameInfos.Count == 0) {
            	animationSet.frameInfos.Add(frame);
				_currentFocusAnimationFrameIndex = 0;
			} else {
				animationSet.frameInfos.Insert(_currentFocusAnimationFrameIndex + 1, frame);
				_currentFocusAnimationFrameIndex = _currentFocusAnimationFrameIndex + 1;
			}
        };

        _scrollPositionOfFramesInTexture = EditorGUILayout.BeginScrollView(_scrollPositionOfFramesInTexture, true, false);
        LayuotFrameButtons(frames.ToArray(), maxRow, size, null, addNewAnimationFrameAction);
        EditorGUILayout.EndScrollView();
    }

	void LayuotFrameButtons(MZFrameInfo[] frameInfos, int maxRow, Vector2 size, 
	                        Func<MZFrameInfo, int, bool> isDrawOutline, 
	                        Action<MZFrameInfo, int> action) {
        EditorGUILayout.BeginVertical();

        for(int col = 0; col < frameInfos.Length/maxRow + 1; col++) {
            EditorGUILayout.BeginHorizontal();
            
            for(int row = 0; row < maxRow; row++) {
                int index = col*maxRow + row;
                if(index >= frameInfos.Length) {
                    break;
                }

                MZFrameInfo frame = frameInfos[index];
                
				if(GUILayout.Button("", GUILayout.Width(size.x), GUILayout.Height(size.y)) && action != null) {
					action(frame, index);
				}
                
				Texture2D outlineTexture = (isDrawOutline != null && isDrawOutline(frame, index))? _cursorOutline : null;

                Rect lastRect = GUILayoutUtility.GetLastRect();
				MZGUIDrawHelper.DrawFrameToRect(frame, lastRect, 4, "", outlineTexture);
            }
            
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }
	    
    #endregion
}

#endif
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using MiniJSON;
using MZSprites;

[CustomEditor(typeof(MZAnimationsEditor))]
public class MZAnimationsEditorInsceptor : Editor {

    public override void OnInspectorGUI() {
        LayoutFileManage();
        EditorGUILayout.Space();

        LayoutAnimationSets();
    }

    #region private

    MZAnimationsEditor _animationsEditor;
    MZAnimationSettingWindow _animationSettingWindow;
    List<string> _toggleEnableListOfAnimationSet;
    bool _foldoutAnimationsList;

    void OnEnable() {
        _animationsEditor = target as MZAnimationsEditor;
        _foldoutAnimationsList = true;
    }

    void LayoutFileManage() {
        GUILayout.Label(_animationsEditor.currentFileName);

        EditorGUILayout.BeginHorizontal();

        if(GUILayout.Button("Load")) { 
            _animationsEditor.Load();
        }

        if(GUILayout.Button("Unload")) {
            _animationsEditor.UnloadAndResetWorkingPath();
        }

        if(GUILayout.Button("Save")) { 
            _animationsEditor.Save();
        }

        if(GUILayout.Button("Save as")) { 
            _animationsEditor.SaveAs();
        }

        EditorGUILayout.EndHorizontal();
    }

    void LayoutAnimationsListManage() {
        GUILayoutOption width = GUILayout.Width(60);

        EditorGUILayout.BeginHorizontal();

        if(GUILayout.Button("Add", width)) {
            MZAnimationSet newAnimationSet = new MZAnimationSet();
            newAnimationSet.name = NewAnimationSetName();
            newAnimationSet.oneLoopTime = 1;

            MZAnimationSetsCollection.instance.AddAnimationSet(newAnimationSet.name, newAnimationSet);
        }

		if(GUILayout.Button("Delete", width)) {
			if(_toggleEnableListOfAnimationSet.Count > 0) {
				foreach(string animationSetName in _toggleEnableListOfAnimationSet) {
					_animationsEditor.animationSetsByName.Remove(animationSetName);
				}
				_toggleEnableListOfAnimationSet.Clear();
			}
        }

		if(GUILayout.Button("Auto Generate", GUILayout.Width(120))) {
			_animationsEditor.AutoGenerateAnimaiton();
		}

        EditorGUILayout.EndHorizontal();
    }

    string NewAnimationSetName() {
        int countOfDefaultName = 0;
        
        if(MZAnimationSetsCollection.instance.animationSetByName != null) {
            foreach(string name in MZAnimationSetsCollection.instance.animationSetByName.Keys) {
                if(System.Text.RegularExpressions.Regex.IsMatch(name, "^New[0-9]*$")) {
                    countOfDefaultName++;
                }
            }
        }

        return "New" + ((countOfDefaultName == 0)? "" : countOfDefaultName.ToString());
    }

	void LayoutAnimationSets() {
        _foldoutAnimationsList = EditorGUILayout.Foldout(_foldoutAnimationsList, "Animations");

        if(_foldoutAnimationsList == false) {
            return;
        }

        LayoutAnimationsListManage();

        if(_animationsEditor.animationSetsByName == null) {
            return;
        }

        foreach(string name in _animationsEditor.animationSetsByName.Keys) {
            bool continueIteration = LayoutAnimationSetAndCheckContinue(name, _animationsEditor.animationSetsByName[name]);
            if(continueIteration == false) {
                break;
            }
        }
    }

    bool LayoutAnimationSetAndCheckContinue(string name, MZAnimationSet animationSet) {
        EditorGUILayout.BeginHorizontal();

		bool nextToggleState = EditorGUILayout.Toggle(ToggleStateFromName(name));
		SetToggleStateWithName(name, nextToggleState);
       
        EditorGUILayout.LabelField(name);

        if(GUILayout.Button("Detail")) {
            ShowAnimationSettingWindowWithAnimationSet(animationSet);
        }

        if(GUILayout.Button("Put Node")) {
            _animationsEditor.PutAnimationNodeToSceneWithSetName(name);
        }

        if(GUILayout.Button("Delete")) {
            _animationsEditor.animationSetsByName.Remove(name);
            return false;
        }

        EditorGUILayout.EndHorizontal();

        return true;
    }

    void SetToggleStateWithName(string name, bool state) {
        if(_toggleEnableListOfAnimationSet == null) {
            _toggleEnableListOfAnimationSet = new List<string>();
        }

        if(state) {
			if(_toggleEnableListOfAnimationSet.Contains(name) == false) {
            	_toggleEnableListOfAnimationSet.Add(name);
			}
        } else {
			if(_toggleEnableListOfAnimationSet.Contains(name) == true) {
            	_toggleEnableListOfAnimationSet.Remove(name);
			}
        }
    }

    bool ToggleStateFromName(string name) {
        if(_toggleEnableListOfAnimationSet == null) {
            return false;
        }

        return _toggleEnableListOfAnimationSet.Contains(name);
    }

    void ShowAnimationSettingWindowWithAnimationSet(MZAnimationSet animationSet) {
        if(_animationSettingWindow == null) {
            _animationSettingWindow = ScriptableObject.CreateInstance<MZAnimationSettingWindow>();
        }

        _animationSettingWindow.ShowWithAnimationSet(animationSet, OnAnimationSettingWindowLostFocus);
    }

    void OnAnimationSettingWindowLostFocus(MZAnimationSet animationSet) {   
        // TODO: replace animation name for new, check corrent in all c# environment
        foreach(string originName in _animationsEditor.animationSetsByName.Keys) {
            if(_animationsEditor.animationSetsByName[originName] == animationSet) {
                if(originName != animationSet.name) {
                    _animationsEditor.animationSetsByName.Remove(originName);
                    _animationsEditor.animationSetsByName.Add(animationSet.name, animationSet);
                }

                break;
            }
        }

		// TODO: Sample way
		_toggleEnableListOfAnimationSet.Clear();
    }

    GameObject GameObjectWithAnimation() {
        GameObject gameObject = new GameObject();

        gameObject.transform.parent = _animationsEditor.AnimationNodesParent;
        gameObject.AddComponent<MZAnimation>();
        gameObject.transform.localScale = new Vector3(100, 100, 0);

        return gameObject;
    }

    #endregion
}
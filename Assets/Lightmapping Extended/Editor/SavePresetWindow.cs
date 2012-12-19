using UnityEngine;
using UnityEditor;
using System.Collections;

public class SavePresetWindow : EditorWindow
{
	internal bool didFocus;
	
	string presetName = "";
	
	public LMExtendedWindow lmExtendedWindow;
	
	void OnGUI ()
	{
		Event current = Event.current;
		bool enterKeyDown = current.type == EventType.KeyDown && (current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter);
		GUI.SetNextControlName ("name");
		presetName = EditorGUILayout.TextField (presetName);
		if (!this.didFocus) {
			this.didFocus = true;
			GUI.FocusControl ("name");
		}
		GUI.enabled = presetName.Length != 0 && presetName != "Custom";
		
		if (GUILayout.Button ("Save") || enterKeyDown) {
			Close ();
			lmExtendedWindow.SavePreset (presetName);
			GUIUtility.ExitGUI ();
		}
	}
}

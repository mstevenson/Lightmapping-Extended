using UnityEngine;
using UnityEditor;
using System.Collections;

public class SavePresetWindow : EditorWindow
{
	internal bool didFocus;
	
	string name = "";
	
	public LMExtendedWindow lmExtendedWindow;
	
	void OnGUI ()
	{
		Event current = Event.current;
		bool enterKeyDown = current.type == EventType.KeyDown && (current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter);
		GUI.SetNextControlName ("name");
		name = EditorGUILayout.TextField (name);
		if (!this.didFocus) {
			this.didFocus = true;
			GUI.FocusControl ("name");
		}
		GUI.enabled = name.Length != 0 && name != "Custom";
		
		if (GUILayout.Button ("Save") || enterKeyDown) {
			Close ();
			lmExtendedWindow.SavePreset (name);
			GUIUtility.ExitGUI ();
		}
	}
}

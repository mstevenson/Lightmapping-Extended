// Copyright (c) 2012 Michael Stevenson
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in the
// Software without restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
// and to permit persons to whom the Software is furnished to do so, subject to the
// following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies
// or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
// OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

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

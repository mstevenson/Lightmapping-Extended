using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;


public class LightmappingAdvancedWindow : EditorWindow
{
	private static LightmappingAdvancedWindow window;
	
	private ILConfig config;
	
	
	[MenuItem ("Window/Lightmapping Advanced")]
	static void Init ()
	{
		LightmappingAdvancedWindow window = (LightmappingAdvancedWindow)EditorWindow.GetWindow (typeof(LightmappingAdvancedWindow));
	}
	
	public string ConfigFilePath {
		get {
			string root = Path.GetDirectoryName (EditorApplication.currentScene);
			string dir = Path.GetFileNameWithoutExtension (EditorApplication.currentScene);
			string path = Path.Combine (root, dir);
			path = Path.Combine (path, "BeastSettings.xml");
			return path;
		}
	}
	
	
	
	private LightmapBakeQuality _bakeQuality;
	private LightmapBakeQuality BakeQuality {
		get {
			return _bakeQuality;
		}
		set {
			_bakeQuality = value;
		}
	}
	
	
	private int tabSelected;
	
	void OnGUI ()
	{
		if (GUILayout.Button ("Save File")) {
			if (config != null) {
				Debug.Log ("save " + ConfigFilePath);
				config.Save (ConfigFilePath);
			}
		}
		
		if (GUILayout.Button ("Load File")) {
			string path = ConfigFilePath;
			if (File.Exists (path)) {
				Debug.Log ("loading " + path);
				config = ILConfig.Load (path);
			} else {
				Debug.Log ("Config file doesn't exist");
			}
		}
		
		
		
		if (config != null) {
			BakeQuality = (LightmapBakeQuality)EditorGUILayout.EnumPopup (BakeQuality);
		}
		
	}
	
	
	void RenderSettingsGUI ()
	{
		
	}
	
	void EnvironmentSettingsGUI ()
	{
		
	}
	
	
	void FrameSettingsGUI ()
	{
		
	}
	
	
	void AdaptiveSamplingGUI ()
	{
		
	}
	
	void GlobalIllumination ()
	{
		
	}
	
	void SurfaceTransfer ()
	{
		
	}
	
	void TextureBakeGUI ()
	{
		
	}
	
}

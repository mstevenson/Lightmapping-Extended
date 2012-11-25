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
	
	
	
	private int selected;
	
	void OnGUI ()
	{
		string path = ConfigFilePath;
		if (File.Exists (path)) {
			config = ILConfig.Load (path);
		} else {
			if (GUILayout.Button ("Generate Beast settings file for current scene")) {
//				ILConfig newConfig = new ILConfig ();
//				newConfig.Save (ConfigFilePath);
//				config = ILConfig.Load (path);
			} else {
				return;
			}
		}
		
		selected = GUILayout.SelectionGrid (selected, new string[] {"Global Illumination", "Environment"}, 3);
		EditorGUILayout.Space ();
		switch (selected) {
		case 0:
			GlobalIlluminationGUI ();
			break;
		case 1:
			EnvironmentGUI ();
			break;
		case 2:
			break;
		}
		
		
		if (GUI.changed) {
			config.Save (ConfigFilePath);
		}
		
//		GUILayout.BeginHorizontal ();
//		{
//			if (GUILayout.Button ("Clear")) {
//				Lightmapping.Clear ();
//			}
//			if (Lightmapping.isRunning) {
//				if (GUILayout.Button ("Cancel")) {
//					Lightmapping.Cancel ();
//				}
//			} else {
//				if (GUILayout.Button ("Bake")) {
//					Lightmapping.Bake ();
//				}
//			}
//		}
//		GUILayout.EndHorizontal ();
		
	}
	
	
	void RenderSettingsGUI ()
	{
		
	}
	
	
	void FrameSettingsGUI ()
	{
		
	}
	
	
	void AdaptiveSamplingGUI ()
	{
		
	}
	
	void GlobalIlluminationGUI ()
	{
		FinalGatherSettings ();
	}
	
	
	void FinalGatherSettings ()
	{
		// Final Gather
		if (EditorGUILayout.BeginToggleGroup ("Final Gather", config.giSettings.primaryIntegrator == ILConfig.GISettings.Integrator.FinalGather)) {
			config.giSettings.primaryIntegrator = ILConfig.GISettings.Integrator.FinalGather;
		} else {
			config.giSettings.primaryIntegrator = ILConfig.GISettings.Integrator.None;
		}
		{
			EditorGUI.indentLevel = 1;
			Toggle ("Fast Preview", ref config.giSettings.fgPreview);
			IntField ("Rays", ref config.giSettings.fgRays);
			FloatField ("Contrast Threshold", ref config.giSettings.fgContrastThreshold);
			FloatField ("Gradient Threshold", ref config.giSettings.fgGradientThreshold);
			IntSlider ("Interpolation Points", ref config.giSettings.fgInterpolationPoints, 1, 50);
			Toggle ("Check Visibility", ref config.giSettings.fgCheckVisibility);
			
			EditorGUILayout.Space ();
			
			IntSlider ("Bounces", ref config.giSettings.fgDepth, 1, 10);
			EditorGUI.indentLevel = 2;
			FloatField ("Boost", ref config.giSettings.diffuseBoost);
			FloatField ("Intensity", ref config.giSettings.primaryIntensity);
			LightmapEditorSettings.bounceIntensity = config.giSettings.primaryIntensity;
			FloatField ("Saturation", ref config.giSettings.primarySaturation);
			EditorGUI.indentLevel = 1;
			
			EditorGUILayout.Space ();
			
			AOSettings ();
			
			EditorGUILayout.Space ();
			
			// Path Tracer
			PathTracerSettings ();
		}
		EditorGUILayout.EndToggleGroup ();
	}
	
	
	void AOSettings ()
	{
		config.giSettings.fgAOInfluence = EditorGUILayout.Slider ("Ambient Occlusion", config.giSettings.fgAOInfluence, 0, 1);
		LightmapEditorSettings.aoAmount = config.giSettings.fgAOInfluence;
		EditorGUI.indentLevel = 2;
		if (config.giSettings.fgAOInfluence <= 0)
			GUI.enabled = false;
		
		config.giSettings.fgAOContrast = EditorGUILayout.Slider ("Contrast", config.giSettings.fgAOContrast, 0, 2);
		LightmapEditorSettings.aoContrast = config.giSettings.fgAOContrast;
		
		config.giSettings.fgAOMaxDistance = EditorGUILayout.FloatField ("Max Distance", config.giSettings.fgAOMaxDistance);
		LightmapEditorSettings.aoMaxDistance = config.giSettings.fgAOMaxDistance;
		
		config.giSettings.fgAOScale = EditorGUILayout.FloatField ("Scale", config.giSettings.fgAOScale);
		
		GUI.enabled = true;
	}
	
	void PathTracerSettings ()
	{
		if (EditorGUILayout.BeginToggleGroup ("Path Tracer", config.giSettings.secondaryIntegrator == ILConfig.GISettings.Integrator.PathTracer)) {
			config.giSettings.secondaryIntegrator = ILConfig.GISettings.Integrator.PathTracer;
		} else {
			config.giSettings.secondaryIntegrator = ILConfig.GISettings.Integrator.None;
		}
		FloatField ("Intensity", ref config.giSettings.secondaryIntensity);
		FloatField ("Saturation", ref config.giSettings.secondarySaturation);
		FloatField ("Accuracy", ref config.giSettings.ptAccuracy);
		FloatField ("Point Size", ref config.giSettings.ptPointSize);
		Toggle ("Cache Direct Light", ref config.giSettings.ptCacheDirectLight);
		Toggle ("Check Visibility", ref config.giSettings.ptCheckVisibility);
	}
	
	void EnvironmentGUI ()
	{
		config.environmentSettings.giEnvironment = (ILConfig.EnvironmentSettings.Environment)EditorGUILayout.EnumPopup ("Environment Type", config.environmentSettings.giEnvironment);
		if (config.environmentSettings.giEnvironment == ILConfig.EnvironmentSettings.Environment.None) {
			GUI.enabled = false;
		} else {
			GUI.enabled = true;
		}
		if (config.environmentSettings.giEnvironment == ILConfig.EnvironmentSettings.Environment.SkyLight) {
			
		} else {
			EditorGUILayout.LabelField ("HDR Image", Path.GetFileName (config.environmentSettings.iblImageFile));
			if (GUILayout.Button ("Choose Image")) {
				config.environmentSettings.iblImageFile = EditorUtility.OpenFilePanel ("Select EXR or HDR file", "", "");
				GUIUtility.ExitGUI ();
			}
		}
		config.environmentSettings.giEnvironmentIntensity = EditorGUILayout.FloatField ("Intensity", config.environmentSettings.giEnvironmentIntensity);
		
		GUI.enabled = true;
	}
	
	
	void TextureBakeGUI ()
	{
		
	}
	
	
	private void Toggle (string name, ref bool val)
	{
		val = EditorGUILayout.Toggle (name, val);
	}
	
	private void FloatField (string name, ref float val)
	{
		val = EditorGUILayout.FloatField (name, val);
	}
	
	private void IntField (string name, ref int val)
	{
		val = EditorGUILayout.IntField (name, val);
	}

	private void IntSlider (string name, ref int val, int min, int max)
	{
		val = EditorGUILayout.IntSlider (name, val, min, max);
	}
}

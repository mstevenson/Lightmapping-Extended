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
			
			GlobalIlluminationGUI ();
//			SurfaceTransferGUI ();
		}
		
		if (GUI.changed) {
			config.Save (ConfigFilePath);
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
	
	void GlobalIlluminationGUI ()
	{
		config.giSettings.enableGI = EditorGUILayout.BeginToggleGroup ("Enable GI", config.giSettings.enableGI);
		{
			config.giSettings.fgRays = EditorGUILayout.IntField ("Rays", config.giSettings.fgRays);
			config.giSettings.fgDepth = EditorGUILayout.IntField ("Bounces", config.giSettings.fgDepth);
			
			// Final Gather
			if (EditorGUILayout.BeginToggleGroup ("Final Gather", config.giSettings.primaryIntegrator == ILConfig.GISettings.Integrator.FinalGather)) {
				config.giSettings.primaryIntegrator = ILConfig.GISettings.Integrator.FinalGather;
			} else {
				config.giSettings.primaryIntegrator = ILConfig.GISettings.Integrator.None;
			}
			config.giSettings.fgPreview = EditorGUILayout.Toggle ("Preview", config.giSettings.fgPreview);
			config.giSettings.fgContrastThreshold = EditorGUILayout.FloatField ("Contrast Threshold", config.giSettings.fgContrastThreshold);
			config.giSettings.fgGradientThreshold = EditorGUILayout.FloatField ("Gradient Threshold", config.giSettings.fgGradientThreshold);
			config.giSettings.fgCheckVisibility = EditorGUILayout.Toggle ("Check Visibility", config.giSettings.fgCheckVisibility);
			config.giSettings.fgInterpolationPoints = EditorGUILayout.IntSlider ("Interpolation Points", config.giSettings.fgInterpolationPoints, 1, 50);
			
			config.giSettings.diffuseBoost = EditorGUILayout.FloatField ("Bounce Boost", config.giSettings.diffuseBoost);
			config.giSettings.primaryIntensity = EditorGUILayout.FloatField ("Bounce Intensity", config.giSettings.primaryIntensity);
			config.giSettings.primarySaturation = EditorGUILayout.FloatField ("Bounce Saturation", config.giSettings.primarySaturation);
			
			EditorGUILayout.EndToggleGroup ();
			EditorGUILayout.Space ();
			
			
			// Path Tracer
			if (EditorGUILayout.BeginToggleGroup ("Path Tracer", config.giSettings.secondaryIntegrator == ILConfig.GISettings.Integrator.PathTracer)) {
				config.giSettings.secondaryIntegrator = ILConfig.GISettings.Integrator.PathTracer;
			} else {
				config.giSettings.secondaryIntegrator = ILConfig.GISettings.Integrator.None;
			}
			config.giSettings.secondaryIntensity = EditorGUILayout.FloatField ("Intensity", config.giSettings.secondaryIntensity);
			config.giSettings.secondarySaturation = EditorGUILayout.FloatField ("Saturation", config.giSettings.secondarySaturation);
			config.giSettings.ptAccuracy = EditorGUILayout.FloatField ("Accuracy", config.giSettings.ptAccuracy);
			config.giSettings.ptPointSize = EditorGUILayout.FloatField ("Point Size", config.giSettings.ptPointSize);
			config.giSettings.ptCacheDirectLight = EditorGUILayout.Toggle ("Cache Direct Light", config.giSettings.ptCacheDirectLight);
			config.giSettings.ptCheckVisibility = EditorGUILayout.Toggle ("Check Visibility", config.giSettings.ptCheckVisibility);
			
			
			EditorGUILayout.EndToggleGroup ();
			EditorGUILayout.Space ();
			
			GUILayout.Label ("Ambient Occlusion");
			config.giSettings.fgAOInfluence = EditorGUILayout.FloatField ("Influence", config.giSettings.fgAOInfluence);
			if (config.giSettings.fgAOInfluence == 0)
				GUI.enabled = false;
			config.giSettings.fgAOMaxDistance = EditorGUILayout.FloatField ("Max Distance", config.giSettings.fgAOMaxDistance);
			config.giSettings.fgAOContrast = EditorGUILayout.FloatField ("Contrast", config.giSettings.fgAOContrast);
			config.giSettings.fgAOScale = EditorGUILayout.FloatField ("Scale", config.giSettings.fgAOScale);
			GUI.enabled = true;
		}
		EditorGUILayout.EndToggleGroup ();
		
	}
	
	void SurfaceTransferGUI ()
	{
		config.surfaceTransferSettings.selectionMode = (ILConfig.SurfaceTransferSettings.SelectionMode)EditorGUILayout.EnumPopup ("Selection Mode", config.surfaceTransferSettings.selectionMode);
		config.surfaceTransferSettings.frontRange = EditorGUILayout.FloatField ("Front Range", config.surfaceTransferSettings.frontRange);
		config.surfaceTransferSettings.frontBias = EditorGUILayout.FloatField ("Front Bias", config.surfaceTransferSettings.frontBias);
		config.surfaceTransferSettings.backRange = EditorGUILayout.FloatField ("Back Range", config.surfaceTransferSettings.backRange);
		config.surfaceTransferSettings.backBias = EditorGUILayout.FloatField ("Back Bias", config.surfaceTransferSettings.backBias);
	}
	
	void TextureBakeGUI ()
	{
		
	}
	
}

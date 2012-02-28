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
			
			GlobalIlluminationGUI ();
			
			SurfaceTransferGUI ();
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
		config.giSettings.enableGI = EditorGUILayout.Toggle ("Preview", config.giSettings.enableGI);
		
		GUILayout.Label ("Final Gather");
		config.giSettings.fgPreview = EditorGUILayout.Toggle ("Preview", config.giSettings.fgPreview);
		config.giSettings.fgRays = EditorGUILayout.IntField ("Rays", config.giSettings.fgRays);
		config.giSettings.fgDepth = EditorGUILayout.IntField ("Bounces", config.giSettings.fgDepth);
		config.giSettings.fgContrastThreshold = EditorGUILayout.FloatField ("Contrast Threshold", config.giSettings.fgContrastThreshold);
		config.giSettings.fgGradientThreshold = EditorGUILayout.FloatField ("Gradient Threshold", config.giSettings.fgGradientThreshold);
		config.giSettings.fgCheckVisibility = EditorGUILayout.Toggle ("Check Visibility", config.giSettings.fgCheckVisibility);
		config.giSettings.fgInterpolationPoints = EditorGUILayout.IntField ("Interpolation Points", config.giSettings.fgInterpolationPoints);
		
		config.giSettings.primaryIntegrator = (ILConfig.GISettings.Integrator)EditorGUILayout.EnumPopup ("Primary Integrator", config.giSettings.primaryIntegrator);
		config.giSettings.primaryIntensity = EditorGUILayout.FloatField ("Primary Intensity", config.giSettings.primaryIntensity);
		config.giSettings.primarySaturation = EditorGUILayout.FloatField ("Primary Saturation", config.giSettings.primarySaturation);
		
		config.giSettings.secondaryIntegrator = (ILConfig.GISettings.Integrator)EditorGUILayout.EnumPopup ("Secondary Integrator", config.giSettings.secondaryIntegrator);
		config.giSettings.secondaryIntensity = EditorGUILayout.FloatField ("Secondary Intensity", config.giSettings.secondaryIntensity);
		config.giSettings.secondarySaturation = EditorGUILayout.FloatField ("Secondary Saturation", config.giSettings.secondarySaturation);
		
		config.giSettings.diffuseBoost = EditorGUILayout.FloatField ("Bounce Boost", config.giSettings.diffuseBoost);
		
		GUILayout.Label ("Ambient Occlusion");
		config.giSettings.fgAOInfluence = EditorGUILayout.FloatField ("Influence", config.giSettings.fgAOInfluence);
		config.giSettings.fgAOMaxDistance = EditorGUILayout.FloatField ("Max Distance", config.giSettings.fgAOMaxDistance);
		config.giSettings.fgAOContrast = EditorGUILayout.FloatField ("Contrast", config.giSettings.fgAOContrast);
		config.giSettings.fgAOScale = EditorGUILayout.FloatField ("Scale", config.giSettings.fgAOScale);
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

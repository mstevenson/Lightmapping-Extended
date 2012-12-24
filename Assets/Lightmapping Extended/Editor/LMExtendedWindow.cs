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
using UnityEditorInternal;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class LMExtendedWindow : EditorWindow
{
	const string assetFolderName = "Lightmapping Extended";
	
	private ILConfig config;

	[MenuItem ("Window/Lightmapping Extended", false, 2098)]
	static void Init ()
	{
		var window = EditorWindow.GetWindow<LMExtendedWindow> (false, "LM Extended");
		window.autoRepaintOnSceneChange = true;
	}
	
	#region Configuration
	
	public string ConfigFilePath {
		get {
			if (string.IsNullOrEmpty (EditorApplication.currentScene))
				return "";
			string root = Path.GetDirectoryName (EditorApplication.currentScene);
			string dir = Path.GetFileNameWithoutExtension (EditorApplication.currentScene);
			string path = Path.Combine (root, dir);
			path = Path.Combine (path, "BeastSettings.xml");
			return path;
		}
	}
	
	void SaveConfig ()
	{
		config.SerializeToPath (ConfigFilePath);
	}
	
	#endregion
	
	
	#region Unity Events
	
	void OnEnable ()
	{
		presetsFolderPath = GetPresetsFolderPath ();
	}
	
	void OnSelectionChange ()
	{
		if (!File.Exists (ConfigFilePath)) {
			SetPresetToDefault ();
			config = null;
		}
		Repaint ();
	}

	void OnFocus ()
	{
		if (!File.Exists (ConfigFilePath))
			config = null;
		Repaint ();
	}

	void OnProjectChange ()
	{
		if (!File.Exists (ConfigFilePath))
			config = null;
		Repaint ();
	}
	
	
	[SerializeField]
	int toolbarSelected;
	[SerializeField]
	Vector2 scroll;

	void OnGUI ()
	{
		string path = ConfigFilePath;
		if (string.IsNullOrEmpty (path)) {
			GUILayout.Label ("Open a scene file to edit lightmap settings");
			return;
		}
		
		// Determine if config file exists
		bool haveConfigFile = false;
		if (config == null) {
			if (File.Exists (path)) {
				config = ILConfig.DeserializeFromPath (path);
				haveConfigFile = true;
			}
		} else {
			haveConfigFile = true;
		}

		// Option to generate a config file
		if (!haveConfigFile) {
			EditorGUILayout.Space ();
			if (GUILayout.Button ("Generate Beast settings file for current scene")) {
				SetPresetToDefault ();
				ILConfig newConfig = new ILConfig ();
				var dir = Path.GetDirectoryName (ConfigFilePath);
				if (!Directory.Exists (dir))
					Directory.CreateDirectory (dir);
				newConfig.SerializeToPath (ConfigFilePath);
				config = ILConfig.DeserializeFromPath (path);
				AssetDatabase.Refresh ();
				GUIUtility.ExitGUI ();
			}
			return;
		}
		
		EditorGUILayout.Space ();
		
		PresetSelectionGUI ();
		
		EditorGUILayout.Space ();
		
		int lastSelected = toolbarSelected;
		toolbarSelected = GUILayout.Toolbar (toolbarSelected, new string[] {"Settings", "Global Illum", "Environment"});
		// Prevent text fields from grabbing focus when switching tabs
		if (toolbarSelected != lastSelected) {
			GUI.FocusControl ("");
		}
		
		EditorGUILayout.Space ();

		scroll = EditorGUILayout.BeginScrollView (scroll);
		{
			SerializedObject serializedConfig = new SerializedObject (config);
			switch (toolbarSelected) {
			case 0:
				PerformanceSettingsGUI (serializedConfig);
				TextureBakeGUI (serializedConfig);
				AASettingsGUI (serializedConfig);
				RenderSettingsGUI (serializedConfig);
				break;
			case 1:
				GlobalIlluminationGUI (serializedConfig);
				break;
			case 2:
				EnvironmentGUI (serializedConfig);
				break;
			}

			serializedConfig.ApplyModifiedProperties ();
			
			if (GUI.changed) {
				SaveConfig ();
			}
		}
		
		EditorGUILayout.EndScrollView ();
		
		EditorGUILayout.Space ();
		GUILayout.BeginHorizontal ();
		{
			BakeButtonsGUI ();
		}
		GUILayout.EndHorizontal ();
		EditorGUILayout.Space ();
		
		// Use FocusControl to release focus from text fields when switching tabs
		GUI.SetNextControlName ("");
	}
	
	#endregion
	
	
	#region Presets
	
	string presetsFolderPath;
	string currentPresetName = "";
	
	void PresetSelectionGUI ()
	{
		GUILayout.BeginHorizontal ();
		{
			GUILayout.Label ("Preset: ");
			currentPresetName = PresetsPopup (currentPresetName);
			EditorGUILayout.BeginHorizontal ();
			{
				int width = 42;
				GUILayout.FlexibleSpace ();
				if (IsCurrentPresetDefault)
					GUI.enabled = false;
				if (GUILayout.Button ("Delete", EditorStyles.miniButtonLeft, GUILayout.Width (width))) {
					if (EditorUtility.DisplayDialog ("Delete Preset", "Do you want to delete the lightmapping preset named \"" + currentPresetName + "\"?", "OK", "Cancel")) {
						DeletePreset (currentPresetName);
					}
				}
				GUI.enabled = true;
				
				if (IsCurrentPresetDefault)
					GUI.enabled = false;
				if (GUILayout.Button ("Save", EditorStyles.miniButtonMid, GUILayout.Width (width))) {
					SavePreset (currentPresetName);
				}
				GUI.enabled = true;
				if (GUILayout.Button ("Create", EditorStyles.miniButtonRight, GUILayout.Width (width))) {
					SetPresetToDefault ();
					CreatePreset ();
				}
			}
			EditorGUILayout.EndHorizontal ();
			if (GUI.changed && !IsCurrentPresetDefault) {
				LoadPreset (currentPresetName);
			}
		}
		GUILayout.EndHorizontal ();
	}
	
	private string PresetsPopup (string presetString)
	{
		List<string> presets = new List<string> (GetPresetNames ());
		int presetIndex = presets.IndexOf (presetString);
		
		// Include a "Custom" option at the beginning of the list
		presets.Insert (0, "Custom");
		// Shift the indexes forward to account for the new "Custom" option
		presetIndex++;
		
		presetIndex = EditorGUILayout.Popup (presetIndex, presets.ToArray ());
		string newPresetName = presets [presetIndex];
		return newPresetName;
	}
	
	void CreatePreset ()
	{
		var w = EditorWindow.GetWindow<LMExtendedWindow> ();
		Rect pos = new Rect (w.position.x, w.position.y, w.position.width, 55);
		var window = EditorWindow.GetWindowWithRect<SavePresetWindow> (pos, true, "Create Lightmapping Preset", true);
		window.position = pos;
		window.lmExtendedWindow = this;
	}
	
	static string GetPresetsFolderPath ()
	{
		string[] assets = AssetDatabase.GetAllAssetPaths ();
		foreach (var a in assets) {
			if (Path.GetFileName (a) == assetFolderName) {
				if (Directory.Exists (a)) {
					return a + "/Presets";
				}
			}
		}
		return null;
	}
	
	string[] GetPresetNames ()
	{
		if (Directory.Exists (presetsFolderPath)) {
			string[] files = Directory.GetFiles (presetsFolderPath, "*.xml", SearchOption.AllDirectories);
			string[] presetNames = files.Select (s => s.Remove (0, presetsFolderPath.Length + 1).Replace (".xml", "")).ToArray ();
			return presetNames;
		} else {
			return new string[0];
		}
	}
	
	public void SavePreset (string name)
	{
		var dir = presetsFolderPath + "/" + Path.GetDirectoryName (name);
		if (!Directory.Exists (dir)) {
			Debug.Log ("Create " + dir);
			Directory.CreateDirectory (dir);
		}
		this.config.SerializeToPath (GetPresetPath (name));
		AssetDatabase.Refresh ();
		currentPresetName = name;
	}
	
	void DeletePreset (string name)
	{
		if (Directory.Exists (presetsFolderPath)) {
			AssetDatabase.DeleteAsset (GetPresetPath (name));
			SetPresetToDefault ();
		}
	}
	
	void LoadPreset (string name)
	{
		// Load the preset config file
		config = ILConfig.DeserializeFromPath (GetPresetPath (name));
		// Save preset data back out to our scene's config file
		SaveConfig ();
	}
	
	string GetPresetPath (string presetName)
	{
		return presetsFolderPath + "/" + presetName + ".xml";
	}
	
	void SetPresetToDefault ()
	{
		currentPresetName = "Custom";
	}
	
	bool IsCurrentPresetDefault {
		get {
			return currentPresetName == "Custom";
		}
	}
	
	#endregion
	
	
	#region Settings
	
	void PerformanceSettingsGUI (SerializedObject serializedConfig)
	{
		SerializedProperty autoThreads = serializedConfig.FindProperty ("frameSettings.autoThreads");
		SerializedProperty autoThreadsSubtract = serializedConfig.FindProperty ("frameSettings.autoThreadsSubtract");
		SerializedProperty renderThreads = serializedConfig.FindProperty ("frameSettings.renderThreads");
		
		// Threads
		GUILayout.Label ("CPU", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField (autoThreads, new GUIContent ("Auto Threads", "If enabled, Beast will try to auto detect the CPU configuration and use one thread per core."));
		EditorGUI.indentLevel++;
		if (!config.frameSettings.autoThreads)
			GUI.enabled = false;
		EditorGUILayout.PropertyField (autoThreadsSubtract, new GUIContent ("Subtract Threads", "If autoThreads is enabled, this can be used to decrease the number of utilized cores, e.g. to leave one or two cores free to do other work."));
		GUI.enabled = true;
		if (config.frameSettings.autoThreads)
			GUI.enabled = false;
		EditorGUILayout.PropertyField (renderThreads, new GUIContent ("Render Threads", "If autoThreads is disabled, this will set the number of threads beast uses. One per core is a good start."));
		GUI.enabled = true;
		EditorGUI.indentLevel--;
		EditorGUI.indentLevel--;
		
		// The following options are not useful unless using Beast's own UI. This UI is accessible if Unity's embedded Beast tool is called
		
//		GUILayout.Label ("Tiles", EditorStyles.boldLabel);
//		EditorGUI.indentLevel++;
//		config.frameSettings.tileScheme = (ILConfig.FrameSettings.TileScheme)EditorGUILayout.EnumPopup (new GUIContent ("Tile Scheme", "Different ways for Beast to distribute tiles over the image plane."), config.frameSettings.tileScheme);
//		IntField ("Tile Size", ref config.frameSettings.tileSize, "A smaller tile gives better ray tracing coherence. There is no 'best setting' for all scenes. Default value is 32, giving 32x32 pixel tiles. The largest allowed tile size is 128.");
//		EditorGUI.indentLevel--;

//		GUILayout.Label ("Output Verbosity");
//		EditorGUI.indentLevel++;
//		Toggle ("Debug File", ref config.frameSettings.outputVerbosity.debugFile, "Save all log messages to a file named debug.out.");
//		Toggle ("Debug Print", ref config.frameSettings.outputVerbosity.debugPrint, "Used for development purposes.");
//		Toggle ("Error Print", ref config.frameSettings.outputVerbosity.errorPrint, "");
//		Toggle ("Warning Print", ref config.frameSettings.outputVerbosity.warningPrint, "");
//		Toggle ("Benchmark Print", ref config.frameSettings.outputVerbosity.benchmarkPrint, "");
//		Toggle ("Progress Print", ref config.frameSettings.outputVerbosity.progressPrint, "");
//		Toggle ("Info Print", ref config.frameSettings.outputVerbosity.infoPrint, "");
//		Toggle ("Verbose Print", ref config.frameSettings.outputVerbosity.verbosePrint, "");
//		EditorGUI.indentLevel--;
	}
	
	void AASettingsGUI (SerializedObject serializedConfig)
	{
		SerializedProperty samplingMode = serializedConfig.FindProperty ("aaSettings.samplingMode");
		SerializedProperty minSampleRate = serializedConfig.FindProperty ("aaSettings.minSampleRate");
		SerializedProperty maxSampleRate = serializedConfig.FindProperty ("aaSettings.maxSampleRate");
		SerializedProperty contrast = serializedConfig.FindProperty ("aaSettings.contrast");
		SerializedProperty filter = serializedConfig.FindProperty ("aaSettings.filter");
		SerializedProperty filterSizeX = serializedConfig.FindProperty ("aaSettings.filterSize.x");
		SerializedProperty filterSizeY = serializedConfig.FindProperty ("aaSettings.filterSize.y");
		
		GUILayout.Label ("Antialiasing", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField (samplingMode, new GUIContent ("Sampling Mode", ""));
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField (minSampleRate, new GUIContent ("Min Sample Rate", "Controls the minimum number of samples per pixel. The formula used is 4^maxSampleRate (1, 4, 16, 64, 256 samples per pixel)"));
		EditorGUILayout.PropertyField (maxSampleRate, new GUIContent ("Max Sample Rate", "Controls the maximum number of samples per pixel. Values less than 0 allows using less than one sample per pixel (if AdaptiveSampling is used). The formula used is 4^maxSampleRate (1, 4, 16, 64, 256 samples per pixel)"));
		EditorGUI.indentLevel--;
		EditorGUILayout.PropertyField (contrast, new GUIContent ("Contrast", "If the contrast differs less than this threshold Beast will consider the sampling good enough. Default value is 0.1."));
		EditorGUILayout.PropertyField (filter, new GUIContent ("Filter", "The sub-pixel filter to use."));
		EditorGUILayout.PrefixLabel (new GUIContent ("Filter Size"));
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField (filterSizeX, new GUIContent ("X", ""));
		EditorGUILayout.PropertyField (filterSizeY, new GUIContent ("Y", ""));
		EditorGUI.indentLevel--;
		EditorGUI.indentLevel--;
	}
	
	void RenderSettingsGUI (SerializedObject serializedConfig)
	{
		SerializedProperty maxRayDepth = serializedConfig.FindProperty ("renderSettings.maxRayDepth");
		SerializedProperty bias = serializedConfig.FindProperty ("renderSettings.bias");
		SerializedProperty reflectionDepth = serializedConfig.FindProperty ("renderSettings.reflectionDepth");
		SerializedProperty reflectionThreshold = serializedConfig.FindProperty ("renderSettings.reflectionThreshold");
		SerializedProperty giTransparencyDepth = serializedConfig.FindProperty ("renderSettings.giTransparencyDepth");
//		SerializedProperty shadowDepth = serializedConfig.FindProperty ("renderSettings.shadowDepth");
		SerializedProperty minShadowRays = serializedConfig.FindProperty ("renderSettings.minShadowRays");
		SerializedProperty maxShadowRays = serializedConfig.FindProperty ("renderSettings.maxShadowRays");
		SerializedProperty vertexMergeThreshold = serializedConfig.FindProperty ("renderSettings.vertexMergeThreshold");
		SerializedProperty tsOddUVFlipping = serializedConfig.FindProperty ("renderSettings.tsOddUVFlipping");
		SerializedProperty tsVertexOrthogonalization = serializedConfig.FindProperty ("renderSettings.tsVertexOrthogonalization");
		SerializedProperty tsVertexNormalization = serializedConfig.FindProperty ("renderSettings.tsVertexNormalization");
		SerializedProperty tsIntersectionOrthogonalization = serializedConfig.FindProperty ("renderSettings.tsIntersectionOrthogonalization");
		SerializedProperty tsIntersectionNormalization = serializedConfig.FindProperty ("renderSettings.tsIntersectionNormalization");
		
		
		GUILayout.Label ("Rays", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField (maxRayDepth, new GUIContent ("Max Bounces", "The maximum amount of 'bounces' a ray can have before being considered done. A bounce can be a reflection or refraction."));
		EditorGUILayout.PropertyField (bias, new GUIContent ("Bias", "An error threshold to avoid double intersections. For example, a shadow ray should not intersect the same triangle as the primary ray did, but because of limited numerical precision this can happen. The bias value moves the intersection point to eliminate this problem. If set to zero this value is computed automatically depending on the scene size."));
		EditorGUI.indentLevel--;

		GUILayout.Label ("Reflections & Transparency", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField (reflectionDepth, new GUIContent ("Max Reflection Bounces", "The maximum amount of reflections a ray can have before being considered done."));
		EditorGUILayout.PropertyField (reflectionThreshold, new GUIContent ("Reflection Threshold", "If the intensity of the reflected contribution is less than the threshold, the ray will be terminated."));
		EditorGUILayout.PropertyField (giTransparencyDepth, new GUIContent ("GI Transparency Depth", "Controls the maximum transparency depth for Global Illumination rays. Used to speed up renderings with a lot of transparency (for example trees)."));
		EditorGUI.indentLevel--;

		GUILayout.Label ("Shadows", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		
		// FIXME add undo
		config.renderSettings.shadowDepth = (int)((ILConfig.ShadowDepth)EditorGUILayout.EnumPopup (new GUIContent ("Shadow Depth", "Controls which rays that spawn shadow rays."), (ILConfig.ShadowDepth)System.Enum.Parse (typeof(ILConfig.ShadowDepth), config.renderSettings.shadowDepth.ToString ())));
		
		EditorGUILayout.PropertyField (minShadowRays, new GUIContent ("Min Shadow Rays", "The minimum number of shadow rays that will be sent to determine if a point is lit by a specific light source. Use this value to ensure that you get enough quality in soft shadows at the price of render times. This will raise the minimum number of rays sent for any light sources that have a minShadowSamples setting lower than this value, but will not lower the number if minShadowSamples is set to a higher value. Setting this to a value higher than maxShadowRays will not send more rays than maxShadowRays."));
		EditorGUILayout.PropertyField (maxShadowRays, new GUIContent ("Max Shadow Rays", "The maximum number of shadow rays per point that will be used to generate a soft shadow for any light source. Use this to shorten render times at the price of soft shadow quality. This will lower the maximum number of rays sent for any light sources that have a shadow samples setting higher than this value, but will not raise the number if shadow samples is set to a lower value."));
		EditorGUI.indentLevel--;

		GUILayout.Label ("Geometry", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField (vertexMergeThreshold, new GUIContent ("Vertex Merge Threshold", "Triangle vertices that are closer together than this threshold will be merged into one (if possible depending on other vertex data)."));
		EditorGUILayout.PropertyField (tsOddUVFlipping, new GUIContent ("Odd UV Flipping", "Using this setting will force Beast to mirror tangent and binormal when UV has odd winding direction."));
		EditorGUILayout.PropertyField (tsVertexOrthogonalization, new GUIContent ("Vertex Orthogonalization", "Orthogonalize tangent space basis vectors (tangent, binormal and normal) at every vertex."));
		EditorGUILayout.PropertyField (tsVertexNormalization, new GUIContent ("Vertex Normalization", "Normalize tangent space basis vectors (tangent, binormal and normal) at every vertex."));
		EditorGUILayout.PropertyField (tsIntersectionOrthogonalization, new GUIContent ("Intersection Orthogonalization", "Orthogonalize tangent space basis vectors (tangent, binormal and normal) at every intersection point."));
		EditorGUILayout.PropertyField (tsIntersectionNormalization, new GUIContent ("Intersection Normalization", "Normalize tangent space basis vectors (tangent, binormal and normal) at every intersection point."));
		EditorGUI.indentLevel--;
	}

	void GlobalIlluminationGUI (SerializedObject serializedConfig)
	{
		SerializedProperty enableGI = serializedConfig.FindProperty ("giSettings.enableGI");
		SerializedProperty fgLightLeakReduction = serializedConfig.FindProperty ("giSettings.fgLightLeakReduction");
		SerializedProperty fgLightLeakRadius = serializedConfig.FindProperty ("giSettings.fgLightLeakRadius");
		
		EditorGUILayout.PropertyField (enableGI, new GUIContent ("Enable GI", ""));
		EditorGUI.BeginDisabledGroup (!config.giSettings.enableGI);
		
		// Caustics have no effect as of Unity 4.0
//		Toggle ("Enable Caustics", ref config.giSettings.enableCaustics, "");

		EditorGUILayout.Space ();

		GUILayout.Label ("Primary Integrator", EditorStyles.boldLabel);
		IntegratorPopup (serializedConfig, true);
		IntegratorSettings (serializedConfig, config.giSettings.primaryIntegrator, true);

		EditorGUILayout.Space ();

		GUILayout.Label ("Secondary Integrator", EditorStyles.boldLabel);
		IntegratorPopup (serializedConfig, false);
		IntegratorSettings (serializedConfig, config.giSettings.secondaryIntegrator, false);

		if (config.giSettings.primaryIntegrator == ILConfig.GISettings.Integrator.FinalGather && config.giSettings.secondaryIntegrator == ILConfig.GISettings.Integrator.PathTracer) {
			EditorGUILayout.Space ();
			EditorGUILayout.PropertyField (fgLightLeakReduction, new GUIContent ("Light Leak Reduction", "This setting can be used to reduce light leakage through walls when using final gather as primary GI and path tracing as secondary GI. Leakage, which can happen when e.g. the path tracer filters in values on the other side of a wall, is reduced by using final gather as a secondary GI fallback when sampling close to walls or corners. When this is enabled a final gather depth of 3 will be used automatically, but the higher depths will only be used close to walls or corners. Note that this is only used when path tracing is set as secondary GI."));
			if (!config.giSettings.fgLightLeakReduction)
				GUI.enabled = false;
			EditorGUILayout.PropertyField (fgLightLeakRadius, new GUIContent ("Light Leak Radius", "Controls how far away from walls the final gather will be called again, instead of the secondary GI. If 0.0 is used a value will be calculated by Beast depending on the secondary GI used. The calculated value is printed in the output window. If you still get leakage you can adjust this by manually typing in a higher value."));
			if (config.giSettings.enableGI)
				GUI.enabled = true;
		}
		
		EditorGUI.EndDisabledGroup ();
	}

	void IntegratorPopup (SerializedObject serializedObject, bool isPrimary)
	{
		SerializedProperty primaryIntegrator = serializedObject.FindProperty ("giSettings.primaryIntegrator");
		SerializedProperty secondaryIntegrator = serializedObject.FindProperty ("giSettings.secondaryIntegrator");
		
		if (isPrimary) {
			EditorGUILayout.PropertyField (primaryIntegrator, new GUIContent (""));
		} else {
			EditorGUILayout.PropertyField (secondaryIntegrator, new GUIContent (""));
		}
	}

	void IntegratorSettings (SerializedObject serializedObject, ILConfig.GISettings.Integrator integrator, bool isPrimary)
	{
//		EditorGUI.indentLevel++;

		if (integrator != ILConfig.GISettings.Integrator.None) {
			if (isPrimary) {
				SerializedProperty primaryIntensity = serializedObject.FindProperty ("giSettings.primaryIntensity");
				SerializedProperty primarySaturation = serializedObject.FindProperty ("giSettings.primarySaturation");
				EditorGUILayout.PropertyField (primaryIntensity, new GUIContent ("Intensity", "Tweak the amount of illumination from the primary and secondary GI integrators. This lets you boost or reduce the amount of indirect light easily."));
				EditorGUILayout.PropertyField (primarySaturation, new GUIContent ("Saturation", "Lets you tweak the amount of color in the primary and secondary GI integrators. This lets you boost or reduce the perceived saturation of the bounced light."));
			} else {
				SerializedProperty secondaryIntensity = serializedObject.FindProperty ("giSettings.secondaryIntensity");
				SerializedProperty secondarySaturation = serializedObject.FindProperty ("giSettings.secondarySaturation");
				EditorGUILayout.PropertyField (secondaryIntensity, new GUIContent ("Intensity", "Tweak the amount of illumination from the primary and secondary GI integrators. This lets you boost or reduce the amount of indirect light easily."));
				EditorGUILayout.PropertyField (secondarySaturation, new GUIContent ("Saturation", "Lets you tweak the amount of color in the primary and secondary GI integrators. This lets you boost or reduce the perceived saturation of the bounced light."));
			}
		}
		
		switch (integrator) {
		case ILConfig.GISettings.Integrator.None:
			if (isPrimary && config.giSettings.primaryIntegrator != ILConfig.GISettings.Integrator.None)
				config.giSettings.primaryIntegrator = ILConfig.GISettings.Integrator.None;
			else if (!isPrimary && config.giSettings.secondaryIntegrator != ILConfig.GISettings.Integrator.None)
				config.giSettings.secondaryIntegrator = ILConfig.GISettings.Integrator.None;
			break;
		case ILConfig.GISettings.Integrator.FinalGather:
			FinalGatherSettings (serializedObject, isPrimary);
			break;
		case ILConfig.GISettings.Integrator.PathTracer:
			PathTracerSettings (serializedObject, isPrimary);
			break;
		case ILConfig.GISettings.Integrator.MonteCarlo:
			MonteCarloSettings (serializedObject, isPrimary);
			break;
		}
//		EditorGUI.indentLevel--;
	}
	
	void FinalGatherSettings (SerializedObject serializedConfig, bool isPrimaryIntegrator)
	{
		SerializedProperty fgDepth = serializedConfig.FindProperty ("giSettings.fgDepth");
		SerializedProperty diffuseBoost = serializedConfig.FindProperty ("giSettings.diffuseBoost");
		SerializedProperty fgRays = serializedConfig.FindProperty ("giSettings.fgRays");
		SerializedProperty fgMaxRayLength = serializedConfig.FindProperty ("giSettings.fgMaxRayLength");
		SerializedProperty fgAttenuationStart = serializedConfig.FindProperty ("giSettings.fgAttenuationStart");
		SerializedProperty fgAttenuationStop = serializedConfig.FindProperty ("giSettings.fgAttenuationStop");
		SerializedProperty fgFalloffExponent = serializedConfig.FindProperty ("giSettings.fgFalloffExponent");
		SerializedProperty fgInterpolationPoints = serializedConfig.FindProperty ("giSettings.fgInterpolationPoints");
		SerializedProperty fgEstimatePoints = serializedConfig.FindProperty ("giSettings.fgEstimatePoints");
		SerializedProperty fgCheckVisibility = serializedConfig.FindProperty ("giSettings.fgCheckVisibility");
		SerializedProperty fgContrastThreshold = serializedConfig.FindProperty ("giSettings.fgContrastThreshold");
		SerializedProperty fgGradientThreshold = serializedConfig.FindProperty ("giSettings.fgGradientThreshold");
		SerializedProperty fgNormalThreshold = serializedConfig.FindProperty ("giSettings.fgNormalThreshold");
		SerializedProperty fgClampRadiance = serializedConfig.FindProperty ("giSettings.fgClampRadiance");
		SerializedProperty fgAOInfluence = serializedConfig.FindProperty ("giSettings.fgAOInfluence");
		SerializedProperty fgAOContrast = serializedConfig.FindProperty ("giSettings.fgAOContrast");
		SerializedProperty fgAOMaxDistance = serializedConfig.FindProperty ("giSettings.fgAOMaxDistance");
		SerializedProperty fgAOScale = serializedConfig.FindProperty ("giSettings.fgAOScale");
		SerializedProperty fgPreview = serializedConfig.FindProperty ("giSettings.fgPreview");
		SerializedProperty fgUseCache = serializedConfig.FindProperty ("giSettings.fgUseCache");
		SerializedProperty fgCacheDirectLight = serializedConfig.FindProperty ("giSettings.fgCacheDirectLight");
		
		if (isPrimaryIntegrator && config.giSettings.primaryIntegrator != ILConfig.GISettings.Integrator.FinalGather)
			config.giSettings.primaryIntegrator = ILConfig.GISettings.Integrator.FinalGather;
		else if (!isPrimaryIntegrator && config.giSettings.secondaryIntegrator != ILConfig.GISettings.Integrator.FinalGather)
			config.giSettings.secondaryIntegrator = ILConfig.GISettings.Integrator.FinalGather;

		// Bounces
		
		GUILayout.Label ("Bounces", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		IntSlider (fgDepth, 1, 10, new GUIContent ("Bounces", "Sets the number of indirect light bounces calculated by final gather. A value higher than 1 will produce more global illumination effects, but note that it can be quite slow since the number of rays will increase exponentially with the depth. It's often better to use a fast method for secondary GI. If a secondary GI is used the number of set final gather bounces will be calculated first, before the secondary GI is called. So in most cases the depth should be set to 1 if a secondary GI is used."));
		EditorGUILayout.PropertyField (diffuseBoost, new GUIContent ("Bounce Boost", "This setting can be used to exaggerate light bouncing in dark scenes. Setting it to a value larger than 1 will push the diffuse color of materials towards 1 for GI computations. The typical use case is scenes authored with dark materials, this happens easily when doing only direct lighting since it's easy to compensate dark materials with strong light sources. Indirect light will be very subtle in these scenes since the bounced light will fade out quickly. Setting a diffuse boost will compensate for this. Note that values between 0 and 1 will decrease the diffuse setting in a similar way making light bounce less than the materials says, values below 0 is invalid. The actual computation taking place is a per component pow(colorComponent, (1.0 / diffuseBoost))."));
		EditorGUI.indentLevel--;

		// Rays

		GUILayout.Label ("Rays", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField (fgRays, new GUIContent ("Rays", "Sets the maximum number of rays to use for each Final Gather sample point. A higher number gives higher quality, but longer rendering time."));
		EditorGUILayout.PropertyField (fgMaxRayLength, new GUIContent ("Max Ray Length", "The max distance a ray can be traced before it's considered to be a 'miss'. This can improve performance in very large scenes. If the value is set to 0.0 the entire scene will be used."));
		
		// Attenuation
		
		GUILayout.Label ("Attenuation", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField (fgAttenuationStart, new GUIContent ("Attenuation Start", "The distance between which attenuation begins and fades to zero. There is no attenuation before this range, and no intensity beyond it. If zero, there will be no attenuation."));
		EditorGUILayout.PropertyField (fgAttenuationStop, new GUIContent ("Attenuation Stop", "The distance between which attenuation begins and fades to zero. There is no attenuation before this range, and no intensity beyond it. If zero, there will be no attenuation."));
		if (config.giSettings.fgAttenuationStop == 0)
			GUI.enabled = false;
		EditorGUILayout.PropertyField (fgFalloffExponent, new GUIContent ("Falloff Exponent", "This can be used to adjust the rate by which lighting falls off by distance. A higher exponent gives a faster falloff. Note that fgAttenuationStop must be set higher than 0.0 to enable attenuation."));
		if (config.giSettings.enableGI)
			GUI.enabled = true;
		EditorGUI.indentLevel--;

		// Points

		GUILayout.Label ("Points", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		IntSlider (fgInterpolationPoints, 1, 40, new GUIContent ("Interpolation Points", "Sets the number of final gather points to interpolate between. A higher value will give a smoother result, but can also smooth out details. If light leakage is introduced through walls when this value is increased, checking the sample visibility solves that problem, see fgCheckVisibility below."));
		IntSlider (fgEstimatePoints, 1, 40, new GUIContent ("Estimate Points", "Sets the minimum number of points that should be used when estimating final gather in the pre calculation pass. The impact is that a higher value will create more points all over the scene. The default value 15 rarely needs to be adjusted."));
		EditorGUILayout.PropertyField (fgCheckVisibility, new GUIContent ("Check Visibility", "Turn this on to reduce light leakage through walls. When points are collected to interpolate between, some of them can be located on the other side of geometry. As a result light will bleed through the geometry. So to prevent this Beast can reject points that are not visible."));
		EditorGUILayout.PropertyField (fgContrastThreshold, new GUIContent ("Contrast Threshold", "Controls how sensitive the final gather should be for contrast differences between the points during pre calculation. If the contrast difference is above this threshold for neighbouring points, more points will be created in that area. This tells the algorithm to place points where they are really needed, e.g. at shadow boundaries or in areas where the indirect light changes quickly. Hence this threshold controls the number of points created in the scene adaptively. Note that if a low number of final gather rays are used, the points will have high variance and hence a high contrast difference, so in that case you might need to increase the contrast threshold to prevent points from clumping together."));
		EditorGUILayout.PropertyField (fgGradientThreshold, new GUIContent ("Gradient Threshold", "Controls how the irradiance gradient is used in the interpolation. Each point stores it's irradiance gradient which can be used to improve the interpolation. However in some situations using the gradient can result in white 'halos' and other artifacts. This threshold can be used to reduce those artifacts."));
		EditorGUILayout.PropertyField (fgNormalThreshold, new GUIContent ("Normal Threshold", "Controls how sensitive the final gather should be for differences in the points normals. A lower value will give more points in areas of high curvature."));
		EditorGUILayout.PropertyField (fgClampRadiance, new GUIContent ("Clamp Radiance", "Turn this on to clamp the sampled values to [0, 1]. This will reduce high frequency noise when Final Gather is used together with other Global Illumination algorithms."));
		EditorGUI.indentLevel--;
		
		// Ambient Occlusion

		GUILayout.Label ("Ambient Occlusion", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		// Visualize AO is not available as of Unity 4.0
//		Toggle ("Visualize AO", ref config.giSettings.fgAOVisualize, "Visualize just the ambient occlusion values. Useful when tweaking the occlusion sampling options.");
		Slider (fgAOInfluence, 0, 1, new GUIContent ("Influence", "Controls a scaling of Final Gather with Ambient Occlusion which can be used to boost shadowing and get more contrast in you lighting. The value controls how much Ambient Occlusion to blend into the Final Gather solution."));
		LightmapEditorSettings.aoAmount = config.giSettings.fgAOInfluence;
		if (config.giSettings.fgAOInfluence <= 0)
			GUI.enabled = false;
		Slider (fgAOContrast, 0, 2, new GUIContent ("Contrast", "Can be used to adjust the contrast for ambient occlusion."));
		LightmapEditorSettings.aoContrast = config.giSettings.fgAOContrast;
		EditorGUILayout.PropertyField (fgAOMaxDistance, new GUIContent ("Max Distance", "Max distance for the occlusion. Beyond this distance a ray is considered to be visible. Can be used to avoid full occlusion for closed scenes."));
		LightmapEditorSettings.aoMaxDistance = config.giSettings.fgAOMaxDistance;
		EditorGUILayout.PropertyField (fgAOScale, new GUIContent ("Scale", "A scaling of the occlusion values. Can be used to increase or decrease the shadowing effect."));
		if (config.giSettings.enableGI)
			GUI.enabled = true;

		// Performance

		GUILayout.Label ("Performance", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField (fgPreview, new GUIContent ("Fast Preview", "Turn this on to visualize the final gather prepass. Using the Preview Calculation Pass enables a quick preview of the final image lighting, reducing lighting setup time."));
		EditorGUILayout.PropertyField (fgUseCache, new GUIContent ("Use Cache", "Selects what caching method to use for final gathering."));
		EditorGUILayout.PropertyField (fgCacheDirectLight, new GUIContent ("Cache Direct Light", "When this is enabled final gather will also cache lighting from light sources. This increases performance since fewer direct light calculations are needed. It gives an approximate result, and hence can affect the quality of the lighting. For instance indirect light bounces from specular highlights might be lost. However this caching is only done for depths higher than 1, so the quality of direct light and shadows in the light map will not be reduced."));
	}
	
	void PathTracerSettings (SerializedObject serializedConfig, bool isPrimaryIntegrator)
	{
		SerializedProperty ptDepth = serializedConfig.FindProperty ("giSettings.ptDepth");
		SerializedProperty ptAccuracy = serializedConfig.FindProperty ("giSettings.ptAccuracy");
		SerializedProperty ptPointSize = serializedConfig.FindProperty ("giSettings.ptPointSize");
		SerializedProperty ptNormalThreshold = serializedConfig.FindProperty ("giSettings.ptNormalThreshold");
		SerializedProperty ptFilterType = serializedConfig.FindProperty ("giSettings.ptFilterType");
		SerializedProperty ptFilterSize = serializedConfig.FindProperty ("giSettings.ptFilterSize");
		SerializedProperty ptCheckVisibility = serializedConfig.FindProperty ("giSettings.ptCheckVisibility");
		SerializedProperty ptPreview = serializedConfig.FindProperty ("giSettings.ptPreview");
		SerializedProperty ptCacheDirectLight = serializedConfig.FindProperty ("giSettings.ptCacheDirectLight");
		SerializedProperty ptPrecalcIrradiance = serializedConfig.FindProperty ("giSettings.ptPrecalcIrradiance");
		
		if (isPrimaryIntegrator && config.giSettings.primaryIntegrator != ILConfig.GISettings.Integrator.PathTracer)
			config.giSettings.primaryIntegrator = ILConfig.GISettings.Integrator.PathTracer;
		else if (!isPrimaryIntegrator && config.giSettings.secondaryIntegrator != ILConfig.GISettings.Integrator.PathTracer)
			config.giSettings.secondaryIntegrator = ILConfig.GISettings.Integrator.PathTracer;

		IntSlider (ptDepth, 0, 20, new GUIContent ("Bounces", ""));
		EditorGUILayout.PropertyField (ptAccuracy, new GUIContent ("Accuracy", "Sets the number of paths that are traced for each sample element (pixel, texel or vertex). For preview renderings, you can use a low value like 0.5 or 0.1, which means that half of the pixels or 1/10 of the pixels will generate a path. For production renderings you can use values above 1.0, if needed to get good quality."));
		// ptDefaultColor has no apparent effect as of Unity 4.0
//		LMColorPicker ("Default Color", ref config.giSettings.ptDefaultColor, "");

		// Points

		GUILayout.Label ("Points", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField (ptPointSize, new GUIContent ("Point Size", "Sets the maximum distance between the points in the path tracer cache. If set to 0 a value will be calculated automatically based on the size of the scene. The automatic value will be printed out during rendering, which is a good starting value if the point spacing needs to be adjusted."));
		EditorGUILayout.PropertyField (ptNormalThreshold, new GUIContent ("Normal Threshold", "Sets the amount of normal deviation that is allowed during cache point filtering."));
		EditorGUILayout.PropertyField (ptFilterType, new GUIContent ("Filter Type", "Selects the filter to use when querying the cache during rendering. None will return the closest cache point (unfiltered)."));
		EditorGUILayout.PropertyField (ptFilterSize, new GUIContent ("Filter Size", "Sets the size of the filter as a multiplier of the Cache Point Spacing value. For example; a value of 3.0 will use a filter that is three times larges then the cache point spacing. If this value is below 1.0 there is no guarantee that any cache point is found. If no cache point is found the Default Color will be returned instead for that query."));
		EditorGUILayout.PropertyField (ptCheckVisibility, new GUIContent ("Check Visibility", "Turn this on to reduce light leakage through walls. When points are collected to interpolate between, some of them can be located on the other side of geometry. As a result light will bleed through the geometry. So to prevent this Beast can reject points that are not visible."));

		// Performance

		GUILayout.Label ("Performance", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField (ptPreview, new GUIContent ("Fast Preview", "If enabled the pre-render pass will be visible in the render view."));
		EditorGUILayout.PropertyField (ptCacheDirectLight, new GUIContent ("Cache Direct Light", "When this is enabled the path tracer will also cache lighting from light sources. This increases performance since fewer direct light calculations are needed. It gives an approximate result, and hence can affect the quality of the lighting. For instance indirect light bounces from specular highlights might be lost."));
		EditorGUILayout.PropertyField (ptPrecalcIrradiance, new GUIContent ("Precalc Irradiance", "If enabled the cache points will be pre-filtered before the final pass starts. This increases the performance using the final render pass."));
	}

	void MonteCarloSettings (SerializedObject serializedConfig, bool isPrimaryIntegrator)
	{
		SerializedProperty mcDepth = serializedConfig.FindProperty ("giSettings.mcDepth");
		SerializedProperty mcRays = serializedConfig.FindProperty ("giSettings.mcRays");
		SerializedProperty mcMaxRayLength = serializedConfig.FindProperty ("giSettings.mcMaxRayLength");
		
		if (isPrimaryIntegrator && config.giSettings.primaryIntegrator != ILConfig.GISettings.Integrator.MonteCarlo)
			config.giSettings.primaryIntegrator = ILConfig.GISettings.Integrator.MonteCarlo;
		else if (!isPrimaryIntegrator && config.giSettings.secondaryIntegrator != ILConfig.GISettings.Integrator.MonteCarlo)
			config.giSettings.secondaryIntegrator = ILConfig.GISettings.Integrator.MonteCarlo;

		IntSlider (mcDepth, 1, 20, new GUIContent ("Bounces", "Sets the number of indirect light bounces calculated by monte carlo."));
		EditorGUILayout.PropertyField (mcRays, new GUIContent ("Rays", "Sets the number of rays to use for each calculation. A higher number gives higher quality, but longer rendering time."));
		EditorGUILayout.PropertyField (mcMaxRayLength, new GUIContent ("Ray Length", "The max distance a ray can be traced before it's considered to be a 'miss'. This can improve performance in very large scenes. If the value is set to 0.0 the entire scene will be used."));
	}

	void EnvironmentGUI (SerializedObject serializedConfig)
	{
		SerializedProperty giEnvironment = serializedConfig.FindProperty ("environmentSettings.giEnvironment");
		SerializedProperty giEnvironmentIntensity = serializedConfig.FindProperty ("environmentSettings.giEnvironmentIntensity");
		SerializedProperty iblImageFile = serializedConfig.FindProperty ("environmentSettings.iblImageFile");
		SerializedProperty iblSwapYZ = serializedConfig.FindProperty ("environmentSettings.iblSwapYZ");
		SerializedProperty iblTurnDome = serializedConfig.FindProperty ("environmentSettings.iblTurnDome");
		SerializedProperty iblGIEnvBlur = serializedConfig.FindProperty ("environmentSettings.iblGIEnvBlur");
		SerializedProperty iblEmitLight = serializedConfig.FindProperty ("environmentSettings.iblEmitLight");
		SerializedProperty iblSamples = serializedConfig.FindProperty ("environmentSettings.iblSamples");
		SerializedProperty iblIntensity = serializedConfig.FindProperty ("environmentSettings.iblIntensity");
		SerializedProperty iblEmitDiffuse = serializedConfig.FindProperty ("environmentSettings.iblEmitDiffuse");
		SerializedProperty iblEmitSpecular = serializedConfig.FindProperty ("environmentSettings.iblEmitSpecular");
		SerializedProperty iblSpecularBoost = serializedConfig.FindProperty ("environmentSettings.iblSpecularBoost");
		SerializedProperty iblShadows = serializedConfig.FindProperty ("environmentSettings.iblShadows");
		SerializedProperty iblBandingVsNoise = serializedConfig.FindProperty ("environmentSettings.iblBandingVsNoise");
		
		EditorGUILayout.PropertyField (giEnvironment, new GUIContent ("Environment Type", ""));
		
		EditorGUI.BeginDisabledGroup (config.environmentSettings.giEnvironment == ILConfig.EnvironmentSettings.Environment.None);

		EditorGUI.indentLevel++;

		EditorGUILayout.PropertyField (giEnvironmentIntensity, new GUIContent ("Intensity", ""));

		if (config.environmentSettings.giEnvironment == ILConfig.EnvironmentSettings.Environment.SkyLight) {
			// FIXME add undo
			LMColorPicker ("Sky Light Color", ref config.environmentSettings.skyLightColor, "It is often a good idea to keep the color below 1.0 in intensity to avoid boosting by gamma correction. Boost the intensity instead with the giEnvironmentIntensity setting.");
		} else if (config.environmentSettings.giEnvironment == ILConfig.EnvironmentSettings.Environment.IBL) {
			GUILayout.Label ("IBL Image", EditorStyles.boldLabel);
			EditorGUILayout.PrefixLabel (new GUIContent ("Image Path", "The absolute image file path to use for IBL. Accepts hdr or OpenEXR format. The file should be long-lat. Use giEnvironmentIntensity to boost the intensity of the image."));
			GUILayout.BeginHorizontal ();
			{
				GUILayout.Space (22);
				EditorGUILayout.PropertyField (iblImageFile, new GUIContent (""));
			}
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			{
				GUILayout.FlexibleSpace ();
				if (!string.IsNullOrEmpty (config.environmentSettings.iblImageFile)) {
					if (GUILayout.Button ("Reveal", GUILayout.Width (54))) {
						EditorUtility.OpenWithDefaultApp (Path.GetDirectoryName (config.environmentSettings.iblImageFile));
					}
					if (GUILayout.Button ("Edit", GUILayout.Width (54))) {
						EditorUtility.OpenWithDefaultApp (config.environmentSettings.iblImageFile);
					}
					GUILayout.Space (8);
				}
				if (GUILayout.Button ("Choose", GUILayout.Width (54))) {
					string file = EditorUtility.OpenFilePanel ("Select EXR or HDR file", "", "");
					string ext = Path.GetExtension (file);
					if (!string.IsNullOrEmpty (file)) {
						if (ext == ".exr" || ext == ".hdr") {
							config.environmentSettings.iblImageFile = file;
							iblImageFile.stringValue = file;
							GUI.changed = true;
							Repaint ();
							SaveConfig ();
							GUIUtility.ExitGUI ();
						} else {
							Debug.LogError ("IBL image files must use the extension .exr or .hdr");
						}
					}
				}
			}
			GUILayout.EndHorizontal ();
			EditorGUILayout.PropertyField (iblSwapYZ, new GUIContent ("Swap Y/Z", "Swap the Up Axis. Default value is false, meaning that Y is up."));
			Slider (iblTurnDome, 0, 360, new GUIContent ("Dome Rotation", "The sphere that the image is projected on can be rotated around the up axis. The amount of rotation is given in degrees. Default value is 0.0."));
			EditorGUILayout.PropertyField (iblGIEnvBlur, new GUIContent ("Blur", "Pre-blur the environment image for Global Illumination calculations. Can help to reduce noise and flicker in images rendered with Final Gather. May increase render time as it is blurred at render time. It is always cheaper to pre-blur the image itself in an external application before loading it into Beast."));

			GUILayout.Label ("IBL Light", EditorStyles.boldLabel);
			
			EditorGUILayout.PropertyField (iblEmitLight, new GUIContent ("Emit Light", "Turns on the expensive IBL implementation. This will generate a number of (iblSamples) directional lights from the image."));
			if (config.environmentSettings.iblEmitLight)
				EditorGUILayout.HelpBox ("The scene will be lit by a number of directional lights with colors sampled from the IBL image. Very expensive.", MessageType.None);
			else
				EditorGUILayout.HelpBox ("The scene will be lit with Global Illumination using the IBL image as a simple environment.", MessageType.None);
			
			EditorGUI.BeginDisabledGroup (!config.environmentSettings.iblEmitLight);
			{
				EditorGUILayout.PropertyField (iblSamples, new GUIContent ("Samples", "The number of samples to be taken from the image. This will affect how soft the shadows will be, as well as the general lighting. The higher number of samples, the better the shadows and lighting."));
				EditorGUILayout.PropertyField (iblIntensity, new GUIContent ("IBL Intensity", "Sets the intensity of the lighting."));
				EditorGUILayout.PropertyField (iblEmitDiffuse, new GUIContent ("Diffuse", "To remove diffuse lighting from IBL, set this to false. To get the diffuse lighting Final Gather could be used instead."));
				EditorGUILayout.PropertyField (iblEmitSpecular, new GUIContent ("Specular", "To remove specular highlights from IBL, set this to false."));
				EditorGUI.indentLevel++;
				{
					if (!config.environmentSettings.iblEmitSpecular)
						GUI.enabled = false;
					EditorGUILayout.PropertyField (iblSpecularBoost, new GUIContent ("Specular Boost", "Further tweak the intensity by boosting the specular component."));
					if (config.environmentSettings.iblEmitLight)
						GUI.enabled = true;
				}
				EditorGUI.indentLevel--;
				EditorGUILayout.PropertyField (iblShadows, new GUIContent ("Shadows", "Controls whether shadows should be created from IBL when this is used."));
				{
					EditorGUI.indentLevel++;
					if (!config.environmentSettings.iblShadows)
						GUI.enabled = false;
					EditorGUILayout.PropertyField (iblBandingVsNoise, new GUIContent ("Shadow Noise", "Controls the appearance of the shadows, banded shadows look more aliased, but noisy shadows flicker more in animations."));
					if (config.environmentSettings.iblEmitLight)
						GUI.enabled = true;
				}
				EditorGUI.indentLevel--;
			}
			EditorGUI.EndDisabledGroup ();
			
			EditorGUILayout.Space ();
		}
		EditorGUI.indentLevel--;
		
		EditorGUI.EndDisabledGroup ();
	}
	
	void TextureBakeGUI (SerializedObject serializedConfig)
	{
		SerializedProperty bilinearFilter = serializedConfig.FindProperty ("textureBakeSettings.bilinearFilter");
		SerializedProperty conservativeRasterization = serializedConfig.FindProperty ("textureBakeSettings.conservativeRasterization");
		
		GUILayout.Label ("Texture", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		
		// The following have no effect as of Unity 4.0
		
//		LMColorPicker ("Background Color", ref config.textureBakeSettings.bgColor, "");
//		IntField ("Edge Dilation", ref config.textureBakeSettings.edgeDilation, "Expands the lightmap with the number of pixels specified to avoid black borders.");
//		Toggle ("Premultiply", ref config.frameSettings.premultiply, "If this box is checked the alpha channel value is pre multiplied into the color channel of the pixel. Note that disabling premultiply alpha gives poor result if used with environment maps and other non constant camera backgrounds. Disabling premultiply alpha can be convenient when composing images in post.");
//		EditorGUI.indentLevel++;
//		if (!config.frameSettings.premultiply) {
//			GUI.enabled = false;
//		}
//		FloatField ("Premultiply Threshold", ref config.frameSettings.premultiplyThreshold, "This is the alpha threshold for pixels to be considered opaque enough to be 'un multiplied' when using premultiply alpha.");
//		EditorGUI.indentLevel--;
//		GUI.enabled = true;
		
		EditorGUILayout.PropertyField (bilinearFilter, new GUIContent ("Bilinear Filter", "Counteract unwanted light seams for tightly packed UV patches."));
		EditorGUILayout.PropertyField (conservativeRasterization, new GUIContent ("Conservative Rasterization", "Find pixels which are only partially covered by the UV map."));

		EditorGUI.indentLevel--;
	}
	
	#endregion
	
	
	#region Bake
	
	enum BakeMode
	{
		BakeScene,
		BakeSelected,
		BakeProbes,
	}
	
	void BakeButtonsGUI ()
	{
		float width = 120;
		bool disabled = LightmapSettings.lightmapsMode == LightmapsMode.Directional && !InternalEditorUtility.HasPro ();
		EditorGUI.BeginDisabledGroup (disabled);
		{
			GUILayout.BeginHorizontal ();
			{
				GUILayout.FlexibleSpace ();
				if (GUILayout.Button ("Clear", GUILayout.Width (width))) {
					Lightmapping.Clear ();
				}
				if (!Lightmapping.isRunning) {
					if (BakeButton (GUILayout.Width (width))) {
						this.DoBake ();
						GUIUtility.ExitGUI ();
					}
				} else {
					if (GUILayout.Button ("Cancel", GUILayout.Width (width))) {
						Lightmapping.Cancel ();
					}
				}
			}
			GUILayout.EndHorizontal ();
		}
		EditorGUI.EndDisabledGroup ();
	}
	
	private bool BakeButton (params GUILayoutOption[] options)
	{
		GUIContent content = new GUIContent (ObjectNames.NicifyVariableName (bakeMode.ToString ()));
		
		Rect dropdownRect = GUILayoutUtility.GetRect (content, (GUIStyle)"DropDownButton", options);
		Rect buttonRect = dropdownRect;
		buttonRect.xMin = buttonRect.xMax - 20;
		if (Event.current.type != EventType.MouseDown || !buttonRect.Contains (Event.current.mousePosition))
			return GUI.Button (dropdownRect, content, (GUIStyle)"DropDownButton");
		GenericMenu genericMenu = new GenericMenu ();
		string[] names = Enum.GetNames (typeof(BakeMode));
		int num1 = Array.IndexOf<string> (names, Enum.GetName (typeof(BakeMode), this.bakeMode));
		int num2 = 0;
		foreach (string text in Enumerable.Select<string, string> (names, x => ObjectNames.NicifyVariableName(x))) {
			genericMenu.AddItem (new GUIContent (text), num2 == num1, new GenericMenu.MenuFunction2 (this.BakeDropDownCallback), num2++);
		}
		genericMenu.DropDown (dropdownRect);
		Event.current.Use ();
		return false;
	}
	
	void BakeDropDownCallback (object data)
	{
		if (CheckSettingsIntegrity ()) {	
			bakeMode = (BakeMode)data;
			DoBake ();
		}
	}
	
	BakeMode bakeMode {
		get {
			return (BakeMode)EditorPrefs.GetInt ("LightmapEditor.BakeMode", 0);
		}
		set {
			EditorPrefs.SetInt ("LightmapEditor.BakeMode", (int)value);
		}
	}
	
	void DoBake ()
	{
		switch (bakeMode) {
		case BakeMode.BakeScene:
			Lightmapping.BakeAsync ();
			break;
		case BakeMode.BakeSelected:
			Lightmapping.BakeSelectedAsync ();
			break;
		case BakeMode.BakeProbes:
			Lightmapping.BakeLightProbesOnlyAsync ();
			break;
		}
	}
	
	bool CheckSettingsIntegrity ()
	{
		if (config.environmentSettings.giEnvironment == ILConfig.EnvironmentSettings.Environment.IBL) {
			if (string.IsNullOrEmpty (config.environmentSettings.iblImageFile)) {
				EditorUtility.DisplayDialog ("Missing IBL image", "The lightmapping environment type is set to IBL, but no IBL image file is available. Either change the environment type or specify an HDR or EXR image file path.", "Ok");
				Debug.LogError ("Lightmapping cancelled, environment type set to IBL but no IBL image file was specified.");
				return false;
			} else if (!File.Exists (config.environmentSettings.iblImageFile)) {
				EditorUtility.DisplayDialog ("Missing IBL image", "The lightmapping environment type is set to IBL, but there is no compatible image file at the specified path. Either change the environment type or specify an absolute path to an HDR or EXR image file.", "Ok");
				Debug.LogError ("Lightmapping cancelled, environment type set to IBL but the absolute path to an IBL image is incorrect.");
				return false;
			}
		}
		return true;
	}
	
	#endregion
	
	
	#region GUI Elements
	
	private void LMColorPicker (string name, ref ILConfig.LMColor color, string tooltip)
	{
		Color c = EditorGUILayout.ColorField (new GUIContent (name, tooltip), new Color (color.r, color.g, color.b, color.a));
		color = new ILConfig.LMColor (c.r, c.g, c.b, c.a);
	}

	private void Slider (SerializedProperty val, float min, float max, GUIContent content)
	{
		EditorGUILayout.Slider (val, min, max, content);
	}
	
	private void Toggle (string name, ref bool val, string tooltip)
	{
		val = EditorGUILayout.Toggle (new GUIContent (name, tooltip), val);
	}
	
	private void FloatField (string name, ref float val, string tooltip)
	{
		val = EditorGUILayout.FloatField (new GUIContent (name, tooltip), val);
	}
	
	private void IntField (string name, ref int val, string tooltip)
	{
		val = EditorGUILayout.IntField (new GUIContent (name, tooltip), val);
	}

	private void IntSlider (SerializedProperty val, int min, int max, GUIContent content)
	{
		EditorGUILayout.IntSlider (val, min, max, content);
	}

//	private void MinMaxField (SerializedProperty min, SerializedProperty max, GUIContent content)
//	{
//		GUILayout.BeginHorizontal ();
//		GUILayout.Space (15 * EditorGUI.indentLevel);
//		GUILayout.Label (content);
//		EditorGUILayout.PropertyField (min, new GUIContent (""), GUILayout.Width (30));
//		if (min.floatValue < 0)
//			min.floatValue = 0;
//		if (min.floatValue > max.floatValue)
//			max.floatValue = min.floatValue;
//		EditorGUILayout.PropertyField (max, new GUIContent (""), GUILayout.Width (30));
//		if (max.floatValue < 0)
//			max.floatValue = 0;
//		if (max.floatValue < min.floatValue)
//			min.floatValue = max.floatValue;
//		GUILayout.EndHorizontal ();
//	}
	
	#endregion
	
}



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
	

	public enum ShadowDepth {
		PrimaryRays = 0,
		PrimaryAndSecondaryRays = 1
	}

	void RenderSettingsGUI ()
	{
		GUILayout.Label ("Rays");
		IntField ("Max Bounces", ref config.renderSettings.maxRayDepth, "The maximum amount of ”bounces” a ray can have before being considered done. A bounce can be a reflection or refraction.");
		FloatField ("Bias", ref config.renderSettings.bias, "An error threshold to avoid double intersections. For example, a shadow ray should not intersect the same triangle as the primary ray did, but because of limited numerical precision this can happen. The bias value moves the intersection point to eliminate this problem. If set to zero this value is computed automatically depending on the scene size.");

		GUILayout.Label ("Reflections & Transparency");
		IntField ("Max Reflection Bounces", ref config.renderSettings.reflectionDepth, "The maximum amount of reflections a ray can have before being considered done.");
		FloatField ("Reflection Threshold", ref config.renderSettings.reflectionThreshold, "If the intensity of the reflected contribution is less than the threshold, the ray will be terminated.");
		IntField ("GI Transparency Depth", ref config.renderSettings.giTransparencyDepth, "Controls the maximum transparency depth for Global Illumination rays. Used to speed up renderings with a lot of transparency (for example trees).");

		GUILayout.Label ("Shadows");
		config.renderSettings.shadowDepth = (int)((ShadowDepth)EditorGUILayout.EnumPopup (new GUIContent ("Shadow Depth", "Controls which rays that spawn shadow rays."), (ShadowDepth)System.Enum.Parse (typeof(ShadowDepth), config.renderSettings.shadowDepth.ToString ())));
		IntField ("Min Shadow Rays", ref config.renderSettings.minShadowRays, "The minimum number of shadow rays that will be sent to determine if a point is lit by a specific light source. Use this value to ensure that you get enough quality in soft shadows at the price of render times. This will raise the minimum number of rays sent for any light sources that have a minShadowSamples setting lower than this value, but will not lower the number if minShadowSamples is set to a higher value. Setting this to a value higher than maxShadowRays will not send more rays than maxShadowRays.");
		IntField ("Max Shadow Rays", ref config.renderSettings.maxShadowRays, "The maximum number of shadow rays per point that will be used to generate a soft shadow for any light source. Use this to shorten render times at the price of soft shadow quality. This will lower the maximum number of rays sent for any light sources that have a shadow samples setting higher than this value, but will not raise the number if shadow samples is set to a lower value.");

		GUILayout.Label ("Geometry");
		FloatField ("Vertex Merge Threshold", ref config.renderSettings.vertexMergeThreshold, "Triangle vertices that are closer together than this threshold will be merged into one (if possible depending on other vertex data).");
		Toggle ("Odd UV Flipping", ref config.renderSettings.tsOddUVFlipping, "Using this setting will force Beast to mirror tangent and binormal when UV has odd winding direction.");
		Toggle ("Vertex Orthogonalization", ref config.renderSettings.tsVertexOrthogonalization, "Orthogonalize tangent space basis vectors (tangent, binormal and normal) at every vertex.");
		Toggle ("Vertex Normalization", ref config.renderSettings.tsVertexNormalization, "Normalize tangent space basis vectors (tangent, binormal and normal) at every vertex.");
		Toggle ("Intersection Orthogonalization", ref config.renderSettings.tsIntersectionOrthogonalization, "Orthogonalize tangent space basis vectors (tangent, binormal and normal) at every intersection point.");
		Toggle ("Intersection Normalization", ref config.renderSettings.tsIntersectionNormalization, "Normalize tangent space basis vectors (tangent, binormal and normal) at every intersection point.");
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
			Toggle ("Fast Preview", ref config.giSettings.fgPreview, "Turn this on to visualize the final gather prepass. Using the Preview Calculation Pass enables a quick preview of the final image lighting, reducing lighting setup time.");
			IntField ("Rays", ref config.giSettings.fgRays, "Sets the maximum number of rays to use for each Final Gather sample point. A higher number gives higher quality, but longer rendering time.");
			FloatField ("Contrast Threshold", ref config.giSettings.fgContrastThreshold, "Controls how sensitive the final gather should be for contrast differences between the points during pre calculation. If the contrast difference is above this threshold for neighbouring points, more points will be created in that area. This tells the algorithm to place points where they are really needed, e.g. at shadow boundaries or in areas where the indirect light changes quickly. Hence this threshold controls the number of points created in the scene adaptively. Note that if a low number of final gather rays are used, the points will have high variance and hence a high contrast difference, so in that case you might need to increase the contrast threshold to prevent points from clumping together.");
			FloatField ("Gradient Threshold", ref config.giSettings.fgGradientThreshold, "Controls how the irradiance gradient is used in the interpolation. Each point stores it's irradiance gradient which can be used to improve the interpolation. However in some situations using the gradient can result in white ”halos” and other artifacts. This threshold can be used to reduce those artifacts.");
			IntSlider ("Interpolation Points", ref config.giSettings.fgInterpolationPoints, 1, 50, "Sets the number of final gather points to interpolate between. A higher value will give a smoother result, but can also smooth out details. If light leakage is introduced through walls when this value is increased, checking the sample visibility solves that problem, see fgCheckVisibility below.");
			Toggle ("Check Visibility", ref config.giSettings.fgCheckVisibility, "Turn this on to reduce light leakage through walls. When points are collected to interpolate between, some of them can be located on the other side of geometry. As a result light will bleed through the geometry. So to prevent this Beast can reject points that are not visible.");
			
			EditorGUILayout.Space ();
			
			IntSlider ("Bounces", ref config.giSettings.fgDepth, 1, 10, "Sets the number of indirect light bounces calculated by final gather. A value higher than 1 will produce more global illumination effects, but note that it can be quite slow since the number of rays will increase exponentially with the depth. It's often better to use a fast method for secondary GI. If a secondary GI is used the number of set final gather bounces will be calculated first, before the secondary GI is called. So in most cases the depth should be set to 1 if a secondary GI is used.");
			EditorGUI.indentLevel = 2;
			FloatField ("Boost", ref config.giSettings.diffuseBoost, "This setting can be used to exaggerate light bouncing in dark scenes. Setting it to a value larger than 1 will push the diffuse color of materials towards 1 for GI computations. The typical use case is scenes authored with dark materials, this happens easily when doing only direct lighting since it's easy to compensate dark materials with strong light sources. Indirect light will be very subtle in these scenes since the bounced light will fade out quickly. Setting a diffuse boost will compensate for this. Note that values between 0 and 1 will decrease the diffuse setting in a similar way making light bounce less than the materials says, values below 0 is invalid. The actual computation taking place is a per component pow(colorComponent, (1.0 / diffuseBoost)).");
			FloatField ("Intensity", ref config.giSettings.primaryIntensity, "Tweak the amount of illumination from the primary and secondary GI integrators. This lets you boost or reduce the amount of indirect light easily.");
			LightmapEditorSettings.bounceIntensity = config.giSettings.primaryIntensity;
			FloatField ("Saturation", ref config.giSettings.primarySaturation, "Lets you tweak the amount of color in the primary and secondary GI integrators. This lets you boost or reduce the perceived saturation of the bounced light.");
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
		Slider ("Ambient Occlusion", ref config.giSettings.fgAOInfluence, 0, 1, "Controls a scaling of Final Gather with Ambient Occlusion which can be used to boost shadowing and get more contrast in you lighting. The value controls how much Ambient Occlusion to blend into the Final Gather solution.");
		LightmapEditorSettings.aoAmount = config.giSettings.fgAOInfluence;
		EditorGUI.indentLevel = 2;
		if (config.giSettings.fgAOInfluence <= 0)
			GUI.enabled = false;
		
		Slider ("Contrast", ref config.giSettings.fgAOContrast, 0, 2, "Can be used to adjust the contrast for ambient occlusion.");
		LightmapEditorSettings.aoContrast = config.giSettings.fgAOContrast;
		
		FloatField ("Max Distance", ref config.giSettings.fgAOMaxDistance, "Max distance for the occlusion. Beyond this distance a ray is considered to be visible. Can be used to avoid full occlusion for closed scenes.");
		LightmapEditorSettings.aoMaxDistance = config.giSettings.fgAOMaxDistance;
		
		FloatField ("Scale", ref config.giSettings.fgAOScale, "A scaling of the occlusion values. Can be used to increase or decrease the shadowing effect.");
		
		GUI.enabled = true;
	}
	
	void PathTracerSettings ()
	{
		if (EditorGUILayout.BeginToggleGroup ("Path Tracer", config.giSettings.secondaryIntegrator == ILConfig.GISettings.Integrator.PathTracer)) {
			config.giSettings.secondaryIntegrator = ILConfig.GISettings.Integrator.PathTracer;
		} else {
			config.giSettings.secondaryIntegrator = ILConfig.GISettings.Integrator.None;
		}
		FloatField ("Intensity", ref config.giSettings.secondaryIntensity, "Tweak the amount of illumination from the primary and secondary GI integrators. This lets you boost or reduce the amount of indirect light easily.");
		FloatField ("Saturation", ref config.giSettings.secondarySaturation, "Lets you tweak the amount of color in the primary and secondary GI integrators. This lets you boost or reduce the perceived saturation of the bounced light.");
		FloatField ("Accuracy", ref config.giSettings.ptAccuracy, "Sets the number of paths that are traced for each sample element (pixel, texel or vertex). For preview renderings, you can use a low value like 0.5 or 0.1, which means that half of the pixels or 1/10 of the pixels will generate a path. For production renderings you can use values above 1.0, if needed to get good quality.");
		FloatField ("Point Size", ref config.giSettings.ptPointSize, "Sets the maximum distance between the points in the path tracer cache. If set to 0 a value will be calculated automatically based on the size of the scene. The automatic value will be printed out during rendering, which is a good starting value if the point spacing needs to be adjusted.");
		Toggle ("Cache Direct Light", ref config.giSettings.ptCacheDirectLight, "When this is enabled the path tracer will also cache lighting from light sources. This increases performance since fewer direct light calculations are needed. It gives an approximate result, and hence can affect the quality of the lighting. For instance indirect light bounces from specular highlights might be lost.");
		Toggle ("Check Visibility", ref config.giSettings.ptCheckVisibility, "Turn this on to reduce light leakage through walls. When points are collected to interpolate between, some of them can be located on the other side of geometry. As a result light will bleed through the geometry. So to prevent this Beast can reject points that are not visible.");
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
		FloatField ("Intensity", ref config.environmentSettings.giEnvironmentIntensity, "");
		
		GUI.enabled = true;
	}
	
	
	void TextureBakeGUI ()
	{
		
	}

	private void Slider (string name, ref float val, float min, float max, string tooltip)
	{
		EditorGUILayout.Slider (new GUIContent (name, tooltip), val, min, max);
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

	private void IntSlider (string name, ref int val, int min, int max, string tooltip)
	{
		GUI.tooltip = tooltip;
		val = EditorGUILayout.IntSlider (new GUIContent (name, tooltip), val, min, max);
	}
}

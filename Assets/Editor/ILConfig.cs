using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.IO;


[System.Serializable]
public class ILConfig
{
	#region Data types

	[System.Serializable]
	public class LMVec2
	{
		public float x;
		public float y;
	
		public LMVec2 ()
		{
			x = 0;
			y = 0;
		}
	
		public LMVec2 (float x, float y)
		{
			this.x = x;
			this.y = y;
		}
	}
	
	[System.Serializable]
	public class LMColor
	{
		public float r;
		public float g;
		public float b;
		public float a;
	
		public LMColor ()
		{
			this.r = 1;
			this.g = 1;
			this.b = 1;
			this.a = 1;
		}
	
		public LMColor (float r, float g, float b, float a)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}
	}
	
	#endregion
	
	[XmlElement(ElementName = "AASettings")]
	public AASettings aaSettings;
	[XmlElement(ElementName = "RenderSettings")]
	public RenderSettings renderSettings;
	[XmlElement(ElementName = "EnvironmentSettings")]
	public EnvironmentSettings environmentSettings;
	[XmlElement(ElementName = "FrameSettings")]
	public FrameSettings frameSettings;
	[XmlElement(ElementName = "GISettings")]
	public GISettings giSettings;
	[XmlElement(ElementName = "SurfaceTransferSettings")]
	public SurfaceTransferSettings surfaceTransferSettings;
	[XmlElement(ElementName = "TextureBakeSettings")]
	public TextureBakeSettings textureBakeSettings;
	
	
	public static ILConfig Load (string path)
	{
		FileInfo info = new FileInfo (path);
		if (!info.Exists) {
			return null;
		}
		
		XmlSerializer serializer = new XmlSerializer (typeof(ILConfig));
		FileStream stream = new FileStream (path, FileMode.Open);
		
		ILConfig config = (ILConfig)serializer.Deserialize (stream);
		stream.Close ();
	
		return config;
	}

	public void Save (string path)
	{	
		using (XmlTextWriter writer = new XmlTextWriter (path, System.Text.Encoding.GetEncoding ("ISO-8859-1"))) {
			XmlSerializerNamespaces ns = new XmlSerializerNamespaces ();
			ns.Add (string.Empty, string.Empty);
			writer.Formatting = Formatting.Indented;
			XmlSerializer serializer = new XmlSerializer (typeof(ILConfig));
			serializer.Serialize (writer, this, ns);
		}
	}
	
	
	
	[System.Serializable]
	public class FrameSettings
	{
		public enum TileScheme
		{
			LeftToRight,
			Hilbert,
			Random,
			Concentric
		}
		
		public enum ColorCorrection
		{
			None,
			Gamma,
			SRGB
		}
		
		[System.Serializable]
		public class OutputVerbosity
		{
			public bool errorPrint = true;
			public bool warningPrint = true;
			public bool benchmarkPrint = false;
			public bool progressPrint = true;
			public bool infoPrint = false;
			public bool verbosePrint = false;
			public bool debugPrint = false;
		}
	
		[System.Serializable]
		public class OutputCorrection
		{
			public ColorCorrection colorCorrection = ColorCorrection.None;
			public float gamma = 1;
		}
		
		public bool autoThreads = true;
		public int autoThreadsSubtract = 0;
		public int renderThreads = 2;
		public bool dither = true;
		public float inputGamma = 1.0f;
		public OutputCorrection outputCorrection;
		public TileScheme tileScheme = TileScheme.Hilbert;
		public bool premultiply = true;
		public float premultiplyThreshold = 0.0f;
		public OutputVerbosity outputVerbosity;
	}
	
	
	
	[System.Serializable]
	public class RenderSettings
	{
		public float bias = 0.005f;
		public int giTransparencyDepth = 2;
		public bool ignoreLightLinks = false;
		public int maxRayDepth = 6;
		public long maxShadowRays = 4294967295;
		public int minShadowRays = 0;
		public int reflectionDepth = 2;
		public float reflectionThreshold = 0.001f;
		public int shadowDepth = 2;
		public bool shadowsIgnoreLightLinks = false;
		public int transparencyDepth = 50;
		public bool tsIntersectionNormalization = true;
		public bool tsIntersectionOrthogonalization = true;
		public bool tsOddUVFlipping = true;
		public bool tsVertexNormalization = true;
		public bool tsVertexOrthogonalization = true;
		public float vertexMergeThreshold = 0.001f;
	}
	
	
	
	[System.Serializable]
	public class AASettings
	{
		public enum SamplingMode {
			/// <summary>
			/// Anti-aliasing scheme for under/over sampling (from 1/256 up to 256 samples per pixel)
			/// </summary>
			Adaptive,
			/// <summary>
			/// Anti-aliasing scheme for super sampling (from 1 up to 128 samples per pixel)
			/// </summary>
			SuperSampling
		}
		
		public enum Filter {
			/// <summary>
			/// Each sample is treated as equally important. The fastest filter to execute but it gives blurry results.
			/// </summary>
			Box,
			CatmullRom,
			Cubic,
			/// <summary>
			/// Removes noise, preserves details.
			/// </summary>
			Gauss,
			Lanczos,
			Mitchell,
			/// <summary>
			/// Distant samples are considered less important.
			/// </summary>
			Triangle,
		}
		
		public int minSampleRate = 0;
		public int maxSampleRate = 2;
		public float contrast = 0.1f;
		public bool diagnose = false;
		public bool clamp = false;
		public SamplingMode samplingMode = SamplingMode.Adaptive;
		public Filter filter = Filter.Box;
		public LMVec2 filterSize = new LMVec2 (1, 1);
	}
	
	
	[System.Serializable]
	/// <summary>
	/// Environment settings control what happens if a ray misses all geometry in the scene. 
	/// </summary>
	/// <remarks>
	/// Defining an environment is usually a very good way to get very pleasing outdoor illumination results,
	/// but might also increase bake times.
	/// 
	/// Note that environments should only be used for effects that can be considered to be infinitely far away,
	/// meaning that only the directional component matters.
	/// </remarks>
	public class EnvironmentSettings
	{
		// 
		public enum Environment {
			None,
			/// <summary>
			/// A constant color.
			/// </summary>
			SkyLight,
			/// <summary>
			/// An HDR image.
			/// </summary>
			IBL
		}
		
		/// <summary>
		/// The type of Environment: None, Skylight or IBL.
		/// </summary>
		public Environment giEnvironment = Environment.SkyLight;
		/// <summary>
		/// A constant environment color. In Unity: "Sky Light Color"
		/// </summary>
		/// <remarks>
		/// Used if type is Skylight. It is often a good idea to keep the color below 1.0 in intensity
		/// to avoid boosting by gamma correction. Boost the intensity instead with the giEnvironmentIntensity setting.
		/// </remarks>
		public LMColor skyLightColor = new LMColor (0.86f, 0.93f, 1, 1);
		/// <summary>
		/// A scale factor for Global Illumination intensity. In Unity: "Sky Light Intensity"
		/// </summary>
		/// <remarks>
		/// Used for avoiding gamma correction errors and to scale HDR textures to something that fits your scene.
		/// </remarks>
		public float giEnvironmentIntensity = 0;
		/// <summary>
		/// The image file to use for IBL, using an absolute path.
		/// </summary>
		/// <remarks>
		/// Accepts hdr or OpenEXR format. The file should be long-lat. Use giEnvironmentIntensity to boost the intensity of the image.
		/// </remarks>
		public string iblImageFile;
		public float iblBandingVsNoise = 1;
		public bool iblEmitDiffuse = true;
		public bool iblEmitLight = false;
		public bool iblEmitSpecular = false;
		public float iblGIEnvBlur = 0.05f;
		public float iblIntensity = 1;
		public int iblSamples = 300;
		public bool iblShadows = true;
		public float iblSpecularBoost = 1;
		public bool iblSwapYZ = false;
		public float iblTurnDome = 0;
	}
	
	
	[System.Serializable]
	public class GISettings
	{
		public enum ClampMaterials {
			None,
			Component,
			Intensity,
		}
		
		public enum Integrator {
			None,
			FinalGather,
			/// <summary>
			/// Used if many indirect bounces are needed and Final Gather-only solution with acceptable
			/// quality would take to much time to render.
			/// </summary>
			PathTracer,
			MonteCarlo
		}
		
		
		// Global Illumination
		
		public ClampMaterials clampMaterials = ClampMaterials.None;
		public float diffuseBoost = 1;
		public float emissiveScale = 1;
		public bool enableCaustics = false;
		public bool enableGI = true;
		public bool ignoreNonDiffuse = true;
		public int prepassMaxSampleRate = 0;
		public int prepassMinSampleRate = -4;
		public Integrator primaryIntegrator = Integrator.FinalGather;
		public float primaryIntensity = 1;
		public float primarySaturation = 1;
		public Integrator secondaryIntegrator = Integrator.PathTracer;
		public float secondaryIntensity = 1;
		public float secondarySaturation = 1;
		public float specularScale = 1;
		
		
		// Final Gather
		
		public enum Cache {
			Off,
			Irradiance,
			RadianceSH
		}
		
		public float fgAOContrast = 1;
		public float fgAOInfluence = 0;
		public float fgAOMaxDistance = 0;
		public float fgAOScale = 1;
		public bool fgAOVisualize = false;
		public float fgAccuracy = 1;
		public float fgAttenuationStart = 0;
		public float fgAttenuationStop = 0;
		public bool fgCacheDirectLight = false;
		public bool fgCheckVisibility = false;
		public float fgCheckVisibilityDepth = 1;
		public bool fgClampRadiance = false;
		public float fgContrastThreshold = 0.1f;
		public int fgDepth = 1;
		public bool fgDisableMinRadius = false;
		public int fgEstimatePoints = 15;
		public bool fgExploitFrameCoherence = false;
		public float fgFalloffExponent = 0;
		public float fgGradientThreshold = 0.5f;
		public int fgInterpolationPoints = 15;
		public float fgLightLeakRadius = 0;
		public bool fgLightLeakReduction = false;
		public float fgMaxRayLength = 0;
		public float fgNormalThreshold = 0.2f;
		public bool fgPreview = true;
		public int fgRays = 300;
		public float fgSmooth = 1;
		public Cache fgUseCache = Cache.Irradiance;
		public bool fgUseLegacy = false;
		
		
		// PathTracer
		
		public enum PTFilterType {
			None,
			Box,
			Gauss,
			Triangle
		}
		
		public float ptAccuracy = 1;
		public bool ptCacheDirectLight = true;
		public bool ptCheckVisibility = false;
		public float ptConservativeEnergyLimit = 0.95f;
		public LMColor ptDefaultColor = new LMColor (0, 0, 0, 1);
		public int ptDepth = 5;
		public bool ptDiffuseIllum = true;
		public string ptFile = "";
		public float ptFilterSize = 3;
		public PTFilterType ptFilterType = PTFilterType.Box;
		public float ptNormalThreshold = 0.707f;
		public float ptPointSize = 0;
		public bool ptPrecalcIrradiance = true;
		public bool ptPreview = true;
		public bool ptSpecularIllum = true;
		public bool ptTransmissiveIllum = true;
		
		
		// Monte Carlo
		
		public int mcDepth = 2;
    	public float mcRayLength = 0;
    	public int mcRays = 16;
	}
	
	
	[System.Serializable]
	public class ADSSettings
	{
		public enum Instancing {
			None,
			AutoDetect
		}
		
		public int gridDensity = 1;
	    public int gridMaxDepth = 4;
	    public int gridThreshold = 25;
	    public int instancingThreshold = 500;
	    public Instancing useInstancing = Instancing.AutoDetect;
	    public bool useSSE = true;
	}
	
	
	[System.Serializable]
	public class SurfaceTransferSettings
	{
		public enum SelectionMode {
			Normal
		}
		
		public float frontRange = 0;
		public float frontBias = 0;
		public float backRange = 2;
		public float backBias = -1;
		public SelectionMode selectionMode = SelectionMode.Normal;
	}
	
	
	[System.Serializable]
	public class TextureBakeSettings
	{
		public LMColor bgColor = new LMColor (1, 1, 1, 1);
		/// <summary>
		/// Counteract unwanted light seams for tightly packed UV patches.
		/// </summary>
		public bool bilinearFilter = true;
		/// <summary>
		/// Find pixels which are only partially covered by the UV map.
		/// </summary>
		public bool conservativeRasterization = true;
		/// <summary>
		/// Expands the lightmap with the number of pixels specified to avoid black borders.
		/// </summary>
		public float edgeDilation = 3;
	}
}


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
			/// <summary>
			/// The filter kernel is a tent which means that distant samples are considered less important.
			/// </summary>
			Triangle,
			/// <summary>
			/// Uses the Gauss function as filter kernel. This gives the best results (removes noise, preserves details).
			/// </summary>
			Gauss
		}
		
		public SamplingMode samplingMode = SamplingMode.Adaptive;
		public bool clamp = false;
		public float contrast = 0.1f;
		public bool diagnose = false;
		public int minSampleRate = 0;
		public int maxSampleRate = 2;
		
		public Filter filter = Filter.Gauss;
		public LMVec2 filterSize = new LMVec2 (2.2f, 2.2f);
	}
	
	[System.Serializable]
	public class RenderSettings
	{
		public float bias = 0;
		public int maxShadowRays = 10000;
		public int maxRayDepth = 6;
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
	}
	
	[System.Serializable]
	public class FrameSettings
	{
		public float inputGamma = 1;
	}
	
	[System.Serializable]
	public class GISettings
	{
		public enum Integrator {
			None,
			/// <summary>
			/// Used if many indirect bounces are needed and Final Gather-only solution with acceptable
			/// quality would take to much time to render.
			/// </summary>
			PathTracer,
			FinalGather
		}
		
		public bool enableGI = true;
		/// <summary>
		/// This setting can be used to exaggerate light bouncing in dark scenes. In Unity: "Bounce Boost"
		/// </summary>
		/// <remarks>
		/// Setting it to a value larger than 1 will push the diffuse color of materials towards 1 for GI computations.
		/// The typical use case is scenes authored with dark materials, this happens easily when doing only direct
		/// lighting since it's easy to compensate dark materials with strong light sources. Indirect light will
		/// be very subtle in these scenes since the bounced light will fade out quickly. Setting a diffuse
		/// boost will compensate for this. Note that values between 0 and 1 will decrease the diffuse setting
		/// in a similar way making light bounce less than the materials says, values below 0 is invalid. The
		/// actual computation taking place is a per component pow(colorComponent, (1.0 / diffuseBoost)).
		/// </remarks>
		public float diffuseBoost = 1;
		
		// ----------------------
		// Final Gather - Quality
		// ----------------------
		
		public bool fgPreview = false;
		/// <summary>
		/// The maximum number of rays taken in each Final Gather sample. In Unity: "Final Gather Rays"
		/// </summary>
		/// <remarks>
		/// More rays gives better results but take longer to evaluate.
		/// </remarks>
		public int fgRays = 1000;
		/// <summary>
		/// Controls how sensitive the final gather should be for contrast differences between the points during precalculation.
		/// In Unity: "Contrast Threshold"
		/// </summary>
		/// <remarks>
		/// If the contrast difference is above this threshold for neighbouring points, more points will be created
		/// in that area. This tells the algorithmto place points where they are really needed, e.g. at shadow boundaries
		/// or in areas where the indirect light changes quickly. Hence this threshold controls the number of points created
		/// in the scene adaptively. Note that if a low number of final gather rays are used, the points will have high
		/// variance and hence a high contrast difference. In that the case contrast threshold needs to be raised to prevent
		/// points from clumping together or using more rays per sample.
		/// </remarks>
		public float fgContrastThreshold = 0.05f;
		/// <summary>
		/// Lower values remove white halos. In Unity: "Interpolation"
		/// </summary>
		/// <remarks>
		/// Each point stores its irradiance gradient which can be used to improve the interpolation.
		/// In some situations using the gradient can result in white "halos" and other artifacts.
		/// </remarks>
		public float fgGradientThreshold = 0;
		/// <summary>
		/// How much a normal can differ in the cache, given as cos(a).
		/// </summary>
//		public float fgNormalThreshold = 0.2f;
		
		// -------------------
		// Final Gather - Look
		// -------------------
		
		/// <summary>
		/// Controls the number of indirect light bounces. In Unity: "Bounces"
		/// </summary>
		/// <remarks>
		/// A higher value gives a more correct result,
		/// but the cost is increased rendering time. For cheaper multi bounce GI, use Path Tracer as
		/// the secondary integrator instead of increasing depth.
		/// </remarks>
		public int fgDepth = 1;
		
		// --------------------------
		// Final Gather - Attenuation
		// --------------------------
		
		/// <summary>
		/// There is no attenuation before this distance.
		/// </summary>
		public float fgAttenuationStart = 0;
		/// <summary>
		/// The distance where attenuation fades to zero.
		/// </summary>
		/// <remarks>
		/// To enable attenuation set this value higher than 0.0. The default value is 0.0.
		/// </remarks>
		public float fgAttenuationStop = 0;
		public float fgFalloffExponent = 0;
		
		
		// --------------------------------
		// Final Gather - Ambient Occlusion
		// --------------------------------
		
		/// <summary>
		/// Set to >0 for AO to be calculated.
		/// </summary>
		public float fgAOInfluence = 0;
		public float fgAOMaxDistance = 0.223798f;
		public float fgAOContrast = 1;
		public float fgAOScale = 2.0525f;
		
		
		public bool fgCheckVisibility = true;
		/// <summary>
		/// Sets the number of final gather points to interpolate between. In Unity: "Interpolation Points"
		/// </summary>
		/// <remarks>
		/// A higher value will give a smoother result, but can also smooth out details. If light leakage
		/// is introduced through walls when this value is increased, checking the sample visibility solves that problem.
		/// </remarks>
		public int fgInterpolationPoints = 15;
		public Integrator primaryIntegrator = Integrator.FinalGather;
		/// <summary>
		/// As a post process, converts the color of the primary integrator result from RGB to HSV
		/// and scales the V value. In Unity: "Bounce Intensity"
		/// </summary>
		public float primaryIntensity = 1;
		public float primarySaturation = 1;
		
		// -----------
		// Path Tracer
		// -----------
		
		public Integrator secondaryIntegrator = Integrator.None;
		public float secondaryIntensity = 1;
		public float secondarySaturation = 1;
		// Path Tracer
		// FIXME these aren't included in the default Beast XML file. How do we handle null values when reading the XML?
		/// <summary>
		/// The Path Tracer accuracy.
		/// </summary>
		/// <remarks>
		/// 1.0 means on pt sample per pixel. A higher value gives less noise.
		/// </remarks>
		public float ptAccuracy = 1;
		/// <summary>
		/// World space value. A small value makes the cache more detailed.
		/// </summary>
		public float ptPointSize = 0;
		/// <summary>
		/// Cache direct light as well, boosts speed.
		/// </summary>
		public bool ptCacheDirectLight = true;
		/// <summary>
		/// Check visibility before using values in the cache.
		/// </summary>
		public bool ptCheckVisibility = false;
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


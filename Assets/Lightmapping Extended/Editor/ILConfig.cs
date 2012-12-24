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
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.IO;


[System.Serializable]
public class ILConfig
{
	public enum ShadowDepth
	{
		PrimaryRays = 1,
		PrimaryAndSecondaryRays = 2
	}
	
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
	public AASettings aaSettings = new AASettings ();
	[XmlElement(ElementName = "RenderSettings")]
	public RenderSettings renderSettings = new RenderSettings ();
	[XmlElement(ElementName = "EnvironmentSettings")]
	public EnvironmentSettings environmentSettings = new EnvironmentSettings ();
	[XmlElement(ElementName = "FrameSettings")]
	public FrameSettings frameSettings = new FrameSettings ();
	[XmlElement(ElementName = "GISettings")]
	public GISettings giSettings = new GISettings ();
	[XmlElement(ElementName = "SurfaceTransferSettings")]
	public SurfaceTransferSettings surfaceTransferSettings = new SurfaceTransferSettings ();
	[XmlElement(ElementName = "TextureBakeSettings")]
	public TextureBakeSettings textureBakeSettings = new TextureBakeSettings ();
	
	
	public static ILConfig DeserializeFromPath (string path)
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

	public void SerializeToPath (string path)
	{	
		using (XmlTextWriter writer = new XmlTextWriter (path, System.Text.Encoding.GetEncoding ("ISO-8859-1"))) {
			XmlSerializerNamespaces ns = new XmlSerializerNamespaces ();
			ns.Add (string.Empty, string.Empty);
			writer.Formatting = Formatting.Indented;
			XmlSerializer serializer = new XmlSerializer (typeof(ILConfig));
			serializer.Serialize (writer, this, ns);
		}
	}
	
	public string SerializeToString ()
	{
		StringWriter writer = new StringWriter ();
		XmlSerializer serializer = new XmlSerializer (typeof(ILConfig));
		serializer.Serialize (writer, this);
		return writer.ToString ();
	}
	
	public static ILConfig DeserializeFromString (string configString)
	{
		XmlSerializer serializer = new XmlSerializer (typeof(ILConfig));
		TextReader reader = new StringReader (configString);
		ILConfig config = (ILConfig)serializer.Deserialize (reader);
		return config;
	}
	
	[System.Serializable]
	public class FrameSettings
	{
		/// <summary>
		/// Different ways for Beast to distribute tiles over the image plane.
		/// </summary>
//		public enum TileScheme
//		{
//			/// <summary>
//			/// Render from left to right.
//			/// </summary>
//			LeftToRight,
//			/// <summary>
//			/// A way for Beast to achieve maximum coherence, e.g., the fastest rendering time possible.
//			/// </summary>
//			Hilbert,
//			/// <summary>
//			/// A good way to get an early feel for the whole picture without rendering everything.
//			/// </summary>
//			Random,
//			/// <summary>
//			/// Starts in the middle and renders outward in a spiral.
//			/// </summary>
//			Concentric
//		}
		
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
			/// <summary>
			/// Used for development purposes.
			/// </summary>
			public bool debugPrint = false;
			/// <summary>
			/// Save all log messages to a file named debug.out.
			/// </summary>
			public bool debugFile = false;
		}
	
		[System.Serializable]
		public class OutputCorrection
		{
			/// <summary>
			/// Set the mode of output color correction to None, Gamma or SRGB.
			/// The Beast API assumes this is set to Gamma.
			/// </summary>
			public ColorCorrection colorCorrection = ColorCorrection.None;
			/// <summary>
			/// A float value specifying what gamma the output data should have.
			/// The Beast API assumes this is set to 2.2.
			/// </summary>
			public float gamma = 1;
		}

		/// <summary>
		/// If enabled, Beast will try to auto detect the CPU configuration and use one thread per core.
		/// </summary>
		public bool autoThreads = true;
		/// <summary>
		/// If autoThreads is enabled, this can be used to decrease the number of utilized cores,
		/// e.g. to leave one or two cores free to do other work.
		/// </summary>
		public int autoThreadsSubtract = 0;
		/// <summary>
		/// If autoThreads is disabled, this will set the number of threads beast uses. One per core is a good start.
		/// </summary>
		public int renderThreads = 2;
		/// <summary>
		/// If the output is LDR, and dither is true, the resulting image will be dithered. Default is true.
		/// </summary>
		public bool dither = true;
		/// <summary>
		/// A float value specifying what gamma the input data has. Always set this to 1.0 / 2.2 = 0.454545,
		/// which is the gamma the Beast API assumes is used.
		/// </summary>
		public float inputGamma = 1.0f;
		public OutputCorrection outputCorrection = new OutputCorrection ();
		/// <summary>
		/// Different ways for Beast to distribute tiles over the image plane.
		/// </summary>
//		public TileScheme tileScheme = TileScheme.Hilbert;
		/// <summary>
		/// A smaller tile gives better ray tracing coherence. There is no “best setting” for all scenes.
		/// Default value is 32, giving 32x32 pixel tiles. The largest allowed tile size is 128.
		/// </summary>
//		public int tileSize = 32;
		/// <summary>
		/// If this box is checked the alpha channel value is pre multiplied into the color channel of the pixel.
		/// Note that disabling premultiply alpha gives poor result if used with environment maps and other
		/// non constant camera backgrounds. Disabling premultiply alpha can be convenient when composing
		/// images in post.
		/// </summary>
		public bool premultiply = true;
		/// <summary>
		/// This is the alpha threshold for pixels to be considered opaque enough to be “un multiplied”
		/// when using premultiply alpha.
		/// </summary>
		public float premultiplyThreshold = 0.0f;
		/// <summary>
		/// Different levels of textual output that Beast can produce.
		/// </summary>
		public OutputVerbosity outputVerbosity = new OutputVerbosity ();
	}
	
	
	
	[System.Serializable]
	public class RenderSettings
	{
		/// <summary>
		/// An error threshold to avoid double intersections.
		/// For example, a shadow ray should not intersect the same triangle as the primary ray did,
		/// but because of limited numerical precision this can happen. The bias value moves the
		/// intersection point to eliminate this problem. If set to zero this value is computed
		/// automatically depending on the scene size.
		/// </summary>
		public float bias = 0.005f;
		/// <summary>
		/// Controls the maximum transparency depth for Global Illumination rays.
		/// Used to speed up renderings with a lot of transparency (for example trees).
		/// </summary>
		public int giTransparencyDepth = 2;
		/// <summary>
		/// If true, light-links will be ignored and all available light sources will be used.
		/// </summary>
		public bool ignoreLightLinks = false;
		/// <summary>
		/// The maximum amount of "bounces" a ray can have before being considered done.
		/// A bounce can be a reflection or refraction.
		/// </summary>
		public int maxRayDepth = 6;
		/// <summary>
		/// The maximum number of shadow rays per point that will be used to generate a soft shadow
		/// for any light source. Use this to shorten render times at the price of soft shadow quality.
		/// This will lower the maximum number of rays sent for any light sources that have a shadow
		/// samples setting higher than this value, but will not raise the number if shadow samples
		/// is set to a lower value.
		/// </summary>
		public int maxShadowRays = 10000;
		/// <summary>
		/// The minimum number of shadow rays that will be sent to determine if a point is lit by a
		/// specific light source. Use this value to ensure that you get enough quality in soft shadows
		/// at the price of render times. This will raise the minimum number of rays sent for any light
		/// sources that have a minShadowSamples setting lower than this value, but will not lower the
		/// number if minShadowSamples is set to a higher value. Setting this to a value higher than
		/// maxShadowRays will not send more rays than maxShadowRays.
		/// </summary>
		public int minShadowRays = 0;
		/// <summary>
		/// The maximum amount of reflections a ray can have before being considered done.
		/// </summary>
		public int reflectionDepth = 2;
		/// <summary>
		/// If the intensity of the reflected contribution is less than the threshold, the ray will be terminated.
		/// </summary>
		public float reflectionThreshold = 0.001f;
		/// <summary>
		/// Controls which rays that spawn shadow rays. If set to 1, only primary rays spawn shadow rays.
		/// If set to 2, the first secondary ray spawns a shadow ray as well.
		/// </summary>
		public int shadowDepth = 2;
		/// <summary>
		/// Make objects cast shadows from all light sources, not only the light-linked light sources.
		/// </summary>
		public bool shadowsIgnoreLightLinks = false;
		public int transparencyDepth = 50;
		/// <summary>
		/// Normalize tangent space basis vectors (tangent, binormal and normal) at every intersection point.
		/// </summary>
		public bool tsIntersectionNormalization = true;
		/// <summary>
		/// Orthogonalize tangent space basis vectors (tangent, binormal and normal) at every intersection point.
		/// </summary>
		public bool tsIntersectionOrthogonalization = true;
		/// <summary>
		/// Using this setting will force Beast to mirror tangent and binormal when UV has odd winding direction.
		/// </summary>
		public bool tsOddUVFlipping = true;
		/// <summary>
		/// Normalize tangent space basis vectors (tangent, binormal and normal) at every vertex.
		/// </summary>
		public bool tsVertexNormalization = true;
		/// <summary>
		/// Orthogonalize tangent space basis vectors (tangent, binormal and normal) at every vertex.
		/// </summary>
		public bool tsVertexOrthogonalization = true;
		/// <summary>
		/// Triangle vertices that are closer together than this threshold will be merged into one
		/// (if possible depending on other vertex data).
		/// </summary>
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

		/// <summary>
		/// Controls the minimum number of samples per pixel. Values less than 0 allows using less than one
		/// sample per pixel (if AdaptiveSampling is used). Default value is 0.
		/// </summary>
		public int minSampleRate = 0;
		/// <summary>
		/// Controls the maximum number of samples per pixel. Default value is 0.
		/// </summary>
		public int maxSampleRate = 2;
		/// <summary>
		/// If the contrast differs less than this threshold Beast will consider the sampling good enough.
		/// Default value is 0.1.
		/// </summary>
		public float contrast = 0.1f;
		/// <summary>
		/// Enable this to diagnose the sampling. The brighter a pixel is, the more samples were taken at that position.
		/// </summary>
		public bool diagnose = false;
		/// <summary>
		/// To work efficiently on LDR images, the sampling algorithm can clamp the intensity of the image
		/// samples to the [minValue..maxValue] range. When rendering in HDR this is not desired.
		/// Clamp should then be disabled.
		/// </summary>
		public bool clamp = false;
		public SamplingMode samplingMode = SamplingMode.Adaptive;
		/// <summary>
		/// The sub-pixel filter to use. The following filters are available (default value is Box):
		/// </summary>
		public Filter filter = Filter.Gauss;
		/// <summary>
		/// The width and height of the filter kernel in pixels, given by setting the sub elements x and y (float).
		/// Default value is 1.0 for both x and y.
		/// </summary>
		public LMVec2 filterSize = new LMVec2 (2.2f, 2.2f);
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
		public Environment giEnvironment = Environment.None;
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
		public float giEnvironmentIntensity = 0.2f;
		/// <summary>
		/// The image file to use for IBL, using an absolute path.
		/// </summary>
		/// <remarks>
		/// Accepts hdr or OpenEXR format. The file should be long-lat. Use giEnvironmentIntensity to boost
		/// the intensity of the image.
		/// </remarks>
		public string iblImageFile = "";
		/// <summary>
		/// Controls the appearance of the shadows, banded shadows look more aliased, but noisy shadows
		/// flicker more in animations.
		/// </summary>
		public float iblBandingVsNoise = 1;
		/// <summary>
		/// To remove diffuse lighting from IBL, set this to false. To get the diffuse lighting
		/// Final Gather could be used instead.
		/// </summary>
		public bool iblEmitDiffuse = true;
		/// <summary>
		/// Turns on the expensive IBL implementation. This will generate a number of (iblSamples)
		/// directional lights from the image.
		/// </summary>
		public bool iblEmitLight = false;
		/// <summary>
		/// To remove specular highlights from IBL, set this to false.
		/// </summary>
		public bool iblEmitSpecular = false;
		/// <summary>
		/// Pre-blur the environment image for Global Illumination calculations. Can help to reduce noise and flicker
		/// in images rendered with Final Gather. May increase render time as it is blurred at render time. It is
		/// always cheaper to pre-blur the image itself in an external application before loading it into Beast.
		/// </summary>
		public float iblGIEnvBlur = 0.05f;
		/// <summary>
		/// Sets the intensity of the lighting.
		/// </summary>
		public float iblIntensity = 1;
		/// <summary>
		/// The number of samples to be taken from the image. This will affect how soft the shadows will be,
		/// as well as the general lighting. The higher number of samples, the better the shadows and lighting.
		/// </summary>
		public int iblSamples = 300;
		/// <summary>
		/// Controls whether shadows should be created from IBL when this is used.
		/// </summary>
		public bool iblShadows = true;
		/// <summary>
		/// Further tweak the intensity by boosting the specular component.
		/// </summary>
		public float iblSpecularBoost = 1;
		/// <summary>
		/// Swap the Up Axis. Default value is false, meaning that Y is up.
		/// </summary>
		public bool iblSwapYZ = false;
		/// <summary>
		/// The sphere that the image is projected on can be rotated around the up axis.
		/// The amount of rotation is given in degrees. Default value is 0.0.
		/// </summary>
		public float iblTurnDome = 0;
	}
	
	
	[System.Serializable]
	public class GISettings
	{
		public enum ClampMaterials {
			/// <summary>
			/// No clamping at all
			/// </summary>
			None,
			/// <summary>
			/// clamps each color component (R, G, B) individually.
			/// </summary>
			Component,
			/// <summary>
			/// clamps the intensity of the color to 1. This can be useful to make sure the color of
			/// a surface is preserved when clamping. If using component clamp on a color like (3, 1, 1) will
			/// give the color (1, 1, 1) which means that all color bleeding is lost.
			/// </summary>
			Intensity,
		}
		
		public enum Integrator {
			None = 0,
			FinalGather = 1,
			/// <summary>
			/// Used if many indirect bounces are needed and Final Gather-only solution with acceptable
			/// quality would take to much time to render.
			/// </summary>
			PathTracer = 2,
			MonteCarlo = 3
		}
		
		
		// Global Illumination

		/// <summary>
		/// This setting controls if the materials should be clamped in any way for GI purposes.
		/// Typically you can use this to avoid problems with non physical materials making your scene
		/// extremely bright. This affects both the specular and diffuse components of materials.
		/// </summary>
		public ClampMaterials clampMaterials = ClampMaterials.None;
		/// <summary>
		/// This setting can be used to exaggerate light bouncing in dark scenes. Setting it to a value larger
		/// than 1 will push the diffuse color of materials towards 1 for GI computations. The typical use case
		/// is scenes authored with dark materials, this happens easily when doing only direct lighting since it’s
		/// easy to compensate dark materials with strong light sources. Indirect light will be very subtle in
		/// these scenes since the bounced light will fade out quickly. Setting a diffuse boost will compensate
		/// for this. Note that values between 0 and 1 will decrease the diffuse setting in a similar way making
		/// light bounce less than the materials says, values below 0 is invalid. The actual computation taking
		/// place is a per component pow(colorComponent, (1.0 / diffuseBoost)).
		/// </summary>
		public float diffuseBoost = 1;
		/// <summary>
		/// This setting globally scales all materials emissive components by the specified value when
		/// they are used by the GI.
		/// </summary>
		public float emissiveScale = 1;
		public bool enableCaustics = false;
		public bool enableGI = true;
		public bool ignoreNonDiffuse = true;
		/// <summary>
		/// Sets the sample rate used during the GI precalculation pass. The prepass is progressive,
		/// rendering from a low resolution up to a high resolution in multiple passes. The prepassMinSampleRate
		/// sets the initial resolution, where a negative value means a lower resolution than the original
		/// image (in powers of two), e.g. -4 gives the original resolution divided by 16. The
		/// prepassMaxSampleRate sets the resolution of the final prepass, e.g 0 giving the same resolution
		/// as the original resolution. The default values are -4/0.
		/// </summary>
		public int prepassMaxSampleRate = 0;
		/// <summary>
		/// Sets the sample rate used during the GI precalculation pass. The prepass is progressive,
		/// rendering from a low resolution up to a high resolution in multiple passes. The prepassMinSampleRate
		/// sets the initial resolution, where a negative value means a lower resolution than the original
		/// image (in powers of two), e.g. -4 gives the original resolution divided by 16. The
		/// prepassMaxSampleRate sets the resolution of the final prepass, e.g 0 giving the same resolution
		/// as the original resolution. The default values are -4/0.
		/// </summary>
		public int prepassMinSampleRate = -4;
		/// <summary>
		/// The Global Illumination system allows you to use two separate algorithms to calculate indirect lighting.
		/// You can for instance calculate multiple levels of light bounces with a fast algorithm like the
		/// Path Tracer, and still calculate the final bounce with Final Gather to get a fast high-quality
		/// global illumination render. Both subsystems have individual control of Intensity and Saturation
		/// to boost the effects if necessary.
		/// </summary>
		public Integrator primaryIntegrator = Integrator.FinalGather;
		/// <summary>
		/// Tweak the amount of illumination from the primary and secondary GI integrators. This lets you boost
		/// or reduce the amount of indirect light easily.
		/// </summary>
		public float primaryIntensity = 1;
		/// <summary>
		/// Lets you tweak the amount of color in the primary and secondary GI integrators. This lets you boost
		/// or reduce the perceived saturation of the bounced light.
		/// </summary>
		public float primarySaturation = 1;
		/// <summary>
		/// The Global Illumination system allows you to use two separate algorithms to calculate indirect lighting.
		/// You can for instance calculate multiple levels of light bounces with a fast algorithm like the
		/// Path Tracer, and still calculate the final bounce with Final Gather to get a fast high-quality
		/// global illumination render. Both subsystems have individual control of Intensity and Saturation
		/// to boost the effects if necessary.
		/// </summary>
		public Integrator secondaryIntegrator = Integrator.None;
		/// <summary>
		/// Lets you tweak the amount of color in the primary and secondary GI integrators. This lets you boost
		/// or reduce the perceived saturation of the bounced light.
		/// </summary>
		public float secondaryIntensity = 1;
		/// <summary>
		/// Lets you tweak the amount of color in the primary and secondary GI integrators. This lets you boost
		/// or reduce the perceived saturation of the bounced light.
		/// </summary>
		public float secondarySaturation = 1;
		/// <summary>
		/// This setting can be used to exaggerate or decrease specular light effects. All materials specular
		/// color is multiplied by this factor when they are used by the GI.
		/// </summary>
		public float specularScale = 1;
		
		
		// Final Gather
		
		public enum Cache {
			/// <summary>
			/// (Brute Force) disables caching and performs a final gathering for every shading point
			/// (same as Monte Carlo).
			/// </summary>
			Off,
			/// <summary>
			/// caches the irradiance at selected points in the scene and uses interpolation in between the points.
			/// This is the default method.
			/// </summary>
			Irradiance,
			/// <summary>
			/// caches radiance SH functions at selected points in the scene and uses interpolation in
			/// between the points. The radiance cache is very useful in some advanced baking passes
			/// (e.g. Radiosity Normal Maps), where directional indirect lighting is needed.
			/// </summary>
			RadianceSH
		}

		/// <summary>
		/// Can be used to adjust the contrast for ambient occlusion.
		/// </summary>
		public float fgAOContrast = 1;
		/// <summary>
		/// Controls a scaling of Final Gather with Ambient Occlusion which can be used to boost shadowing and get
		/// more contrast in you lighting. The value controls how much Ambient Occlusion to blend into the
		/// Final Gather solution.
		/// </summary>
		public float fgAOInfluence = 0;
		/// <summary>
		/// Max distance for the occlusion. Beyond this distance a ray is considered to be visible.
		/// Can be used to avoid full occlusion for closed scenes.
		/// </summary>
		public float fgAOMaxDistance = 0.3f;
		/// <summary>
		/// A scaling of the occlusion values. Can be used to increase or decrease the shadowing effect.
		/// </summary>
		public float fgAOScale = 2;
		/// <summary>
		/// Visualize just the ambient occlusion values. Useful when tweaking the occlusion sampling options.
		/// </summary>
		public bool fgAOVisualize = false;
		/// <summary>
		/// The distance where attenuation is started. There is no attenuation before this distance.
		/// Note that fgAttenuationStop must be set higher than 0.0 to enable attenuation.
		/// </summary>
		public float fgAttenuationStart = 0;
		/// <summary>
		/// Sets the distance where attenuation is stopped (fades to zero). There is zero intensity beyond this
		/// distance. To enable attenuation set this value higher than 0.0. The default value is 0.0.
		/// </summary>
		public float fgAttenuationStop = 0;
		/// <summary>
		/// When this is enabled final gather will also cache lighting from light sources. This increases performance
		/// since fewer direct light calculations are needed. It gives an approximate result, and hence can affect
		/// the quality of the lighting. For instance indirect light bounces from specular highlights might be lost.
		/// However this caching is only done for depths higher than 1, so the quality of direct light and shadows
		/// in the light map will not be reduced.
		/// </summary>
		public bool fgCacheDirectLight = false;
		/// <summary>
		/// Turn this on to reduce light leakage through walls. When points are collected to interpolate between,
		/// some of them can be located on the other side of geometry.
		/// As a result light will bleed through the geometry. So to prevent this Beast can reject points
		/// that are not visible.
		/// </summary>
		public bool fgCheckVisibility = true;
		public float fgCheckVisibilityDepth = 1;
		/// <summary>
		/// Turn this on to clamp the sampled values to [0, 1]. This will reduce high frequency noise when
		/// Final Gather is used together with other Global Illumination algorithms.
		/// </summary>
		public bool fgClampRadiance = false;
		/// <summary>
		/// Controls how sensitive the final gather should be for contrast differences between the points
		/// during pre calculation. If the contrast difference is above this threshold for neighbouring points,
		/// more points will be created in that area. This tells the algorithm to place points where they are really
		/// needed, e.g. at shadow boundaries or in areas where the indirect light changes quickly. Hence this
		/// threshold controls the number of points created in the scene adaptively. Note that if a low number of
		/// final gather rays are used, the points will have high variance and hence a high contrast difference,
		/// so in that case you might need to increase the contrast threshold to prevent points from clumping together.
		/// </summary>
		public float fgContrastThreshold = 0.05f;
		/// <summary>
		/// Sets the number of indirect light bounces calculated by final gather. A value higher than 1 will produce
		/// more global illumination effects, but note that it can be quite slow since the number of rays will increase
		/// exponentially with the depth. It’s often better to use a fast method for secondary GI. If a secondary GI is
		/// used the number of set final gather bounces will be calculated first, before the secondary GI is called.
		/// So in most cases the depth should be set to 1 if a secondary GI is used.
		/// </summary>
		public int fgDepth = 1;
		/// <summary>
		/// Sets the minimum number of points that should be used when estimating final gather in the pre calculation
		/// pass. The impact is that a higher value will create more points all over the scene. The default value 15
		/// rarely needs to be adjusted.
		/// </summary>
		public int fgEstimatePoints = 15;
		public bool fgExploitFrameCoherence = false;
		/// <summary>
		/// This can be used to adjust the rate by which lighting falls off by distance. A higher exponent gives a
		/// faster falloff. Note that fgAttenuationStop must be set higher than 0.0 to enable attenuation.
		/// </summary>
		public float fgFalloffExponent = 0;
		/// <summary>
		/// Controls how the irradiance gradient is used in the interpolation. Each point stores it’s irradiance
		/// gradient which can be used to improve the interpolation. However in some situations using the gradient
		/// can result in white ”halos” and other artifacts. This threshold can be used to reduce those artifacts.
		/// </summary>
		public float fgGradientThreshold = 0.5f;
		/// <summary>
		/// Sets the number of final gather points to interpolate between. A higher value will give a smoother result,
		/// but can also smooth out details. If light leakage is introduced through walls when this value is increased,
		/// checking the sample visibility solves that problem, see fgCheckVisibility below.
		/// </summary>
		public int fgInterpolationPoints = 15;
		/// <summary>
		/// Controls how far away from walls the final gather will be called again, instead of the secondary GI.
		/// If 0.0 is used a value will be calculated by Beast depending on the secondary GI used. The calculated
		/// value is printed in the output window. If you still get leakage you can adjust this by manually typing
		/// in a higher value.
		/// </summary>
		public float fgLightLeakRadius = 0;
		/// <summary>
		/// This setting can be used to reduce light leakage through walls when using final gather as primary GI and
		/// path tracing as secondary GI. Leakage, which can happen when e.g. the path tracer filters in values on the
		/// other side of a wall, is reduced by using final gather as a secondary GI fallback when sampling close to
		/// walls or corners. When this is enabled a final gather depth of 3 will be used automatically, but the higher
		/// depths will only be used close to walls or corners. Note that this is only used when path tracing is set
		/// as secondary GI.
		/// </summary>
		public bool fgLightLeakReduction = false;
		/// <summary>
		/// The max distance a ray can be traced before it’s considered to be a “miss”.
		/// This can improve performance in very large scenes. If the value is set to 0.0 the entire scene will be used.
		/// </summary>
		public float fgMaxRayLength = 0;
		/// <summary>
		/// Controls how sensitive the final gather should be for differences in the points normals.
		/// A lower value will give more points in areas of high curvature.
		/// </summary>
		public float fgNormalThreshold = 0.2f;
		/// <summary>
		/// Turn this on to visualize the final gather prepass. Using the Preview Calculation Pass enables a quick
		/// preview of the final image lighting, reducing lighting setup time.
		/// </summary>
		public bool fgPreview = false;
		/// <summary>
		/// Sets the maximum number of rays to use for each Final Gather sample point.
		/// A higher number gives higher quality, but longer rendering time.
		/// </summary>
		public int fgRays = 1000;
		/// <summary>
		/// Selects what caching method to use for final gathering.
		/// </summary>
		public Cache fgUseCache = Cache.Irradiance;
		
		
		// PathTracer

		/// <summary>
		/// Selects the filter to use when querying the cache during rendering. None will return the closest
		/// cache point (unfiltered). The filter type can be set to None, Box, Gauss or Triangle.
		/// </summary>
		public enum PTFilterType {
			None,
			Box,
			Gauss,
			Triangle
		}

		/// <summary>
		/// Sets the number of paths that are traced for each sample element (pixel, texel or vertex).
		/// For preview renderings, you can use a low value like 0.5 or 0.1, which means 
		/// that half of the pixels or 1/10 of the pixels will generate a path. For production renderings
		/// you can use values above 1.0, if needed to get good quality.
		/// </summary>
		public float ptAccuracy = 1;
		/// <summary>
		/// When this is enabled the path tracer will also cache lighting from light sources. This increases
		/// performance since fewer direct light calculations are needed. It gives an approximate result, and
		/// hence can affect the quality of the lighting. For instance indirect light bounces from specular
		/// highlights might be lost.
		/// </summary>
		public bool ptCacheDirectLight = false;
		/// <summary>
		/// Turn this on to reduce light leakage through walls. When points are collected to interpolate between,
		/// some of them can be located on the other side of geometry. As a result light will bleed through the
		/// geometry. So to prevent this Beast can reject points that are not visible.
		/// </summary>
		public bool ptCheckVisibility = true;
		public float ptConservativeEnergyLimit = 0.95f;
		public LMColor ptDefaultColor = new LMColor (0, 0, 0, 1);
		public int ptDepth = 5;
		public bool ptDiffuseIllum = true;
		public string ptFile = "";
		/// <summary>
		/// Sets the size of the filter as a multiplier of the Cache Point Spacing value. For example;
		/// a value of 3.0 will use a filter that is three times larges then the cache point spacing.
		/// If this value is below 1.0 there is no guarantee that any cache point is found. If no cache
		/// point is found the Default Color will be returned instead for that query.
		/// </summary>
		public float ptFilterSize = 3;
		/// <summary>
		/// Selects the filter to use when querying the cache during rendering. None will return the closest
		/// cache point (unfiltered). The filter type can be set to None, Box, Gauss or Triangle.
		/// </summary>
		public PTFilterType ptFilterType = PTFilterType.Gauss;
		/// <summary>
		/// Sets the amount of normal deviation that is allowed during cache point filtering. ptFilterType
		/// Selects the filter to use when querying the cache during rendering. None will return the closest
		/// cache point (unfiltered). The filter type can be set to None, Box, Gauss or Triangle.
		/// </summary>
		public float ptNormalThreshold = 0.7f;
		/// <summary>
		/// Sets the maximum distance between the points in the path tracer cache. If set to 0 a value will be
		/// calculated automatically based on the size of the scene. The automatic value will be printed out
		/// during rendering, which is a good starting value if the point spacing needs to be adjusted.
		/// </summary>
		public float ptPointSize = 0;
		/// <summary>
		/// If enabled the cache points will be pre-filtered before the final pass starts. This increases the
		/// performance using the final render pass.
		/// </summary>
		public bool ptPrecalcIrradiance = true;
		/// <summary>
		/// If enabled the pre-render pass will be visible in the render view.
		/// </summary>
		public bool ptPreview = false;
		public bool ptSpecularIllum = true;
		public bool ptTransmissiveIllum = true;
		
		
		// Monte Carlo

		/// <summary>
		/// Sets the number of indirect light bounces calculated by monte carlo.
		/// </summary>
		public int mcDepth = 2;
		/// <summary>
		/// The max distance a ray can be traced before it’s considered to be a “miss”. This can improve
		/// performance in very large scenes. If the value is set to 0.0 the entire scene will be used.
		/// </summary>
    	public float mcMaxRayLength = 0;
		/// <summary>
		/// Sets the number of rays to use for each calculation. A higher number gives higher quality,
		/// but longer rendering time.
		/// </summary>
    	public int mcRays = 16;
	}
	
	
	[System.Serializable]
	public class ADSSettings
	{
		public enum Instancing {
			/// <summary>
			/// Never use instancing while rendering.
			/// </summary>
			Never,
			/// <summary>
			/// Use instancing whenever a shape is used more than once (default).
			/// </summary>
			AutoDetect
		}
		
		public int gridDensity = 1;
		/// <summary>
		/// Decides how deep the Acceleration Data Structure can subdivide.
		/// </summary>
	    public int gridMaxDepth = 4;
		/// <summary>
		/// Decides how many triangles that can reside in a leaf before it is split up. The Recursion Depth
		/// has precedence over the threshold. A leaf at max depth will never be split. The Recursion Depth
		/// and Recursion Threshold are advanced settings that shouldn't be altered unless Acceleration Data
		/// Structures are second nature to you.
		/// </summary>
	    public int gridThreshold = 25;
		/// <summary>
		/// Specifies the minimum number of triangles that an instance is allowed to have in order
		/// for geometry instancing to be used.
		/// </summary>
	    public int instancingThreshold = 500;
		/// <summary>
		/// This tag lets the user control if instances should be used or not during rendering.
		/// Instancing allows the user to place a single shape in several different places, each with an
		/// individual transform. This preserves disk space and will lower memory consumption when rendering,
		/// but might increase render times quite a bit if there are many large instances.
		/// </summary>
	    public Instancing useInstancing = Instancing.AutoDetect;
		/// <summary>
		/// Beast uses specialized vector instructions to speed up the rendering. The cost is higher memory usage.
		/// Turn off SSE if Beast starts swapping.
		/// </summary>
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
		public int edgeDilation = 3;
	}
}


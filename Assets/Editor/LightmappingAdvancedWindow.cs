using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

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


#region Beast Settings

[System.Serializable]
[XmlRoot (Namespace = null)]
public class ILConfig
{
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
	
	
	[System.Serializable]
	public class AASettings
	{
		public string samplingMode = "Adaptive";
		public bool clamp = false;
		public float contrast = 0.1f;
		public bool diagnose = false;
		public int minSampleRate = 0;
		public int maxSampleRate = 2;
		public string filter = "Gauss1234";
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
	public class EnvironmentSettings
	{
		public string giEnvironment = "SkyLight";
		public LMColor skyLightColor = new LMColor (0.86f, 0.93f, 1, 1);
		public float giEnvironmentIntensity = 0;
	}
	
	[System.Serializable]
	public class FrameSettings
	{
		public float inputGamma = 1;
	}
	
	[System.Serializable]
	public class GISettings
	{
		public bool enableGI = true;
		public bool fgPreview = false;
		public int fgRays = 1000;
		public float fgContrastThreshold = 0.05f;
		public float fgGradientThreshold = 0;
		public bool fgCheckVisibility = true;
		public int fgInterpolationPoints = 15;
		public float fgDepth = 1;
		public string primaryIntegrator = "FinalGather";
		public float primaryIntensity = 1;
		public float primarySaturation = 1;
		public string secondaryIntegrator = "None";
		public float secondaryIntensity = 1;
		public float secondarySaturation = 1;
		public float fgAOInfluence = 0;
		public float fgAOMaxDistance = 0.223798f;
		public float fgAOContrast = 1;
		public float fgAOScale = 2.0525f;
	}
	
	[System.Serializable]
	public class SurfaceTransferSettings
	{
		public float frontRange = 0;
		public float frontBias = 0;
		public float backRange = 2;
		public float backBias = -1;
		public string selectionMode = "Normal";
	}
	
	[System.Serializable]
	public class TextureBakeSettings
	{
		public LMColor bgColor = new LMColor (1, 1, 1, 1);
		public bool bilinearFilter = true;
		public bool conservativeRasterization = true;
		public float edgeDilation = 3;
	}
}

#endregion


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
	
	private ILConfig LoadConfigFile (string path)
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

	private void SaveConfigFile (string path, ILConfig data)
	{	
		using (XmlTextWriter writer = new XmlTextWriter (path, System.Text.Encoding.GetEncoding ("ISO-8859-1"))) {
			XmlSerializerNamespaces ns = new XmlSerializerNamespaces ();
			ns.Add (string.Empty, string.Empty);
			writer.Formatting = Formatting.Indented;
			XmlSerializer serializer = new XmlSerializer (typeof(ILConfig));
			serializer.Serialize (writer, data, ns);
		}
	}
	
	
	void OnGUI ()
	{
		if (GUILayout.Button ("Save File")) {
			if (config != null) {
				Debug.Log ("save " + ConfigFilePath);
				string path = ConfigFilePath;
				SaveConfigFile (path, config);
			}
		}
		
		if (GUILayout.Button ("Load File")) {
			string path = ConfigFilePath;
			if (File.Exists (path)) {
				Debug.Log ("loading " + path);
				config = LoadConfigFile (path);
			} else {
				Debug.Log ("Config file doesn't exist");
			}
		}
		
		
		if (config != null) {
			GUILayout.Label (config.aaSettings.filter);
		}
		
	}
}

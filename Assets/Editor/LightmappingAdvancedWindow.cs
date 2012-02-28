using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
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

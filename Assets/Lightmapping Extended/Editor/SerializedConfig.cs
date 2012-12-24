using UnityEngine;
using System.Collections;

// Workaround for incompatibility between Unity's serialization
// of ScriptableObjects and XML serialization of ILConfig
public class SerializedConfig : ScriptableObject
{
	public ILConfig config;
}

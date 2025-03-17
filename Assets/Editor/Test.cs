using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class Test : Editor
{
	[MenuItem("Testage/Test")]
	public static void Testage()
	{
		foreach (Material material in from guid in AssetDatabase.FindAssets("t:Material") select AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)))
		{
			if (shaders.Contains(material.shader.name))
			{
				material.shader = Shader.Find(material.shader.name == "iPhone/LightMap" || material.shader.name == "iPhone/CartoonRenderingLightmap" ? "iPhone/CartoonRenderingLightmap"  : "iPhone/CartoonRendering");

				material.SetFloat("_Outline", material.shader.name == "iPhone/LightMap" || material.shader.name == "iPhone/CartoonRenderingLightmap" ? 0.1f : 0.05f);
				material.SetColor("_Color", Color.white);
			}
		}

		AssetDatabase.Refresh();
	}

	public static string[] shaders
	{
		get
		{
			return new string[]
			{
				"iPhone/SolidTexture",
				"iPhone/CartoonRendering",
				"iPhone/CartoonRenderingLightmap",
				"iPhone/LightMap"
			};
		}
	}
}

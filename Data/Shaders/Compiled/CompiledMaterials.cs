
using System;
using System.Collections.Generic;
using Reign.Core;
using Reign.Video;
using Reign.Video.API;

namespace ShaderMaterials.Shaders
{
	public static class Materials
	{
		public static List<Type> Types {get; private set;}

		public static void Init(VideoTypes apiType, DisposableI parent, string contentPath, string tag, ShaderVersions shaderVersion)
		{
			// init shaders
			Types = new List<Type>();
			Types.Add(typeof(DiffuseTextureMaterial)); DiffuseTextureMaterial.Init(apiType, parent, contentPath, tag, shaderVersion);
		}
	}
}

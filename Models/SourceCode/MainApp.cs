﻿using System;
using System.Collections.Generic;
using Reign.Core;
using Reign.Video;
using Reign.Video.API;
using ShaderMaterials.Shaders;

namespace Demo
{
	#if WIN32
	public class MainApp : WinFormApplication
	#elif WINRT
	public class MainApp : CoreWindowApplication
	#elif WP8
	public class MainApp : XAMLApplication
	#elif SILVERLIGHT
	public class MainApp : SilverlightApplication
	#elif XNA
	public class MainApp : XNAApplication
	#elif VITA
	public class MainApp : VitaApplication
	#elif ANDROID
	[Android.App.Activity (MainLauncher = true)]
	public class MainApp : WinFormApplication
	#endif
	{
		bool loaded;
		RootDisposable root;
		VideoI video;
		ViewPortI viewPort;
		Camera camera;
		Model model;
		Vector3 modelOffset;

		RasterizerStateI rasterizerState;
		DepthStencilStateI depthStencilState;
		BlendStateI blendState;
		SamplerStateI samplerState;

		public MainApp()
		{
			var desc = new ApplicationDesc()
			{
				Type = ApplicationTypes.FrameSizable
			};
			Init(desc);
		}
		
		public override void Shown()
		{
			try
			{
				root = new RootDisposable();
				VideoTypes videoType;
				video = Video.Init(VideoTypes.D3D11 | VideoTypes.D3D9 | VideoTypes.OpenGL | VideoTypes.XNA | VideoTypes.Vita, out videoType, root, this, true);
				
				DiffuseTextureMaterial.Init(video, "Data/", video.FileTag, ShaderVersions.Max, null);
				DiffuseTextureMaterial.ApplyInstanceConstantsCallback = applyInstanceData;
				
				var materialTypes = new Dictionary<string,Type>();
				materialTypes.Add("Material", typeof(DiffuseTextureMaterial));
				materialTypes.Add("Material.001", typeof(DiffuseTextureMaterial));
				var materialFieldTypes = new List<MaterialFieldBinder>();
				materialFieldTypes.Add(new MaterialFieldBinder("Material", "Roxy_dds", "Diffuse"));
				materialFieldTypes.Add(new MaterialFieldBinder("Material.001", "Wolf_dds", "Diffuse"));
				var extOverrides = new Dictionary<string,string>();
				#if SILVERLIGHT || VITA || (LINUX && ARM)
				extOverrides.Add(".dds", ".png");
				#endif
				#if iOS
				extOverrides.Add(".dds", ".pvr");
				#endif
				#if ANDROID
				if (((Reign.Video.OpenGL.Video)video).Caps.TextureCompression_ATC) extOverrides.Add(".dds", ".atc");
				else if (((Reign.Video.OpenGL.Video)video).Caps.TextureCompression_PVR) extOverrides.Add(".dds", ".pvr");
				#endif
				var emptyBinders = new List<MaterialFieldBinder>();
				model = new Model(video, "Data/boxes.rm", "Data/", materialTypes, emptyBinders, emptyBinders, emptyBinders, emptyBinders, materialFieldTypes, extOverrides, 0, null);
				
				var frame = FrameSize;
				viewPort = ViewPortAPI.New(video, 0, 0, frame.Width, frame.Height);
				camera = new Camera(viewPort, new Vector3(5, 5, 5), new Vector3(), new Vector3(5, 5+1, 5), 1, 50, MathUtilities.DegToRad(45));
				
				rasterizerState = RasterizerStateAPI.New(video, RasterizerStateDescAPI.New(RasterizerStateTypes.Solid_CullCW));
				depthStencilState = DepthStencilStateAPI.New(video, DepthStencilStateDescAPI.New(DepthStencilStateTypes.ReadWrite_Less));
				blendState = BlendStateAPI.New(video, BlendStateDescAPI.New(BlendStateTypes.None));
				samplerState = SamplerStateAPI.New(video, SamplerStateDescAPI.New(SamplerStateTypes.Linear_Wrap));

				loaded = true;
			}
			catch (Exception e)
			{
				dispose();
				Message.Show("Error", e.Message);
			}
		}

		private void dispose()
		{
			loaded = false;
			if (root != null)
			{
				root.Dispose();
				root = null;
			}
		}

		public override void Closing()
		{
			dispose();
		}

		private void applyInstanceData(DiffuseTextureMaterial material, Mesh mesh)
		{
			material.Transform = Matrix4.FromAffineTransform(Matrix3.FromEuler(mesh.Rotation), mesh.Scale, mesh.Position + modelOffset);
		}

		public override void Update(Time time)
		{
			if (!loaded) return;
			
			camera.RotateAroundLookLocationWorld(0, 1 * time.Delta, 0);

			#if XNA && !SILVERLIGHT
			if (Microsoft.Xna.Framework.Input.GamePad.GetState(Microsoft.Xna.Framework.PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed) Exit();
			#endif
		}

		public override void Render(Time time)
		{
			if (!loaded) return;
			
			video.Update();
			var e = Loader.UpdateLoad();
			if (e != null)
			{
				Message.Show("File loading Error", e.Message);
				dispose();
				return;
			}
			if (Loader.ItemsRemainingToLoad != 0) return;
			
			video.EnableRenderTarget();
			video.ClearColorDepth(0, .3f, .3f, 1);
			rasterizerState.Enable();
			depthStencilState.Enable();
			blendState.Enable();
			samplerState.Enable(0);

			viewPort.Size = video.BackBufferSize;
			viewPort.Apply();
			camera.Apply();

			DiffuseTextureMaterial.Camera = camera.TransformMatrix;
			DiffuseTextureMaterial.LightDirection = -camera.Position.Normalize();
			DiffuseTextureMaterial.LightColor = new Vector4(1);
			model.Render();

			video.Present();
		}
	}
}

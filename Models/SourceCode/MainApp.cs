﻿using System;
using System.Collections.Generic;
using Reign.Core;
using Reign.Video;
using Reign.Video.API;
using ShaderMaterials.Shaders;

namespace Demo_Windows
{
	#if WINDOWS
	class MainApp : Window
	#else
	class MainApp : Application
	#endif
	{
		bool loaded;
		RootDisposable root;
		VideoI video;
		ViewPortI viewPort;
		Camera camera;
		ModelI model;

		RasterizerStateI rasterizerState;
		DepthStencilStateI depthStencilState;
		SamplerStateI samplerState;

		public MainApp()
		#if WINDOWS
		: base("Models", 512, 512, WindowStartPositions.CenterCurrentScreen, WindowTypes.Frame)
		#elif METRO
		: base(ApplicationOrientations.Landscape)
		#elif XNA
		: base(512, 512)
		#endif
		{
			
		}

		protected override void shown()
		{
			try
			{
				root = new RootDisposable();
				VideoTypes videoType;
				#if WINDOWS
				video = Video.Create(VideoTypes.D3D11 | VideoTypes.D3D9 | VideoTypes.OpenGL, out videoType, root, this, true);
				#elif METRO
				video = Video.Create(VideoTypes.D3D11, out videoType, root, this, true);
				#elif XNA
				video = Video.Create(VideoTypes.XNA, out videoType, root, this);
				#endif
				DiffuseTextureMaterial.Init(videoType, video, "Data\\", video.FileTag, ShaderVersions.Max);
				DiffuseTextureMaterial.ApplyInstanceConstantsCallback = applyInstanceData;

				var softwareModel = new SoftwareModel("Data\\box.dae");
				var materialTypes = new Dictionary<string,Type>();
				materialTypes.Add("Material", typeof(DiffuseTextureMaterial));
				materialTypes.Add("Material.001", typeof(DiffuseTextureMaterial));
				var materialFieldTypes = new List<MaterialFieldBinder>();
				materialFieldTypes.Add(new MaterialFieldBinder("Material", "Roxy_dds", "Diffuse"));
				materialFieldTypes.Add(new MaterialFieldBinder("Material.001", "Wolf_dds", "Diffuse"));
				model = Model.Create(videoType, video, softwareModel, MeshVertexSizes.Float3, video, "Data\\", materialTypes, materialFieldTypes);

				var frame = FrameSize;
				viewPort = ViewPort.Create(videoType, video, 0, 0, frame.Width, frame.Height);
				camera = new Camera(viewPort, new Vector3(5, 5, 5), new Vector3(), new Vector3(5, 5+1, 5), 1, 50, Reign.Core.Math.DegToRad(45));

				rasterizerState = RasterizerState.Create(videoType, video, RasterizerStateDesc.Create(videoType, RasterizerStateTypes.Solid_CullCW));
				depthStencilState = DepthStencilState.Create(videoType, video, DepthStencilStateDesc.Create(videoType, DepthStencilStateTypes.ReadWrite_Less));
				samplerState = SamplerState.Create(videoType, video, SamplerStateDesc.Create(videoType, SamplerStateTypes.Linear_Wrap));

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

		protected override void closing()
		{
			dispose();
		}

		private void applyInstanceData(DiffuseTextureMaterial material, MeshI mesh)
		{
			material.Transform = new Matrix4(Matrix3.FromEuler(mesh.Rotation), mesh.Scale, mesh.Location);
		}

		protected override void render()
		{
			if (!loaded) return;

			var e = Streams.TryLoad();
			if (e != null)
			{
				Message.Show("File loading Error", e.Message);
				dispose();
				return;
			}
			if (Streams.ItemsRemainingToLoad != 0) return;

			video.Update();
			video.EnableRenderTarget();
			video.Clear(0, .3f, .3f, 1);
			rasterizerState.Enable();
			depthStencilState.Enable();
			samplerState.Enable(0);

			viewPort.Apply();
			camera.RotateAroundLookLocationWorld(0, .01f, 0);
			camera.Apply();

			DiffuseTextureMaterial.Camera = camera.TransformMatrix;
			DiffuseTextureMaterial.LightDirection = -camera.Location.Normalize();
			model.Render();

			#if !XNA
			video.Present();
			#endif
		}
	}
}
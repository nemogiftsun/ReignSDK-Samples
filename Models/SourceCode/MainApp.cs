using System;
using System.Collections.Generic;
using Reign.Core;
using Reign.Video;
using Reign.Video.API;
using ShaderMaterials.Shaders;

namespace Demo_Windows
{
	#if WINDOWS || OSX || LINUX || NaCl
	class MainApp : Window
	#else
	#if ANDROID
	[Android.App.Activity (MainLauncher = true)]
	#endif
	public class MainApp : Application
	#endif
	{
		bool loaded;
		RootDisposable root;
		VideoI video;
		ViewPortI viewPort;
		Camera camera;
		Model model, model2;
		Vector3 modelOffset;

		RasterizerStateI rasterizerState;
		DepthStencilStateI depthStencilState;
		BlendStateI blendState;
		SamplerStateI samplerState;

		public MainApp()
		#if WINDOWS || OSX || LINUX
		: base("Models", 512, 512, WindowStartPositions.CenterCurrentScreen, WindowTypes.Frame)
		#elif METRO
		: base(ApplicationOrientations.Landscape)
		#elif XNA && !XBOX360
		: base(512, 512)
		#elif XBOX360
		: base(0, 0)
		#elif iOS
		: base(ApplicationOrientations.Landscape, false)
		#elif ANDROID
		: base(ApplicationOrientations.Landscape, 0, false, null)
		#elif NaCl
		: base("Models", 512, 512)
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
				video = Video.Init(VideoTypes.D3D11, out videoType, root, this, true);//VideoTypes.D3D11 | VideoTypes.D3D9 | VideoTypes.OpenGL
				#elif METRO
				video = Video.Init(VideoTypes.D3D11, out videoType, root, this, true);
				#elif XNA
				video = Video.Init(VideoTypes.XNA, out videoType, root, this, true);
				#elif OSX || LINUX || iOS || ANDROID || NaCl
				video = Video.Init(VideoTypes.OpenGL, out videoType, root, this, true);
				#endif
				
				DiffuseTextureMaterial.Init(video, "Data/", video.FileTag, ShaderVersions.Max, null, null);
				DiffuseTextureMaterial.ApplyInstanceConstantsCallback = applyInstanceData;
				
				//var softwareModel = new SoftwareModel("Data/boxes.dae", null, null);
				var materialTypes = new Dictionary<string,Type>();
				materialTypes.Add("Material", typeof(DiffuseTextureMaterial));
				materialTypes.Add("Material.001", typeof(DiffuseTextureMaterial));
				var materialFieldTypes = new List<MaterialFieldBinder>();
				materialFieldTypes.Add(new MaterialFieldBinder("Material", "Roxy_dds", "Diffuse"));
				materialFieldTypes.Add(new MaterialFieldBinder("Material.001", "Wolf_dds", "Diffuse"));
				var extOverrides = new Dictionary<string,string>();
				#if iOS
				extOverrides.Add(".dds", ".pvr");
				#endif
				#if ANDROID
				if (((Reign.Video.OpenGL.Video)video).Caps.TextureCompression_ATC) extOverrides.Add(".dds", ".atc");
				else if (((Reign.Video.OpenGL.Video)video).Caps.TextureCompression_PVR) extOverrides.Add(".dds", ".pvr");
				#endif
				var emptyBinders = new List<MaterialFieldBinder>();
				//model = new Model(video, softwareModel, MeshVertexSizes.Float3, false, true, true, "Data/", materialTypes, emptyBinders, emptyBinders, emptyBinders, emptyBinders, materialFieldTypes, extOverrides, 0, null, null);
				model2 = new Model(video, "Data/boxes.rm", "Data/", materialTypes, emptyBinders, emptyBinders, emptyBinders, emptyBinders, materialFieldTypes, extOverrides, 0, null, null);

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

		protected override void closing()
		{
			dispose();
		}

		private void applyInstanceData(DiffuseTextureMaterial material, Mesh mesh)
		{
			material.Transform = Matrix4.FromAffineTransform(Matrix3.FromEuler(mesh.Rotation), mesh.Scale, mesh.Position + modelOffset);
		}

		protected override void update(Time time)
		{
			if (!loaded) return;
			
			camera.RotateAroundLookLocationWorld(0, 1 * time.Delta, 0);

			#if XNA
			if (Microsoft.Xna.Framework.Input.GamePad.GetState(Microsoft.Xna.Framework.PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed) Exit();
			#endif
		}

		protected override void render(Time time)
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

			viewPort.Size = FrameSize;
			viewPort.Apply();
			camera.Apply();

			DiffuseTextureMaterial.Camera = camera.TransformMatrix;
			DiffuseTextureMaterial.LightDirection = -camera.Position.Normalize();
			DiffuseTextureMaterial.LightColor = new Vector4(1);
			//modelOffset = new Vector3();
			//model.Render();
			//modelOffset = new Vector3(3, 0, 0);
			model2.Render();

			video.Present();
		}
	}
}

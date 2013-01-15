using System;
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

		Font font;
		Texture2DI fontTexture;

		RasterizerStateI rasterizerState;
		DepthStencilStateI depthStencilState;
		SamplerStateI samplerState;
		BlendStateI blendState;

		public MainApp()
		#if WINDOWS
		: base("Models", 512, 512, WindowStartPositions.CenterCurrentScreen, WindowTypes.FrameSizable)
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
				video = Video.Init(VideoTypes.D3D11 | VideoTypes.D3D9 | VideoTypes.OpenGL, out videoType, root, this, true);
				#elif METRO
				video = Video.Create(VideoTypes.D3D11, out videoType, root, this, true);
				#elif XNA
				video = Video.Create(VideoTypes.XNA, out videoType, root, this);
				#endif
				
				FontMaterial.Init(video, "Data/", video.FileTag, ShaderVersions.Max, null);
				
				fontTexture = Texture2DAPI.New(video, "Data/Font.png", null);
				font = new Font(video, FontMaterial.Shader, fontTexture, "Data/Font.xml", null);
				
				var frame = FrameSize;
				viewPort = ViewPortAPI.New(video, 0, 0, frame.Width, frame.Height);
				camera = new Camera(viewPort, new Vector3(5, 5, 5), new Vector3(), new Vector3(5, 5+1, 5), 1, 50, MathUtilities.DegToRad(45));
				
				rasterizerState = RasterizerStateAPI.New(video, RasterizerStateDescAPI.New(RasterizerStateTypes.Solid_CullNone));
				depthStencilState = DepthStencilStateAPI.New(video, DepthStencilStateDescAPI.New(DepthStencilStateTypes.ReadWrite_Less));
				samplerState = SamplerStateAPI.New(video, SamplerStateDescAPI.New(SamplerStateTypes.Linear_Wrap));
				blendState = BlendStateAPI.New(video, BlendStateDescAPI.New(BlendStateTypes.Alpha));

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

		Time uTime;
		protected override void update(Time time)
		{
			if (!loaded) return;
			
			uTime = time;
			camera.RotateAroundLookLocationWorld(new Reign.Core.Vector3(.25f, .5f, .75f) * time.Delta);
		}

		protected override void render(Time time)
		{
			if (!loaded) return;

			var e = Loader.UpdateLoad();
			if (e != null)
			{
				Message.Show("File loading Error", e.Message);
				dispose();
				return;
			}
			if (Loader.ItemsRemainingToLoad != 0) return;
			
			video.Update();
			video.EnableRenderTarget();
			video.ClearColorDepth(0, 0, 0, 1);
			rasterizerState.Enable();
			depthStencilState.Enable();
			samplerState.Enable(0);
			blendState.Enable();

			viewPort.Size = FrameSize;
			viewPort.Apply();
			camera.Apply();
			
			font.DrawStart(camera);
			font.Draw(string.Format("RFPS: {0} - UFPS: {1}", time.FPS, uTime.FPS), new Vector2(), new Vector4(1), .5f, true, true);

			#if !XNA
			video.Present();
			#endif
		}
	}
}

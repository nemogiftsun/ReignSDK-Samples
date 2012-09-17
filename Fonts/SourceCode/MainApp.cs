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

		FontI font;
		Texture2DI fontTexture;

		RasterizerStateI rasterizerState;
		DepthStencilStateI depthStencilState;
		SamplerStateI samplerState;
		BlendStateI blendState;

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
				FontMaterial.Init(videoType, video, "Data\\", video.FileTag, ShaderVersions.Max);

				fontTexture = Texture2D.Create(videoType, video, "Data\\Font.png");
				font = Reign.Video.API.Font.Create(videoType, video, FontMaterial.Shader, fontTexture, "Data\\Font.xml");

				var frame = FrameSize;
				viewPort = ViewPort.Create(videoType, video, 0, 0, frame.Width, frame.Height);
				camera = new Camera(viewPort, new Vector3(5, 5, 5), new Vector3(), new Vector3(5, 5+1, 5), 1, 50, Reign.Core.Math.DegToRad(45));

				rasterizerState = RasterizerState.Create(videoType, video, RasterizerStateDesc.Create(videoType, RasterizerStateTypes.Solid_CullNone));
				depthStencilState = DepthStencilState.Create(videoType, video, DepthStencilStateDesc.Create(videoType, DepthStencilStateTypes.ReadWrite_Less));
				samplerState = SamplerState.Create(videoType, video, SamplerStateDesc.Create(videoType, SamplerStateTypes.Linear_Wrap));
				blendState = BlendState.Create(videoType, video, BlendStateDesc.Create(videoType, BlendStateTypes.Alpha));

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
			video.Clear(0, 0, 0, 1);
			rasterizerState.Enable();
			depthStencilState.Enable();
			samplerState.Enable(0);
			blendState.Enable();

			viewPort.Apply();
			camera.RotateAroundLookLocationWorld(.001f, .01f, .003f);
			camera.Apply();

			font.DrawStart(camera);
			font.Draw("Hello World!", new Vector2(), new Vector4(1), 1, true, true);

			#if !XNA
			video.Present();
			#endif
		}
	}
}

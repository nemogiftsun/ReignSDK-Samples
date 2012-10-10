using System;
using Reign.Core;
using Reign.Video;
using Reign.Video.API;
using Reign.Input;
using Reign.Input.API;
using ShaderMaterials.Shaders;

namespace Demo
{
	#if WINDOWS
	class MainApp : Window
	#elif METRO || XNA
	class MainApp : Application
	#endif
	{
		bool loaded;
		RootDisposable root;
		VideoI video;
		QuickDrawI qd;
		ViewPortI viewPort;
		Camera camera;
		QuickDraw3ColorUVMaterial material;
		Texture2DI texture;

		RasterizerStateI rasterizerState;
		SamplerStateI samplerState;
		BlendStateI blendState;
		DepthStencilStateI depthStencilState;

		InputI input;
		#if WINDOWS || METRO
		MouseI mouse;
		KeyboardI keyboard;
		#elif XNA
		GamePadI gamePad;
		#endif
		
		public MainApp()
		#if WINDOWS
		: base("QuickDraw Sample", 512, 512, WindowStartPositions.CenterCurrentScreen, WindowTypes.Frame)
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

				// video objects
				VideoTypes videoType;
				#if METRO
				VideoTypes createVideoTypes = VideoTypes.D3D11;
				#elif WINDOWS
				VideoTypes createVideoTypes = VideoTypes.D3D11 | VideoTypes.D3D9 | VideoTypes.OpenGL;
				#elif XNA
				VideoTypes createVideoTypes = VideoTypes.XNA;
				#endif

				#if WINDOWS || METRO
				video = Video.Create(createVideoTypes, out videoType, root, this, true);
				#elif XNA
				video = Video.Create(createVideoTypes, out videoType, root, this);
				#endif

				QuickDraw3ColorUVMaterial.Init(videoType, video, "Data\\", video.FileTag, ShaderVersions.Max);
				material = new QuickDraw3ColorUVMaterial();
				texture = Texture2D.Create(videoType, video, "Data\\Roxy.dds");
				qd = QuickDraw.Create(videoType, video, QuickDraw3ColorUVMaterial.BufferLayoutDesc);

				var frame = FrameSize;
				viewPort = ViewPort.Create(videoType, video, 0, 0, frame.Width, frame.Height);
				camera = new Camera(viewPort, new Vector3(0, 0, 5), new Vector3(), new Vector3(0, 0+1, 5));

				// states
				rasterizerState = RasterizerState.Create(videoType, video, RasterizerStateDesc.Create(videoType, RasterizerStateTypes.Solid_CullNone));
				samplerState = SamplerState.Create(videoType, video, SamplerStateDesc.Create(videoType, SamplerStateTypes.Linear_Wrap));
				blendState = BlendState.Create(videoType, video, BlendStateDesc.Create(videoType, BlendStateTypes.Alpha));
				depthStencilState = DepthStencilState.Create(videoType, video, DepthStencilStateDesc.Create(videoType, DepthStencilStateTypes.None));

				// input
				InputTypes inputType;
				#if METRO
				InputTypes createInputTypes = InputTypes.Metro;
				#elif WINDOWS
				InputTypes createInputTypes = InputTypes.WinForms;
				#elif XNA
				InputTypes createInputTypes = InputTypes.XNA;
				#endif
				input = Input.Create(createInputTypes, out inputType, root, this);
				#if WINDOWS || METRO
				mouse = Mouse.Create(inputType, input);
				keyboard = Keyboard.Create(inputType, input);
				#elif XNA
				gamePad = GamePad.Create(InputTypes.XNA, input, GamePadControllers.All);
				#endif

				loaded = true;
			}
			catch (Exception e)
			{
				Message.Show("Error", e.Message);
				dispose();
			}
		}

		private void dispose()
		{
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

		protected override void update(Time time)
		{
			#if WINDOWS || METRO
			if (keyboard.ArrowUp.On) camera.Zoom(1 * time.Delta, 1);
			if (keyboard.ArrowDown.On) camera.Zoom(-1 * time.Delta, 1);
			if (!mouse.Left.On) camera.RotateAroundLookLocationWorld(0, 1 * time.Delta, 0);
			else camera.RotateAroundLookLocation(mouse.Velocity.Y * 1 * time.Delta, mouse.Velocity.X * 1 * time.Delta, 0);
			#elif XNA
			if (gamePad.A.On) camera.Zoom(1 * time.Delta, 1);
			if (gamePad.B.On) camera.Zoom(-1 * time.Delta, 1);
			if (gamePad.LeftStick.Length() <= .1f) camera.RotateAroundLookLocationWorld(0, 1 * time.Delta, 0);
			else camera.RotateAroundLookLocation(-gamePad.LeftStick.Y * 1 * time.Delta, -gamePad.LeftStick.X * 1 * time.Delta, 0);
			if (gamePad.Back.Up) Exit();
			#endif
		}

		protected override void render(Time time)
		{
			if (!loaded) return;

			var e = Streams.TryLoad();
			if (e != null)
			{
				Message.Show("Error", e.Message);
				dispose();
				loaded = false;
			}
			if (Streams.ItemsRemainingToLoad != 0) return;

			input.Update();
			video.Update();
			video.EnableRenderTarget();
			rasterizerState.Enable();
			samplerState.Enable(0);
			blendState.Enable();
			depthStencilState.Enable();
			video.ClearColorDepth(0, .3f, .3f, 1);

			viewPort.Apply();
			camera.Apply();

			QuickDraw3ColorUVMaterial.Camera = camera.TransformMatrix;
			material.Diffuse = texture;
			material.Enable();
			material.Apply();
			qd.StartTriangles();
			    qd.Color(1, 1, 1, 1);
				qd.UV(0, 0); qd.Pos(-1, -1, 0);
			    qd.UV(0, 1); qd.Pos(-1, 1, 0);
			    qd.UV(1, 1); qd.Pos(1, 1, 0);

			    qd.UV(0, 0); qd.Pos(-1, -1, 0);
			    qd.UV(1, 1); qd.Pos(1, 1, 0);
			    qd.UV(1, 0); qd.Pos(1, -1, 0);
			qd.End();

			#if !XNA
			video.Present();
			#endif
		}
	}
}

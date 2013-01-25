using System;
using Reign.Core;
using Reign.Input;
using Reign.Input.API;
using System.IO;

namespace Demo
{
	#if WIN32 || OSX || LINUX
	class MainApp : Window
	#else
	class MainApp : Application
	#endif
	{
		bool loaded;
		RootDisposable root;
		InputI input;
		KeyboardI keyboard;

		public MainApp()
		#if WIN32 || OSX || LINUX
		: base("Keycodes", 512, 512, WindowStartPositions.CenterCurrentScreen, WindowTypes.Frame)
		#elif WINRT
		: base(ApplicationOrientations.Landscape)
		#endif
		{
			
		}

		protected override void shown()
		{
			try
			{
				root = new RootDisposable();

				#if WIN32
				InputTypes inputType = InputTypes.WinForms;
				#elif WINRT
				InputTypes inputType = InputTypes.WinRT;
				#elif OSX
				InputTypes inputType = InputTypes.Cocoa;
				#elif LINUX
				InputTypes inputType = InputTypes.X11;
				#endif

				input = Input.Init(inputType, out inputType, root, this);
				keyboard = KeyboardAPI.New(input);
				loaded = true;
			}
			catch (Exception e)
			{
				Message.Show("Error", e.Message);
			}
		}

		protected override void closing()
		{
			if (root != null)
			{
				root.Dispose();
				root = null;
			}
		}

		protected override void update(Time time)
		{
			if (!loaded) return;

			input.Update();
			for (int i = 0; i != 256; ++i)
			{
				#if WIN32 || WINRT
				if (keyboard.Key(i).Down) System.Diagnostics.Debug.WriteLine(i);
				#else
				if (keyboard.Key(i).Down) Console.WriteLine(i);
				#endif
			}
		}
	}
}

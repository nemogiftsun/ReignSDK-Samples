using System;
using Reign.Core;
using Reign.Input;
using Reign.Input.API;
using System.IO;

namespace Demo
{
	#if WINDOWS
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
		#if WINDOWS
		: base("Keycodes", 512, 512, WindowStartPositions.CenterCurrentScreen, WindowTypes.Frame)
		#else
		: base(ApplicationOrientations.Landscape)
		#endif
		{
			
		}

		protected override void shown()
		{
			try
			{
				root = new RootDisposable();

				#if WINDOWS
				InputTypes inputType = InputTypes.WinForms;
				#else
				InputTypes inputType = InputTypes.Metro;
				#endif

				input = Input.Create(inputType, out inputType, root, this);
				keyboard = Keyboard.Create(inputType, input);
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
				if (keyboard.Key(i).Down) System.Diagnostics.Debug.WriteLine(i);
			}
		}
	}
}

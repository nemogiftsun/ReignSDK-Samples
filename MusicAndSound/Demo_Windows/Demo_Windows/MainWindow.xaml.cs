using System;
using System.Windows;

using Core = Reign.Core;
using Reign.Audio;
using Reign.Audio.API;

namespace Demo_Windows
{
	public partial class MainWindow : Window
	{
		Core.RootDisposable root;
		AudioI audio;
		SoundI sound;
		SoundInstanceI soundInstance;

		public MainWindow()
		{
			InitializeComponent();

			root = new Core.RootDisposable();
			AudioTypes audioType;
			audio = Audio.Create(AudioTypes.XAudio | AudioTypes.Dumby, out audioType, root);
			sound = Sound.Create(audioType, audio, "Data\\Explo2.wav", 1, false);

			while (true)
			{
				var e = Core.Streams.TryLoad();
				if (e != null) throw e;
				if (Core.Streams.ItemsRemainingToLoad == 0) break;
			}

			System.Windows.Media.CompositionTarget.Rendering += render;
		}

		private void render(object sender, EventArgs e)
		{
			audio.Update();
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			if (root != null)
			{
				root.Dispose();
				root = null;
			}
			base.OnClosing(e);
		}

		private void playSound_Click(object sender, RoutedEventArgs e)
		{
			if (soundInstance == null) soundInstance = sound.Play(.5f);
			else soundInstance.Play(.5f);
		}

		private void pauseSound_Click(object sender, RoutedEventArgs e)
		{
			if (soundInstance != null) soundInstance.Pause();
		}

		private void stopSound_Click(object sender, RoutedEventArgs e)
		{
			if (soundInstance != null) soundInstance.Stop();
		}
	}
}

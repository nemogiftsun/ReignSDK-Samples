using System;
using System.Collections.Generic;
using System.IO;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using Core = Reign.Core;
using Reign.Audio;
using Reign.Audio.API;

namespace Demo_Metro
{
    public sealed partial class MainPage : Page
    {
		Core.RootDisposable root;
		AudioI audio;
		SoundI sound;
		SoundInstanceI soundInstance;

        public MainPage()
        {
            this.InitializeComponent();

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

			Windows.UI.Xaml.Media.CompositionTarget.Rendering += render;
        }

		private void render(object sender, object e)
		{
			audio.Update();
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			if (root != null)
			{
				root.Dispose();
				root = null;
			}
			base.OnNavigatedFrom(e);
		}

		private void playSound_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			if (soundInstance == null) soundInstance = sound.Play(.5f);
			else soundInstance.Play(.5f);
		}

		private void pauseSound_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			if (soundInstance != null) soundInstance.Pause();
		}

		private void stopSound_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			if (soundInstance != null) soundInstance.Stop();
		}
    }
}

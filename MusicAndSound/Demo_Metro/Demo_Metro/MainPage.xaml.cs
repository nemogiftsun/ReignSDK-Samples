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
			audio = Audio.Init(AudioTypes.XAudio | AudioTypes.Dumby, out audioType, root);
			sound = SoundAPI.New(audio, "Data\\Explo2.wav", 1, false, null);

			while (true)
			{
				var e = Core.Loader.UpdateLoad();
				if (e != null) throw e;
				if (Core.Loader.ItemsRemainingToLoad == 0) break;
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
			var instance = sound.Play(.5f);
			if (instance != null) soundInstance = instance;
			else if (soundInstance != null) soundInstance.Play(.5f);
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

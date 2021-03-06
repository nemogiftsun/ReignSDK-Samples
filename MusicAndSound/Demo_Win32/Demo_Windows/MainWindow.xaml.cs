﻿using System;
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
			audio = Audio.Init(AudioTypes.XAudio | AudioTypes.Dumby, out audioType, root);
			sound = SoundAPI.New(audio, "Data/Explo2.wav", 1, false, null);

			while (true)
			{
				var e = Core.Loader.UpdateLoad();
				if (e != null) throw e;
				if (Core.Loader.ItemsRemainingToLoad == 0) break;
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
			var instance = sound.Play(.5f);
			if (instance != null) soundInstance = instance;
			else if (soundInstance != null) soundInstance.Play(.5f);
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

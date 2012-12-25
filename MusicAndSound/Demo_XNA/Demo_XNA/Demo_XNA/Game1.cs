using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using Core =  Reign.Core;
using Reign.Audio;
using Reign.Audio.API;

namespace Demo_XNA
{
	public class Game1 : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		SpriteFont spriteFont;

		Core.RootDisposable root;
		AudioI audio;
		SoundI sound;
		SoundInstanceI soundInstance;

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "";
		}

		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);
			spriteFont = Content.Load<SpriteFont>("Data\\SpriteFont1");

			root = new Core.RootDisposable(Content);
			AudioTypes audioType;
			audio = Audio.Init(AudioTypes.XNA | AudioTypes.Dumby, out audioType, root);
			sound = SoundAPI.New(audio, "Data/Explo2.wav", 1, false, null, null);

			while (true)
			{
				var e = Core.Loader.UpdateLoad();
				if (e != null) throw e;
				if (Core.Loader.ItemsRemainingToLoad == 0) break;
			}
		}

		protected override void UnloadContent()
		{
			if (root != null)
			{
				root.Dispose();
				root = null;
			}
		}

		protected override void Update(GameTime gameTime)
		{
			// Allows the game to exit
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) this.Exit();

			audio.Update();

			if (GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed)
			{
				var instance = sound.Play(.5f);
				if (instance != null) soundInstance = instance;
				else if (soundInstance != null) soundInstance.Play(.5f);
			}

			if (GamePad.GetState(PlayerIndex.One).Buttons.X == ButtonState.Pressed)
			{
				if (soundInstance != null) soundInstance.Pause();
			}

			if (GamePad.GetState(PlayerIndex.One).Buttons.B == ButtonState.Pressed)
			{
				if (soundInstance != null) soundInstance.Stop();
			}

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			spriteBatch.Begin();
			spriteBatch.DrawString(spriteFont, "Sound: Play-A, Pause-X, Stop-B", new Vector2(), Color.Red);
			spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System;
using Devcade;
using Devcade.SaveData;
using Newtonsoft.Json;
using System.IO;

namespace SardineCanOpeningSimulatorTheGame
{
	public class Game1 : Game
	{
		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;
		private int openVal;
		private Texture2D can0;
		private Texture2D can1;
		private Texture2D can2;
		private Texture2D can3;
		private Texture2D can4;
		private SoundEffect canOpening0;
		private SoundEffect canOpening1;
		private SoundEffect canOpening2;
		private SoundEffect canOpening3;
		private SoundEffect canOpening4;
		private List<Texture2D> cans = new List<Texture2D>();
		private List<SoundEffect> sounds = new List<SoundEffect>();
		private List<(string Name,int Score)> highScores;
		private SpriteFont fish;
		private int score;
		private bool canClosed;
		private bool canOpened;
		private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrzwxyz";
		private int[] selectedName;
		private int currentLetter;
		private enum states {
			died,
			nameInput,
			nameEntered,
			alive
		}
		private states state;

		/// <summary>
		/// Game constructor
		/// </summary>
		public Game1()
		{
			_graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = false;
		}

		/// <summary>
		/// Does any setup prior to the first frame that doesn't need loaded content.
		/// </summary>
		protected override void Initialize()
		{
			Input.Initialize(); // Sets up the input library

			// Set window size if running debug (in release it will be fullscreen)
			#region
#if DEBUG
			_graphics.PreferredBackBufferWidth = 420;
			_graphics.PreferredBackBufferHeight = 980;
			_graphics.ApplyChanges();
#else
			_graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
			_graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
			_graphics.ApplyChanges();
#endif
			#endregion

			openVal = 0;

			canClosed = true;
			canOpened = false;
			
			state = states.alive;

			selectedName = new int[3] { 0, 1, 2};
			currentLetter = 0;
			// TODO: Add your initialization logic here

			base.Initialize();
		}

		/// <summary>
		/// Does any setup prior to the first frame that needs loaded content.
		/// </summary>
		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			can0 = Content.Load<Texture2D>("0");
			can1 = Content.Load<Texture2D>("1");
			can2 = Content.Load<Texture2D>("2");
			can3 = Content.Load<Texture2D>("3");
			can4 = Content.Load<Texture2D>("4");

			canOpening0 = Content.Load<SoundEffect>("0sound");
			canOpening1 = Content.Load<SoundEffect>("1sound");
			canOpening2 = Content.Load<SoundEffect>("2sound");
			canOpening3 = Content.Load<SoundEffect>("3sound");
			canOpening4 = Content.Load<SoundEffect>("4sound");

			fish = Content.Load<SpriteFont>("fish");

			cans.Add(can0);
			cans.Add(can1);
			cans.Add(can2);
			cans.Add(can3);
			cans.Add(can4);

			sounds.Add(canOpening0);
			sounds.Add(canOpening1);
			sounds.Add(canOpening2);
			sounds.Add(canOpening3);
			sounds.Add(canOpening4);

			if(!File.Exists("highscores.json")) {
				highScores = new List<(string Name,int Score)> {
					("",0)
				};
			} else {
				using (StreamReader file = File.OpenText("highscores.json"))
				{
					highScores = JsonConvert.DeserializeObject<List<(string Name, int Score)>>(File.ReadAllText("highscores.json"));
				}
			}

			//Console.WriteLine(highScores.Count);
			// TODO: use this.Content to load your game content here
			// ex.
			// texture = Content.Load<Texture2D>("fileNameWithoutExtention");
		}

		/// <summary>
		/// Your main update loop. This runs once every frame, over and over.
		/// </summary>
		/// <param name="gameTime">This is the gameTime object you can use to get the time since last frame.</param>
		protected override void Update(GameTime gameTime)
		{
			Input.Update(); // Updates the state of the input library

			// Exit when both menu buttons are pressed (or escape for keyboard debuging)
			// You can change this but it is suggested to keep the keybind of both menu
			// buttons at once for gracefull exit.
			if (Keyboard.GetState().IsKeyDown(Keys.Escape) ||
				(Input.GetButton(1, Input.ArcadeButtons.Menu) &&
				Input.GetButton(2, Input.ArcadeButtons.Menu)))
			{
				Exit();
			}

			if (gameTime.TotalGameTime.Milliseconds % 50 == 0) {

				switch (state) {
					case (states.died):
						for (int i = 0; i < highScores.Count ; i++) {
							if (score > highScores[i].Score) {
								state = states.nameInput;
							}
						}
						break;
					case (states.nameInput):
						if (Keyboard.GetState().IsKeyDown(Keys.W) || Input.GetStick(1).Y > 0.2) {
							if (selectedName[currentLetter] == alphabet.Length - 1)
								selectedName[currentLetter] = 0;
							selectedName[currentLetter]++;
						}
						if (Keyboard.GetState().IsKeyDown(Keys.S) || Input.GetStick(1).Y < -0.2) {
							if (selectedName[currentLetter] == 0) 
								selectedName[currentLetter] = alphabet.Length - 1;
							selectedName[currentLetter]--;
						}
						if (Keyboard.GetState().IsKeyDown(Keys.Enter) || Input.GetStick(1).X > 0.2) {
							if (currentLetter == 2) {
								state = states.nameEntered;
							} else {
								currentLetter++;
							}
						}
						break;
					case (states.nameEntered):
						currentLetter = 0;
						for (int i = 0; i < highScores.Count ; i++) {
							if (score > highScores[i].Score) {
								highScores.Insert(i,(Name : new String(new char[] {alphabet[selectedName[0]], alphabet[selectedName[1]], alphabet[selectedName[2]]}),Score : score));
								File.WriteAllText("highscores.json", JsonConvert.SerializeObject(highScores));
								break;
							}
						}
						state = states.alive;
						score = 0;
						break;
					case (states.alive):
						if (Keyboard.GetState().IsKeyDown(Keys.A) || Input.GetStick(1).X < -0.2) {
							if (openVal < 4) {
								openVal++;
								sounds[openVal].Play();
							}
						}

						if (Keyboard.GetState().IsKeyDown(Keys.D) || Input.GetStick(1).X > 0.2) {
							if (openVal > 0) {
								openVal--;
								sounds[openVal].Play();
							}
						}

						if (Input.GetStick(1).Y < -0.2 || Input.GetStick(1).Y > 0.2 || Keyboard.GetState().IsKeyDown(Keys.W) || Keyboard.GetState().IsKeyDown(Keys.S)) {
							state = states.nameInput;
						}

						if (openVal == 4) {
							canOpened = true;
						}
						if (openVal == 0) {
							canClosed = true;
						}
						if (canClosed && canOpened) {
							score++;
							canClosed = false;
							canOpened = false;
						}
						break;
				}
			}

			// TODO: Add your update logic here

			base.Update(gameTime);
		}

		/// <summary>
		/// Your main draw loop. This runs once every frame, over and over.
		/// </summary>
		/// <param name="gameTime">This is the gameTime object you can use to get the time since last frame.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);

			_spriteBatch.Begin();
			switch (state) {
				case (states.alive):
					_spriteBatch.Draw(cans[openVal],new Rectangle(0,0,_graphics.PreferredBackBufferWidth,_graphics.PreferredBackBufferHeight),Color.White);
					break;
				case (states.nameInput):
					_spriteBatch.Draw(cans[openVal],new Rectangle(0,0,_graphics.PreferredBackBufferWidth,_graphics.PreferredBackBufferHeight),Color.White);
					_spriteBatch.DrawString(fish, new String(new char[] {alphabet[selectedName[0]], alphabet[selectedName[1]], alphabet[selectedName[2]]}), new Vector2(_graphics.PreferredBackBufferWidth/4,_graphics.PreferredBackBufferHeight/4-150), Color.Black);

					for (int i = 0; i < highScores.Count; i++) {
						_spriteBatch.DrawString(fish, highScores[i].Name + ": " + highScores[i].Score, new Vector2(_graphics.PreferredBackBufferWidth/4,_graphics.PreferredBackBufferHeight/4 + i * 75), Color.Black);

					}
					break;

			}
			// TODO: Add your drawing code here
			_spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
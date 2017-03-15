using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System.IO;

namespace Project2
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class ModelRenderer : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		BasicEffect effect;

		Model lowerModel;

		public ModelRenderer()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			effect = new BasicEffect(GraphicsDevice);

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			StreamReader reader = new StreamReader(File.Open("model.txt", FileMode.Open));

			lowerModel = new Project2.Model(GraphicsDevice, "models\\players\\laracroft\\head.md3", "models\\players\\laracroft\\head_default.skin");
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// game-specific content.
		/// </summary>
		protected override void UnloadContent()
		{
			
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

			// TODO: Add your update logic here

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			Matrix world = Matrix.CreateScale((float) 1 / 64);
			Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 100), Vector3.Zero, Vector3.Up);
			Matrix projection = Matrix.CreatePerspectiveFieldOfView(.9f, GraphicsDevice.Viewport.AspectRatio, .01f, 200);

			effect.LightingEnabled = true;
			effect.TextureEnabled = true;
			effect.VertexColorEnabled = false;
			effect.DirectionalLight0.DiffuseColor = new Vector3(.5f, .5f, .5f);
			effect.DirectionalLight0.SpecularColor = new Vector3(.25f, .25f, .25f);
			effect.DirectionalLight0.Direction = new Vector3(-1, 0, -1);
			effect.AmbientLightColor = new Vector3(.2f, .2f, .2f);

			GraphicsDevice.Clear(Color.CornflowerBlue);

			base.Draw(gameTime); //yeeeeaaaah draw it baby
		}
	}
}

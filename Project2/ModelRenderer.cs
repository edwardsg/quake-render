using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Project2
{
	// Handles keyboard input and program initialization
	public class ModelRenderer : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
        BasicEffect effect;

		// Window size
		int windowWidth = 1600;
		int windowHeight = 800;

		// Camera starting position, rotation and zoom speed
		Vector3 cameraPosition = new Vector3(0, 0, 100);
		float cameraRotateSpeed = .01f;
		float cameraMoveSpeed = .02f;
		float minCameraDistance = 10;
		float maxCameraDistance = 300;

		// Projection
		float viewAngle = .9f;
		float nearPlane = .01f;
		float farPlane = 500;
		
        bool enterPressed = false;
        float cameraRotation = 0;
		float cameraZoom = 0;

		// Player models
		MD3[] players;
		Vector3[] playerPositions;

		public ModelRenderer()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}
		
		protected override void Initialize()
		{
			// Set window position
			int screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
			int screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
			Window.Position = new Point(screenWidth / 2 - windowWidth / 2, screenHeight / 2 - windowHeight / 2);

			// Set window size
			graphics.PreferredBackBufferWidth = windowWidth;
			graphics.PreferredBackBufferHeight = windowHeight;
			graphics.ApplyChanges();

			// Set window title
			Window.Title = "Quake Model Renderer";

			effect = new BasicEffect(GraphicsDevice);

			base.Initialize();
		}
		
		protected override void LoadContent()
		{
            spriteBatch = new SpriteBatch(GraphicsDevice);

			// Load player models from files
			players = new MD3[2] { new MD3(effect, "model.txt"), new MD3(effect, "modelSarge.txt") };

			// Set starting player positions
			playerPositions = new Vector3[2] { new Vector3(-30, 0, 0), new Vector3(30, 0, 0) };
		}

		protected override void UnloadContent()
		{
			
		}

		protected override void Update(GameTime gameTime)
		{
            KeyboardState keyboard = Keyboard.GetState();

			// Milliseconds passed since last update
			float milliPassed = gameTime.ElapsedGameTime.Milliseconds;

			// Exit program with escape key
			if (keyboard.IsKeyDown(Keys.Escape))
				Exit();

			// Cycling through animations with enter key
            if (keyboard.IsKeyDown(Keys.Enter))
            {
                if (enterPressed == false)
                {
                    enterPressed = true;

					// Increment animations for all models
					foreach (MD3 player in players)
						player.IncrementAnimation();
                }
            }

            if (keyboard.IsKeyUp(Keys.Enter))
                enterPressed = false;

			// Camera rotation - left and right arrows
			if (keyboard.IsKeyDown(Keys.Left))
				cameraRotation = -cameraRotateSpeed * milliPassed;
			else if (keyboard.IsKeyDown(Keys.Right))
				cameraRotation = cameraRotateSpeed * milliPassed;
			else
				cameraRotation = 0;

			// Camera zoom - up and down arrows
			if (keyboard.IsKeyDown(Keys.Up) && cameraPosition.Length() > minCameraDistance)
				cameraZoom = cameraMoveSpeed;
			else if (keyboard.IsKeyDown(Keys.Down) && cameraPosition.Length() < maxCameraDistance)
				cameraZoom = -cameraMoveSpeed;
			else
				cameraZoom = 0;

			foreach (MD3 player in players)
				player.Update(milliPassed / 1000);

            base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			// Rotate camera around origin
			Matrix cameraRotateY = Matrix.CreateRotationY(cameraRotation);
			cameraPosition += cameraPosition * -cameraZoom;
			cameraPosition = Vector3.Transform(cameraPosition, cameraRotateY);

			// Set up scale, camera direction, and perspective projection
			Matrix world = Matrix.CreateRotationX(-MathHelper.PiOver2);
			Matrix view = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);
			Matrix projection = Matrix.CreatePerspectiveFieldOfView(viewAngle, GraphicsDevice.Viewport.AspectRatio, nearPlane, farPlane);
			
			effect.View = view;
            effect.Projection = projection;

			// Set up lighting
			effect.LightingEnabled = true;
			effect.TextureEnabled = true;
			effect.VertexColorEnabled = false;
			effect.DirectionalLight0.DiffuseColor = new Vector3(.5f, .5f, .5f);
			effect.DirectionalLight0.SpecularColor = new Vector3(.25f, .25f, .25f);
			effect.DirectionalLight0.Direction = new Vector3(-1, 0, -1);
			effect.AmbientLightColor = new Vector3(.2f, .2f, .2f);

			GraphicsDevice.Clear(Color.CornflowerBlue);

			// Render player models
			for (int i = 0; i < players.Length; ++i)
			{
				// Move to respective positions then render
				effect.World = world * Matrix.CreateTranslation(playerPositions[i]);
				players[i].Render(effect);
			}

            base.Draw(gameTime);
		}
	}
}

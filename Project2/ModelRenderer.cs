using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Project2
{
	public class ModelRenderer : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
        BasicEffect effect;

		// Camera starting position and rotation speed
		Vector3 cameraPosition = new Vector3(0, 0, 100);
		float cameraRotateSpeed = 0.01f;

		// Scale of model
		float scale = (float) 1 / 64;

		// Projection
		float viewAngle = .9f;
		float nearPlane = .01f;
		float farPlane = 200;
		
        bool enterPressed = false;
        int currentAnimation = 0;
        float cameraRotation = 0;

		// Player model
		MD3 player;

		public ModelRenderer()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}
		
		protected override void Initialize()
		{
			Window.Title = "Quake Model Renderer";

			effect = new BasicEffect(GraphicsDevice);

			base.Initialize();
		}
		
		protected override void LoadContent()
		{
            spriteBatch = new SpriteBatch(GraphicsDevice);

			// Load model from file model.txt
			player = new MD3(GraphicsDevice, "model.txt");
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

			player.Update(milliPassed / 1000);

            base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			// Rotate camera around origin
			Matrix cameraRotateY = Matrix.CreateRotationY(cameraRotation);
			cameraPosition = Vector3.Transform(cameraPosition, cameraRotateY);

			// Set up scale, camera direction, and perspective projection
			Matrix world = Matrix.CreateScale(scale) * Matrix.CreateRotationX(-MathHelper.PiOver2);
			Matrix view = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);
			Matrix projection = Matrix.CreatePerspectiveFieldOfView(viewAngle, GraphicsDevice.Viewport.AspectRatio, nearPlane, farPlane);
			
			effect.World = world;
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

			// Render player model
			player.Render(effect);

            base.Draw(gameTime);
		}
	}
}

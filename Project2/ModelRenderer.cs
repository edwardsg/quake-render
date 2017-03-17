using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System.IO;

namespace Project2
{
	public class ModelRenderer : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
        BasicEffect effect;
		
        bool enterPressed = false;
        int currentAnimation = 0;

		Vector3 cameraPosition = new Vector3(0, 0, 100);
		float cameraRotateSpeed = 0.01f;

		float scale = (float) 1 / 64;

		float viewAngle = .9f;
		float nearPlane = .01f;
		float farPlane = 200;

        float cameraRotation = 0;

		MD3 lara;

		public ModelRenderer()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}

		
		protected override void Initialize()
		{
			effect = new BasicEffect(GraphicsDevice);

			base.Initialize();
		}

		
		protected override void LoadContent()
		{
            spriteBatch = new SpriteBatch(GraphicsDevice);

			lara = new MD3(GraphicsDevice, "model.txt");
		}

		

		protected override void UnloadContent()
		{
			
		}

		
		protected override void Update(GameTime gameTime)
		{
            KeyboardState keyboard = Keyboard.GetState();
			float milliPassed = gameTime.ElapsedGameTime.Milliseconds;

            if (keyboard.IsKeyDown(Keys.Enter))
            {
                if (enterPressed == false)
                {
                    enterPressed = true;
                    currentAnimation++;
                }
            }

            if (keyboard.IsKeyUp(Keys.Enter))
            {
                enterPressed = false;
            }

			if (keyboard.IsKeyDown(Keys.Left))
				cameraRotation = -cameraRotateSpeed * milliPassed;
			else if (keyboard.IsKeyDown(Keys.Right))
				cameraRotation = cameraRotateSpeed * milliPassed;
			else
				cameraRotation = 0;

			lara.Update(milliPassed / 1000);

            base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			Matrix cameraRotateY = Matrix.CreateRotationY(cameraRotation);
			cameraPosition = Vector3.Transform(cameraPosition, cameraRotateY);

			Matrix world = Matrix.CreateScale(scale);
			Matrix view = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);
			Matrix projection = Matrix.CreatePerspectiveFieldOfView(viewAngle, GraphicsDevice.Viewport.AspectRatio, nearPlane, farPlane);
			
			effect.World = world;
			effect.View = view;
            effect.Projection = projection;

			effect.LightingEnabled = true;
			effect.TextureEnabled = true;
			effect.VertexColorEnabled = false;
			effect.DirectionalLight0.DiffuseColor = new Vector3(.5f, .5f, .5f);
			effect.DirectionalLight0.SpecularColor = new Vector3(.25f, .25f, .25f);
			effect.DirectionalLight0.Direction = new Vector3(-1, 0, -1);
			effect.AmbientLightColor = new Vector3(.2f, .2f, .2f);

			GraphicsDevice.Clear(Color.CornflowerBlue);

			lara.Render(effect);

            base.Draw(gameTime);
		}
	}
}

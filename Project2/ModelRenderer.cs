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

        float rotateY;
        Matrix rotationMatrix;
        bool enterPressed = false;
        int currentAnimation = 0;
        Matrix world;
        Matrix view;
        Matrix projection;
        Vector3 trans;
        Matrix position;

        BasicEffect effect;

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
            world = Matrix.CreateScale((float) 1 / 64) * Matrix.CreateTranslation(new Vector3(0, 0, 0));
            view = Matrix.CreateLookAt(new Vector3(0, 0, 100), Vector3.Zero, Vector3.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(.9f, GraphicsDevice.Viewport.AspectRatio, .01f, 200.0f);
            spriteBatch = new SpriteBatch(GraphicsDevice);

			lara = new MD3(GraphicsDevice, "model.txt");
		}

		

		protected override void UnloadContent()
		{
			
		}

		
		protected override void Update(GameTime gameTime)
		{
            KeyboardState keyboard = Keyboard.GetState();

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
            {
                rotateY -= 0.1f * gameTime.ElapsedGameTime.Milliseconds;
            }

            if (keyboard.IsKeyDown(Keys.Right)) 
            {
                rotateY += 0.1f  * gameTime.ElapsedGameTime.Milliseconds;
            }

            rotationMatrix = Matrix.CreateFromYawPitchRoll(rotateY, 0, 0);

            trans += rotateY * rotationMatrix.Right;
            position = Matrix.CreateTranslation(trans);
            effect.View = view * position * rotationMatrix;

            base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
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

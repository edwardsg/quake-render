using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace Project2
{
	// Contains an entire MD3 character model made up of multiple models
    class MD3
    {
		// All types of animation in MD3
        enum AnimationType
        {
            BOTH_DEATH1 = 0,
            BOTH_DEAD1 = 1,
            BOTH_DEATH2 = 2,
            BOTH_DEAD2 = 3,
            BOTH_DEATH3 = 4,
            BOTH_DEAD3 = 5,

            TORSO_GESTURE = 6,
            TORSO_ATTACK = 7,
            TORSO_ATTACK2 = 8,
            TORSO_DROP = 9,
            TORSO_RAISE = 10,
            TORSO_STAND = 11,
            TORSO_STAND2 = 12,

            LEGS_WALKCR = 13,
            LEGS_WALK = 14,
            LEGS_RUN = 15,
            LEGS_BACK = 16,
            LEGS_SWIM = 17,
            LEGS_JUMP = 18,
            LEGS_LAND = 19,
            LEGS_JUMPB = 20,
            LEGS_LANDB = 21,
            LEGS_IDLE = 22,
            LEGS_IDLECR = 23,
            LEGS_TURN = 24,

            MAX_ANIMATIONS
        };

		// Information for a single animation
		public struct Animation
		{
			public int firstFrame;		// Starting frame of animation
			public int totalFrames;		// Number of frames in animation
			public int loopingFrames;	// Unused
			public int fps;				// Frames per second
		}
		
		// Submodels
        private Model lowerModel;
        private Model upperModel;
        private Model headModel;
        private Model gunModel;

		// All animations for this model
        private Animation[] animations = new Animation[(int) AnimationType.MAX_ANIMATIONS];
        private int currentAnimation;

		// Loads model data from file containing paths to all models, skins, and animation
		public MD3(BasicEffect effect, string modelFilePath)
		{
			StreamReader reader = new StreamReader(File.Open(modelFilePath, FileMode.Open));

			// Geth file paths
			string lowerModelPath = reader.ReadLine();
			string lowerSkinPath = reader.ReadLine();
			string upperModelPath = reader.ReadLine();
			string upperSkinPath = reader.ReadLine();
			string headModelPath = reader.ReadLine();
			string headSkinPath = reader.ReadLine();
			string gunModelPath = reader.ReadLine();
			string gunSkinPath = reader.ReadLine();
			string animationPath = reader.ReadLine();

			Model.SetUp();

			// Create models from files
			lowerModel = new Model(lowerModelPath, lowerSkinPath, effect);
			upperModel = new Model(upperModelPath, upperSkinPath, effect);
			headModel = new Model(headModelPath, headSkinPath, effect);
			gunModel = new Model(gunModelPath, gunSkinPath, effect);

			// Load animation data
			LoadAnimation(animationPath);

			currentAnimation = (int) AnimationType.BOTH_DEATH1;
			SetAnimation();

            lowerModel.Link("tag_torso", upperModel);
            upperModel.Link("tag_head", headModel);
            upperModel.Link("tag_weapon", gunModel);
        }

		// Loads animation data from file
        public void LoadAnimation(string animationPath)
        {
			StreamReader reader = new StreamReader(File.Open(animationPath, FileMode.Open));

			string[] numbers;

			// Loop through all lines, ignoring anything but those with numbers
			int index = 0;
			while (index < animations.Length)
			{
				if (Char.IsDigit((char)reader.Peek()))
				{
					numbers = reader.ReadLine().Split('\t');
					animations[index].firstFrame = Convert.ToInt32(numbers[0]);
					animations[index].totalFrames = Convert.ToInt32(numbers[1]);
					animations[index].loopingFrames = Convert.ToInt32(numbers[2]);
					animations[index].fps = Convert.ToInt32(numbers[3]);

					++index;
				}
				else
					reader.ReadLine();
			}

			// Fix differences for leg animations
			int legsTorsoDifference = animations[(int)AnimationType.LEGS_WALKCR].firstFrame - animations[(int)AnimationType.TORSO_GESTURE].firstFrame;
			for (int i = (int)AnimationType.LEGS_WALKCR; i < (int)AnimationType.MAX_ANIMATIONS; ++i)
				animations[i].firstFrame -= legsTorsoDifference;
		}

		// Sets animation data in appropriate models based on current animation being used
		public void SetAnimation()
        {
			// Animations that apply for both upper and lower models
			if (currentAnimation <= (int)AnimationType.BOTH_DEAD3)
			{
				upperModel.SetAnimation(animations[currentAnimation]);
				lowerModel.SetAnimation(animations[currentAnimation]);
			}
			// Animations that only affect upper model
			else if (currentAnimation <= (int)AnimationType.TORSO_STAND2)
			{
				upperModel.SetAnimation(animations[currentAnimation]);
				lowerModel.SetAnimation(animations[(int) AnimationType.LEGS_IDLE]);
			}
			// Only lower model
			else
			{
				upperModel.SetAnimation(animations[(int)AnimationType.TORSO_STAND]);
				lowerModel.SetAnimation(animations[currentAnimation]);
			}
        }

		// Moves current animation along one, loops to beginning
        public void IncrementAnimation()
        {
			currentAnimation = (currentAnimation + 1) % (int) AnimationType.MAX_ANIMATIONS;
			SetAnimation();
        }

		// Updates each model with amount between keyframes
        public void Update(float secondsElapsed)
        {
			float frameFraction = secondsElapsed * animations[currentAnimation].fps;

			lowerModel.UpdateFrame(frameFraction);
			upperModel.UpdateFrame(frameFraction);
			headModel.UpdateFrame(frameFraction);
			gunModel.UpdateFrame(frameFraction);
        }

		// Renders whole player model
        public void Render(BasicEffect effect)
        {
			Matrix current = Matrix.Identity;
			Matrix next = Matrix.Identity;

			lowerModel.DrawAllModels(current, next, effect);
        }
    }
}



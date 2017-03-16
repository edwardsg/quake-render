﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace Project2
{
    class MD3
    {
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

		struct Animation
		{
			public int firstFrame;
			public int totalFrames;
			public int loopingFrames;
			public int fps;
		}

        private Model lowerModel;
        private Model upperModel;
        private Model headModel;
        private Model gunModel;

        private Animation[] animations = new Animation[(int) AnimationType.MAX_ANIMATIONS];
        private int currentAnimation;

		GraphicsDevice device;

		public MD3(GraphicsDevice device, string modelFilePath)
		{
			this.device = device;

			StreamReader reader = new StreamReader(File.Open(modelFilePath, FileMode.Open));

			string lowerModelPath = reader.ReadLine();
			string lowerSkinPath = reader.ReadLine();
			string upperModelPath = reader.ReadLine();
			string upperSkinPath = reader.ReadLine();
			string headModelPath = reader.ReadLine();
			string headSkinPath = reader.ReadLine();
			string gunModelPath = reader.ReadLine();
			string gunSkinPath = reader.ReadLine();
			string animationPath = reader.ReadLine();

			Model.SetUp(device);

			lowerModel = new Model(lowerModelPath, lowerSkinPath);
			upperModel = new Model(upperModelPath, upperSkinPath);
			headModel = new Model(headModelPath, headSkinPath);
			gunModel = new Model(gunModelPath, gunSkinPath);

			LoadAnimation(animationPath);

			currentAnimation = (int)AnimationType.LEGS_SWIM;
			SetAnimation();
		}

        public void LoadAnimation(string animationPath)
        {
			StreamReader reader = new StreamReader(File.Open(animationPath, FileMode.Open));

			string[] numbers;

			for (int i = 0; i < animations.Length; ++i)
			{
				if (Char.IsDigit((char)reader.Peek()))
				{
					numbers = reader.ReadLine().Split(' ');
					animations[i].firstFrame = Convert.ToInt32(numbers[0]);
					animations[i].totalFrames = Convert.ToInt32(numbers[1]);
					animations[i].loopingFrames = Convert.ToInt32(numbers[2]);
					animations[i].fps = Convert.ToInt32(numbers[3]);
				}
			}

			for (int i = (int) AnimationType.LEGS_WALKCR; i < (int) AnimationType.MAX_ANIMATIONS; ++i)
				animations[i].firstFrame -= animations[(int) AnimationType.TORSO_GESTURE].firstFrame;
        }

        public void SetAnimation()
        {
			if (currentAnimation <= (int)AnimationType.BOTH_DEAD3)
			{
				upperModel.StartFrame = animations[currentAnimation].firstFrame;
				upperModel.EndFrame = animations[currentAnimation].totalFrames - 1;
				upperModel.NextFrame = animations[currentAnimation].firstFrame;
				upperModel.CurrentFrame = animations[currentAnimation].firstFrame + 1;

				lowerModel.StartFrame = animations[currentAnimation].firstFrame;
				lowerModel.EndFrame = animations[currentAnimation].totalFrames - 1;
				lowerModel.NextFrame = animations[currentAnimation].firstFrame;
				lowerModel.CurrentFrame = animations[currentAnimation].firstFrame + 1;
			}
			else if (currentAnimation <= (int)AnimationType.TORSO_STAND2)
			{
				upperModel.StartFrame = animations[currentAnimation].firstFrame;
				upperModel.EndFrame = animations[currentAnimation].totalFrames - 1;
				upperModel.NextFrame = animations[currentAnimation].firstFrame;
				upperModel.CurrentFrame = animations[currentAnimation].firstFrame + 1;
			}
			else
			{
				lowerModel.StartFrame = animations[currentAnimation].firstFrame;
				lowerModel.EndFrame = animations[currentAnimation].totalFrames - 1;
				lowerModel.NextFrame = animations[currentAnimation].firstFrame;
				lowerModel.CurrentFrame = animations[currentAnimation].firstFrame + 1;
			}
        }

        public void IncrementAnimation()
        {

        }

        public void Update(float secondsElapsed)
        {
			float frameFraction = secondsElapsed * animations[currentAnimation].fps;

			lowerModel.UpdateFrame(frameFraction);
			upperModel.UpdateFrame(frameFraction);
			headModel.UpdateFrame(frameFraction);
			gunModel.UpdateFrame(frameFraction);
        }

        public void Render(BasicEffect effect)
        {
			Matrix current = Matrix.Identity;
			Matrix next = Matrix.Identity;

			upperModel.DrawAllModels(current, next, effect);
        }
    }
}



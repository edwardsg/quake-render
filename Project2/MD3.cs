﻿
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Project2
{
    class MD3
    {
        enum AnimationTypes
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

        Model lowerModel;
        Model upperModel;
        Model headModel;
        Model gunModel;
        Animation[] animations;
        int currentAnimation;

        int firstFrame;
        int totalFrames;
        int loopingFrames;
        int FPS;

        public static void LoadAnimation()
        {

        }

        public static void SetAnimation()
        {

        }

        public static void IncrementAnimation()
        {

        }

        public static void Update()
        {

        }

        public static void UpdateFrame()
        {

        }

        public static void Render(Model model, BasicEffect effect)
        {
            Model.SetUp();
            Matrix current = new Matrix();
            Matrix next = new Matrix();
            //DrawAllModels(model, current, next);
            Model.DrawModel(model);
        }

    }
}



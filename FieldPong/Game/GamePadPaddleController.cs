#region Using statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Physics = FarseerGames.FarseerPhysics;
#endregion

namespace FieldPong
{
    /// <summary>
    /// This paddle controller makes a paddle react to input from an Xbox 360 game pad.
    /// </summary>
    class GamePadPaddleController : PaddleController
    {
        #region Fields

        TimeSpan vibrationInterval = TimeSpan.FromSeconds(0.5f);
        TimeSpan vibrationTimer = TimeSpan.Zero;
        bool vibrateNow = false;

        int level = 1;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the level the player has reached.
        /// </summary>
        public int Level
        {
            get { return level; }
            set { level = value; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new instance of GamePadPaddleController.
        /// </summary>
        public GamePadPaddleController(ActorManager actorManager)
            : base(actorManager)
        {
        }

        #endregion

        #region Update

        /// <summary>
        /// Reads the game pad input and reacts accordingly.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Move(GamePad.GetState(PlayerIndex.One).ThumbSticks.Left);
            Spin(GamePad.GetState(PlayerIndex.One).Triggers.Right);
            Spin(-GamePad.GetState(PlayerIndex.One).Triggers.Left);

            if (GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.Length() >= 0.3f)
            {
                Vector2 direction = Vector2.Normalize(GamePad.GetState(PlayerIndex.One).ThumbSticks.Right);
                direction.Y = -direction.Y;

                if (level < 20)
                    ShootSingleBullet(direction);
                else
                    ShootBulletSpray(direction);
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.X == ButtonState.Pressed)
            {
                bool success = ShootGravityBall(1);

                if (success)
                    vibrateNow = true;

                if (vibrateNow)
                    GamePad.SetVibration(PlayerIndex.One, 0.5f, 0.5f);
            }

            if (vibrateNow)
            {
                vibrationTimer += gameTime.ElapsedGameTime;

                if (vibrationTimer >= vibrationInterval)
                {
                    vibrationTimer = TimeSpan.Zero;
                    vibrateNow = false;

                    GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.0f);
                }
            }
        }

        #endregion
    }
}
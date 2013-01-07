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
    /// This behavior controls a paddle using basic AI.
    /// </summary>
    class AIPaddleController : PaddleController
    {
        #region Fields

        // We need to know where the ball is at least.
        Actor ball = null;

        Random randomGen;

        // This is the "level" the player has reached. It's used to scale the difficulty of the AI.
        int level = 1;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the reference to the ball actor.
        /// </summary>
        public Actor Ball
        {
            get { return ball; }
            set { ball = value; }
        }

        /// <summary>
        /// Gets or sets the level the player has reached, which is used here to scale the difficulty of the AI.
        /// </summary>
        public int Level
        {
            get { return level; }
            set { level = value; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes a new instance of AIPaddleController.
        /// </summary>
        public AIPaddleController(ActorManager actorManager)
            : base(actorManager)
        {
            randomGen = new Random();
        }

        #endregion

        #region Update

        /// <summary>
        /// This method contains the actual AI. This tries to line up the paddle with the ball, and if the ball
        /// is close it spins the paddle in an attempt to hit it. It also will make the computer fire
        /// at the ball to try to push it over past the player's paddle.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float forceRatio = level * (1.0f / 20.0f);

            if (ball.PhysicsBody.Position.X > parentActor.PhysicsBody.Position.X)
            {
                MoveRight(forceRatio);

                if (Math.Abs(ball.PhysicsBody.Position.Y - parentActor.PhysicsBody.Position.Y) < 100)
                    Spin(forceRatio);
            }
            else if (ball.PhysicsBody.Position.X < parentActor.PhysicsBody.Position.X)
            {
                MoveLeft(forceRatio);

                if (Math.Abs(ball.PhysicsBody.Position.Y - parentActor.PhysicsBody.Position.Y) < 100)
                    Spin(-forceRatio);
            }

            // The probability of the AI shooting a projectile goes up as the square of the player's level.
            if (randomGen.NextDouble() < 0.001f * (level * level))
                ShootGravityBall(-1);

            if (randomGen.NextDouble() < 0.001f * (level * level) && 
                ball.PhysicsBody.Position.Y > parentActor.PhysicsBody.Position.Y)
            {
                Vector2 direction = ball.PhysicsBody.Position - parentActor.PhysicsBody.Position;
                direction.Normalize();

                ShootSingleBullet(direction);
            }
        }

        #endregion
    }
}
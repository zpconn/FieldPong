#region Using statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Physics = FarseerGames.FarseerPhysics;
#endregion

namespace FieldPong
{
    /// <summary>
    /// This behavior causes an actor to initially shoot off moving in a random direction with a random speed.
    /// </summary>
    class ShootOffRandomlyBehavior : ActorBehavior
    {
        #region Fields

        Random randomGen = new Random();
        float minSpeed = 0.0f;
        float maxSpeed = 0.0f;
        bool justStarting = true;

        #endregion

        #region Properties

        /// <summary>
        /// This signifies whether things are just getting started so that the behavior should give the actor
        /// a random push.
        /// </summary>
        public bool JustStarting
        {
            get { return justStarting; }
            set { justStarting = value; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new ShootOffRandomlyBehavior. The parameters minSpeed and maxSpeed specify the range from which
        /// a random speed will be selected.
        /// </summary>
        /// <param name="actorManager"></param>
        public ShootOffRandomlyBehavior(ActorManager actorManager, float minSpeed, float maxSpeed)
            : base(actorManager)
        {
            this.minSpeed = minSpeed;
            this.maxSpeed = maxSpeed;
        }

        #endregion

        #region Update

        /// <summary>
        /// If JustStarting is set to true, then this method will give the actor a random initial velocity and set
        /// JustStarting to false.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (justStarting)
            {
                float angle = 2.0f * (float)Math.PI * (float)randomGen.NextDouble();
                float speed = minSpeed + (float)randomGen.NextDouble() * (maxSpeed - minSpeed);

                parentActor.PhysicsBody.LinearVelocity = new Vector2(speed * (float)Math.Cos(angle),
                                                                     speed * (float)Math.Sin(angle));

                justStarting = false;
            }
        }

        #endregion
    }
}
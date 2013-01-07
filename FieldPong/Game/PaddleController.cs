#region Using statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Physics = FarseerGames.FarseerPhysics;
#endregion

namespace FieldPong
{
    /// <summary>
    /// This actor behavior implements some methods for the basic actions that a paddle can perform. New controllers
    /// are derived from this base controller class, one that receives input from a gamepad and another that
    /// uses AI to choose which action to take. This class automatically sets some parameters in its parent actor's
    /// physics body.
    /// </summary>
    class PaddleController : ActorBehavior
    {
        #region Fields

        // These constants fine tune various aspects of a paddle's motion.
        protected const float MoveForce = 700.0f;
        protected const float WhackTorque = 60000.0f;
        protected const float GravityBallVelocity = 100.0f;
        protected const float BulletVelocity = 500.0f;

        protected TimeSpan gravityBallShootInterval = TimeSpan.FromSeconds(4);
        protected TimeSpan gravityBallShootTimer = TimeSpan.FromSeconds(4);

        protected TimeSpan bulletShootInterval = TimeSpan.FromSeconds(0.1f);
        protected TimeSpan bulletShootTimer = TimeSpan.FromSeconds(0.1f);

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new instance of PaddleController.
        /// </summary>
        public PaddleController(ActorManager actorManager)
            : base(actorManager)
        {
        }

        #endregion

        #region Paddle Actions

        /// <summary>
        /// Applies a force to the paddle in the leftward direction.
        /// </summary>
        /// <param name="forceRatio">The proportion of the maximum force to apply.</param>
        protected void MoveLeft(float forceRatio)
        {
            Vector2 force = new Vector2(-forceRatio * MoveForce, 0.0f);
            parentActor.PhysicsBody.ApplyForce(force);
        }

        /// <summary>
        /// Applies a force to the paddle in the rightward direction.
        /// </summary>
        /// <param name="forceRatio">The proportion of the maximum force to apply.</param>
        protected void MoveRight(float forceRatio)
        {
            Vector2 force = new Vector2(Math.Abs(forceRatio) * MoveForce, 0.0f);
            parentActor.PhysicsBody.ApplyForce(force);
        }

        /// <summary>
        /// Applies a force to the paddle in the direction determined by the sign of the force ratio.
        /// </summary>
        /// <param name="forceRatio">The proportion of the maximum force to apply.</param>
        protected void MoveHorizontally(float signedForceRatio)
        {
            Vector2 force = new Vector2(signedForceRatio * MoveForce, 0.0f);
            parentActor.PhysicsBody.ApplyForce(force);
        }

        /// <summary>
        /// Applies a force in the specified direction. The magnitude of the direction vector is taken to be
        /// the proportion of the maximum force to apply.
        /// </summary>
        /// <param name="direction"></param>
        protected void Move(Vector2 direction)
        {
            Vector2 force = new Vector2(direction.X * MoveForce, -direction.Y * MoveForce);
            parentActor.PhysicsBody.ApplyForce(force);
        }

        /// <summary>
        /// Applies a torque to the physics body.
        /// </summary>
        /// <param name="torqueRatio">The proportion of the maximum torque to apply.</param>
        protected void Spin(float torqueRatio)
        {
            parentActor.PhysicsBody.ApplyTorque(torqueRatio * WhackTorque);
        }

        /// <summary>
        /// This shoots a gravity ball into the arena.
        /// </summary>
        /// <param name="verticalDirection">1 means shoot up; -1 means shoot down.</param>
        protected bool ShootGravityBall(int verticalDirection)
        {
            // Make sure the timer has elapsed so we can shoot.
            if (gravityBallShootTimer >= gravityBallShootInterval)
            {
                gravityBallShootTimer = TimeSpan.Zero;

                Actor gravBall = actorManager.InstantiateTemplate("Gravity Ball");

                // Position the gravity ball a bit away from the paddle so the paddle doesn't collide with it immediately.

                gravBall.PhysicsBody.Position = parentActor.PhysicsBody.Position + new Vector2(0, 85 * -verticalDirection);

                // Now send it on its way!

                gravBall.PhysicsBody.LinearVelocity = new Vector2(0, GravityBallVelocity * -verticalDirection);
                gravBall.PhysicsBody.AngularVelocity = 2 * (float)Math.PI;

                return true;
            }

            return false;
        }

        /// <summary>
        /// This method provides the functionality for actually generating a bullet actor and sending it on its way. It does not enforce timing restraints.
        /// </summary>
        private void ShootBullet(Vector2 direction)
        {
            Actor bullet = actorManager.InstantiateTemplate("Bullet");

            bullet.PhysicsBody.Position = parentActor.PhysicsBody.Position;

            // Make sure the bullet doesn't collide with whatever actor fired it!
            bullet.PhysicsGeom.CollidesWith &= ~parentActor.PhysicsGeom.CollisionCategories;

            // Now send it on its way!

            bullet.PhysicsBody.LinearVelocity = direction * BulletVelocity + parentActor.PhysicsBody.LinearVelocity;
        }

        /// <summary>
        /// This shoots a bullet in the specified direction. The direction vector is assumed to be normalized.
        /// </summary>
        protected void ShootSingleBullet(Vector2 direction)
        {
            // Make sure the timer has elapsed so we can shoot.
            if (bulletShootTimer >= bulletShootInterval)
            {
                bulletShootTimer = TimeSpan.Zero;

                ShootBullet(direction);
            }
        }

        /// <summary>
        /// This shoots a three bullets in the general specified direction.
        /// </summary>
        protected void ShootBulletSpray(Vector2 direction)
        {
            // Make sure the timer has elapsed so we can shoot.
            if (bulletShootTimer >= bulletShootInterval)
            {
                bulletShootTimer = TimeSpan.Zero;

                Vector2 leftBulletDirection = Vector2.Transform(direction, Matrix.CreateRotationZ((float)Math.PI / 8.0f));
                Vector2 rightBulletDirection = Vector2.Transform(direction, Matrix.CreateRotationZ(-(float)Math.PI / 8.0f));

                ShootBullet(leftBulletDirection);
                ShootBullet(direction);
                ShootBullet(rightBulletDirection);
            }
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates the controller.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // Update the firing timers.

            gravityBallShootTimer += gameTime.ElapsedGameTime;
            bulletShootTimer += gameTime.ElapsedGameTime;
        }

        #endregion
    }
}
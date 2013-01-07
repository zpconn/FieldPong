#region Using statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Physics = FarseerGames.FarseerPhysics;
#endregion

namespace FieldPong
{
    /// <summary>
    /// This specifies how detailed the physics of the distortion should be.
    /// </summary>
    enum DistortGridDetail
    {
        /// <summary>
        /// Only the actor's linear velocity is taken into account.
        /// </summary>
        Low = 0,
        /// <summary>
        /// Here the spinning of the actor's physics body is also taken into account and will cause the grid
        /// to wrap around itself as well.
        /// </summary>
        High = 1
    }

    /// <summary>
    /// This behavior causes an actor's physics body to distort the grid as it passes over it.
    /// </summary>
    class DistortGridBehavior : ActorBehavior
    {
        #region Fields

        // We need a reference to the grid to distort.
        MassSpringGrid grid = null;

        // Some tuning constants
        private const float DisturbanceMagnitude = 80;
        private const float SpinMagnitude = 1200f;
        private const float MaxMagnitude = 380;
        private const float MaxTorque = 2000;

        DistortGridDetail detail;

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new instance of DistortGridBehavior.
        /// </summary>
        /// <param name="grid">The behavior needs a reference to the grid to distort.</param>
        public DistortGridBehavior(ActorManager actorManager, MassSpringGrid grid, DistortGridDetail detail)
            : base(actorManager)
        {
            this.detail = detail;
            this.grid = grid;
        }

        #endregion

        #region Update

        /// <summary>
        /// This looks at the motion of the actor's rigid body and distorts the grid accordingly. It also
        /// makes the grid "spin" about the center of the rigid body if the rigid body itself is spinning.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            if (parentActor.PhysicsBody.LinearVelocity.Length() > 0)
                grid.ApplyDisturbance(parentActor.PhysicsBody.Position,
                    Math.Min(DisturbanceMagnitude * parentActor.PhysicsBody.LinearVelocity.Length(), MaxMagnitude));

            if (detail == DistortGridDetail.High && Math.Abs(parentActor.PhysicsBody.Torque) > 0)
            {
                float torque = Math.Abs(parentActor.PhysicsBody.Torque);

                grid.TwistGrid(parentActor.PhysicsBody.Position, Math.Sign(parentActor.PhysicsBody.Torque),
                    Math.Min(SpinMagnitude * torque, MaxTorque));
            }
        }

        #endregion
    }
}
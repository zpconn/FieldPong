#region Using statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Physics = FarseerGames.FarseerPhysics;
#endregion

namespace FieldPong
{
    /// <summary>
    /// This behavior causes the actor to behave like a black hole, sucking in the grid with massive gravitational force.
    /// </summary>
    class BlackHoleBehavior : ActorBehavior
    {
        #region Fields

        // We must know which grid to distort.
        MassSpringGrid grid = null;

        // Just how powerful should the black hole be?
        const float GravityStrength = 15000;

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new instance of BlackHoleBehavior with the specific mass-spring grid.
        /// </summary>
        public BlackHoleBehavior(ActorManager actorManager, MassSpringGrid grid)
            : base(actorManager)
        {
            this.grid = grid;
        }

        #endregion

        #region Update

        /// <summary>
        /// This sucks in the grid about the actor with massive gravitational force.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            grid.TwistGrid(parentActor.PhysicsBody.Position, 1, GravityStrength);
        }

        #endregion
    }
}
#region Using statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Physics = FarseerGames.FarseerPhysics;
#endregion

namespace FieldPong
{
    /// <summary>
    /// This behavior creates a barrior of invisible rectangular rigid bodies that trap an actor within it.
    /// </summary>
    class ConstrainToRectangleBehavior : ActorBehavior
    {
        #region Helper Classes

        class CollisionBorders
        {
            public Physics.Collisions.Geom leftBorderGeom;
            public Physics.Collisions.Geom rightBorderGeom;
            public Physics.Collisions.Geom topBorderGeom;
            public Physics.Collisions.Geom bottomBorderGeom;

            public Physics.Dynamics.Body borderBody;
        }

        #endregion

        #region Fields

        Rectangle rectangle;
        Physics.Enums.CollisionCategories collisionCategory;

        static Dictionary<Rectangle, CollisionBorders> borderDictionary = new Dictionary<Rectangle, CollisionBorders>();

        bool assignedCollisionCategoriesYet = false;

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new instance of ConstrainToRectangleBehavior.
        /// </summary>
        /// <param name="rectangle">The rectangle within which the actor is to be trapped.</param>
        public ConstrainToRectangleBehavior(ActorManager actorManager, Rectangle rectangle,
                                            Physics.Enums.CollisionCategories collisionCategory)
            : base(actorManager)
        {
            this.rectangle = rectangle;
            this.collisionCategory = collisionCategory;
        }

        public override void Initialize()
        {
            if (!borderDictionary.ContainsKey(rectangle))
            {
                BuildBorders();
            }

            // We need to make the actor collide with the collision borders.
            parentActor.PhysicsGeom.CollidesWith |= collisionCategory;

            CollisionBorders borders = borderDictionary[rectangle];
            borders.leftBorderGeom.CollidesWith |= parentActor.PhysicsGeom.CollisionCategories;
            borders.topBorderGeom.CollidesWith |= parentActor.PhysicsGeom.CollisionCategories;
            borders.rightBorderGeom.CollidesWith |= parentActor.PhysicsGeom.CollisionCategories;
            borders.bottomBorderGeom.CollidesWith |= parentActor.PhysicsGeom.CollisionCategories;

            assignedCollisionCategoriesYet = true;
        }

        /// <summary>
        /// This constructs rectangular rigid bodies around the edges of the rectangle.
        /// </summary>
        private void BuildBorders()
        {
            CollisionBorders borders = new CollisionBorders();

            Physics.PhysicsSimulator physicsSimulator =
                (Physics.PhysicsSimulator)this.ParentActor.Game.Services.GetService(typeof(Physics.PhysicsSimulator));

            borders.borderBody = Physics.Dynamics.BodyFactory.Instance.CreateRectangleBody(
                rectangle.Width, rectangle.Height, 1.0f);
            borders.borderBody.IsStatic = true;
            borders.borderBody.Position = new Vector2(rectangle.Left + rectangle.Width / 2, rectangle.Top + rectangle.Height / 2);

            Vector2 geometryOffset = new Vector2(-rectangle.Width / 2 - 50f, 0.0f);
            borders.leftBorderGeom = Physics.Collisions.GeomFactory.Instance.CreateRectangleGeom(
                borders.borderBody, 100.0f, rectangle.Height, geometryOffset, 0.0f);
            borders.leftBorderGeom.RestitutionCoefficient = 1.0f;
            borders.leftBorderGeom.CollisionCategories = collisionCategory;

            geometryOffset = new Vector2(rectangle.Width / 2 + 50f, 0.0f);
            borders.rightBorderGeom = Physics.Collisions.GeomFactory.Instance.CreateGeom(
                borders.borderBody, borders.leftBorderGeom, geometryOffset, 0.0f);

            geometryOffset = new Vector2(0.0f, -rectangle.Height / 2 - 50f);
            borders.topBorderGeom = Physics.Collisions.GeomFactory.Instance.CreateRectangleGeom(
                borders.borderBody, rectangle.Width, 100.0f, geometryOffset, 0.0f);
            borders.topBorderGeom.RestitutionCoefficient = 1.0f;
            borders.topBorderGeom.CollisionCategories = collisionCategory;

            geometryOffset = new Vector2(0.0f, rectangle.Height / 2 + 50f);
            borders.bottomBorderGeom = Physics.Collisions.GeomFactory.Instance.CreateGeom(
                borders.borderBody, borders.topBorderGeom, geometryOffset, 0.0f);

            borders.leftBorderGeom.CollidesWith = Physics.Enums.CollisionCategories.None;
            borders.topBorderGeom.CollidesWith = Physics.Enums.CollisionCategories.None;
            borders.rightBorderGeom.CollidesWith = Physics.Enums.CollisionCategories.None;
            borders.bottomBorderGeom.CollidesWith = Physics.Enums.CollisionCategories.None;

            physicsSimulator.Add(borders.borderBody);
            physicsSimulator.Add(borders.leftBorderGeom);
            physicsSimulator.Add(borders.rightBorderGeom);
            physicsSimulator.Add(borders.topBorderGeom);
            physicsSimulator.Add(borders.bottomBorderGeom);

            borderDictionary.Add(rectangle, borders);
        }

        #endregion

        #region Update

        /// <summary>
        /// This just updates the behavior. In this case, however, there's nothing to update.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // Nothing to do!
        }

        #endregion
    }
}
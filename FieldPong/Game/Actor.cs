#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Physics = FarseerGames.FarseerPhysics;
#endregion

namespace FieldPong
{
    /// <summary>
    /// Encapsulates a behavior or property that an actor performs or exhibits.
    /// </summary>
    class ActorBehavior : ICloneable
    {
        #region Fields

        protected ActorManager actorManager = null;

        /// <summary>
        /// The actor that owns this behavior.
        /// </summary>
        protected Actor parentActor = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the actor that owns this behavior.
        /// </summary>
        public Actor ParentActor
        {
            get { return parentActor; }
            set { parentActor = value; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new ActorBehavior.
        /// </summary>
        public ActorBehavior(ActorManager actorManager)
        {
            this.actorManager = actorManager;
        }

        /// <summary>
        /// This method gives the behavior a chance to initialize itself after such things
        /// as the physics geometry have been set up.
        /// </summary>
        public virtual void Initialize()
        {
        }

        #endregion

        #region Update

        public virtual void Update(GameTime gameTime) { }

        #endregion

        #region Clone

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }

    /// <summary>
    /// Represents an active, physical object in the game that can interact with other objects in the game world.
    /// Actors are represented using an aggregation model as opposed to the more common inheritance model.
    /// An actor is defined by its set of ActorBehaviors. In order to keep the identity of an actor constant, therefore,
    /// the list of ActorBehaviors must not change after the time of construction. Thus, the class interface strictly
    /// prohibits any such changes from occurring.
    /// 
    /// Each actor is automatically endowed with a sprite and a physics object. The behavior of the actor, however, must
    /// be implemented in the ActorBehaviors.
    /// </summary>
    class Actor
    {
        #region Fields

        Game game = null;

        List<ActorBehavior> behaviors = null;
        bool alive = true;

        Sprite sprite = null;

        Physics.Dynamics.Body physicsBody = null;
        Physics.Collisions.Geom physicsGeom = null;

        float alpha = 255;

        #endregion

        #region Properties

        /// <summary>
        /// Gets whether this actor is alive or not. To kill an actor, use the Kill() method.
        /// </summary>
        public bool Alive
        {
            get { return alive; }
        }

        /// <summary>
        /// Gets or sets the actor's sprite, its visual representation.
        /// </summary>
        public Sprite Sprite
        {
            get { return sprite; }
            set { sprite = value; }
        }

        /// <summary>
        /// Gets or sets the alpha value used for rendering the sprite, representing the opaqueness of the on-screen sprite.
        /// </summary>
        public float Alpha
        {
            get { return alpha; }
            set
            {
                alpha = Math.Min(Math.Max(value, 0), 255.0f);
            }
        }

        /// <summary>
        /// Gets or sets the physics body for this actor.
        /// </summary>
        public Physics.Dynamics.Body PhysicsBody
        {
            get { return physicsBody; }
            set { physicsBody = value; }
        }

        /// <summary>
        /// Gets or sets the physics geometry for this actor.
        /// </summary>
        public Physics.Collisions.Geom PhysicsGeom
        {
            get { return physicsGeom; }
            set { physicsGeom = value; }
        }

        /// <summary>
        /// Gets a reference to the main Game class.
        /// </summary>
        public Game Game
        {
            get { return game; }
        }

        /// <summary>
        /// Exposes the ActorBehaviors of this actors in an array so external code can examine and interact with 
        /// the behaviors that this actor supports.
        /// </summary>
        public ActorBehavior[] Behaviors
        {
            get
            {
                if (behaviors == null)
                    return null;

                return behaviors.ToArray();
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new Actor. All ActorBehaviors must be provided here at once.
        /// </summary>
        public Actor(Game game, ActorBehavior[] newBehaviors)
        {
            this.game = game;
            behaviors = new List<ActorBehavior>(newBehaviors);
        }

        /// <summary>
        /// Creates a new actor with a static sprite using the provided texture.
        /// </summary>
        public Actor(Game game, ActorBehavior[] newBehaviors, ContentManager content, string textureName)
        {
            this.game = game;
            behaviors = new List<ActorBehavior>(newBehaviors);

            sprite = Sprite.CreateStatic(content, textureName);
        }

        /// <summary>
        /// Creates a new actor with a static sprite using the provided texture, width, and height.
        /// </summary>
        public Actor(Game game, ActorBehavior[] newBehaviors, ContentManager content, string textureName,
                     float width, float height)
        {
            this.game = game;
            behaviors = new List<ActorBehavior>(newBehaviors);

            sprite = Sprite.CreateStatic(content, textureName, width, height);
        }

        #endregion

        #region Update

        /// <summary>
        /// Executes each behavior this actor supports and then draws the actor's sprite with the transformation
        /// provided by its physics body.
        /// </summary>
        public void Update(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // First update all the behaviors.
            foreach (ActorBehavior behavior in behaviors)
            {
                behavior.Update(gameTime);
            }

            Vector2 origin = new Vector2(sprite.SourceWidth / 2, sprite.SourceHeight / 2);

            // Now draw the sprite.
            sprite.Update(gameTime);
            sprite.Draw(spriteBatch, physicsBody.Position, physicsBody.Rotation, 1.0f,
                new Color(255, 255, 255, (byte)alpha), origin);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Searches this actor's behavior list for a certain behavior. If no such behavior is found, null is returned.
        /// </summary>
        public ActorBehavior GetBehavior(Type behaviorType)
        {
            // First make sure the passed in type is derived from ActorBehavior
            if (!behaviorType.IsSubclassOf(typeof(ActorBehavior)))
                throw new ArgumentException("The first parameter of Actor.GetBehavior() must be a type that derives from " +
                                            "ActorBehavior.", "behaviorType");

            // Now search the behavior list for a behavior of the given type
            foreach (ActorBehavior behavior in behaviors)
            {
                if (behavior.GetType() == behaviorType)
                    return behavior;
            }

            // No valid behavior found.
            return null;
        }

        /// <summary>
        /// Produces an exact clone of this actor.
        /// </summary>
        public Actor Clone()
        {
            // First we must clone all the behaviors.

            ActorBehavior[] behaviorClones = new ActorBehavior[behaviors.Count];

            for (int i = 0; i < behaviors.Count; ++i)
            {
                ActorBehavior behavior = behaviors[i];
                behaviorClones[i] = (ActorBehavior)behavior.Clone();
            }

            Actor clone = new Actor(game, behaviorClones);

            clone.Sprite = sprite;

            clone.PhysicsBody = Physics.Dynamics.BodyFactory.Instance.CreateBody(physicsBody);
            clone.physicsGeom = Physics.Collisions.GeomFactory.Instance.CreateGeom(clone.PhysicsBody, physicsGeom);

            // We need to add the cloned physics body and geometry to the simulator.
            Physics.PhysicsSimulator physicsSimulator =
                (Physics.PhysicsSimulator)game.Services.GetService(typeof(Physics.PhysicsSimulator));
            physicsSimulator.Add(clone.PhysicsBody);
            physicsSimulator.Add(clone.PhysicsGeom);

            // We need to loop through all the behaviors and assign the cloned actor as their parent.
            foreach (ActorBehavior behavior in clone.Behaviors)
            {
                behavior.ParentActor = clone;
                behavior.Initialize();
            }

            return clone;
        }

        /// <summary>
        /// This method kills the actor and frees certain resources it used.
        /// </summary>
        public void Kill()
        {
            // Remove the physics body and geometry from the simulator.

            Physics.PhysicsSimulator physicsSimulator =
                (Physics.PhysicsSimulator)game.Services.GetService(typeof(Physics.PhysicsSimulator));

            physicsSimulator.Remove(physicsBody);
            physicsSimulator.Remove(physicsGeom);

            alive = false;
        }

        #endregion
    }
}
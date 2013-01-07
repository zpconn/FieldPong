#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Physics = FarseerGames.FarseerPhysics;
#endregion

namespace FieldPong
{
    /// <summary>
    /// This screen implements the actual game logic.
    /// </summary>
    class GameplayScreen : GameScreen
    {
        #region Fields

        ContentManager content;
        SpriteBatch spriteBatch;
        LineBatch lineBatch;
        Starfield starfield;
        Texture2D starTexture;
        MassSpringGrid grid;

        ActorManager actorManager;

        bool isPaused = false;

        // These are the key objects in the game.
        Actor playerPaddle;
        Actor computerPaddle;
        Actor ball;

        // Lightning boxes function as little cages to keep the paddles in so that they can't go everywhere in the arena.
        LightningBox playerLightningBox;
        LightningBox computerLightningBox;

        // These track the countdown timer that runs between rounds.
        TimeSpan countdownTimeSpan = TimeSpan.FromSeconds(2.0f);
        TimeSpan countdownTimer = TimeSpan.Zero;

        Sprite countdownSprite;

        // Are we transitioning between rounds, waiting for the countdown timer?
        bool inRoundTransit = true;

        // Some statistics for the player:

        int lives = 3;
        int level = 1;

        // All this safe area business is necessary to make sure the game fits on a standard TV screen
        // when it's run on the Xbox 360.

        int screenWidth = 1024;
        int screenHeight = 768;

        int screenSafeWidth;
        int screenSafeHeight;

        int safeAreaWidth;
        int safeAreaHeight;

        #endregion

        #region Initialization

        /// <summary>
        /// Constructs a new gameplay screen
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.0);
            TransitionOffTime = TimeSpan.FromSeconds(1.0);

            // WINDOWS CODE:

            safeAreaWidth = 0;
            safeAreaHeight = 0;

            screenSafeWidth = screenWidth;
            screenSafeHeight = screenHeight;

            // XBOX CODE:

            //safeAreaWidth = (int)(0.05f * (float)screenWidth);
            //safeAreaHeight = (int)(0.05f * (float)screenHeight);

            //screenSafeWidth = (int)((float)screenWidth * 0.9f);
            //screenSafeHeight = (int)((float)screenHeight * 0.9f);
        }

        /// <summary>
        /// Initializes the game after the ScreenManager is set.
        /// </summary>
        public void Initialize()
        {
            starfield = new Starfield(2048, new Rectangle(-512, -512, screenWidth + 512, screenHeight + 512));
        }

        /// <summary>
        /// Loads graphics content needed for the game.
        /// </summary>
        public override void LoadGraphicsContent(bool loadAllContent)
        {
            if (loadAllContent)
            {
                if (content == null)
                    content = new ContentManager(ScreenManager.Game.Services, "Content");

                grid = new MassSpringGrid(60, 5f, 100f, 0.5f,
                    new Rectangle(safeAreaWidth, safeAreaHeight, screenSafeWidth, screenSafeHeight));

                actorManager = new ActorManager();

                #region Allocate graphics content

                spriteBatch = new SpriteBatch(ScreenManager.GraphicsDevice);
                lineBatch = new LineBatch(ScreenManager.GraphicsDevice);
                starTexture = content.Load<Texture2D>("blank");

                Animation countdownAnim = new Animation(content, 300, 100);
                countdownAnim.AddFrame(new AnimationFrame(TimeSpan.FromSeconds(2.0f / 3.0f), "ready"));
                countdownAnim.AddFrame(new AnimationFrame(TimeSpan.FromSeconds(2.0f / 3.0f), "set"));
                countdownAnim.AddFrame(new AnimationFrame(TimeSpan.FromSeconds(2.0f / 3.0f), "go"));

                countdownSprite = new Sprite();
                countdownSprite.AddAnimation("default", countdownAnim);
                countdownSprite.SetCurrentAnimation("default");

                #endregion

                #region Set up lightning boxes

                playerLightningBox = new LightningBox(new Rectangle(safeAreaWidth + 10, screenHeight - safeAreaHeight - 38 - 150, screenSafeWidth - 30, 150));
                computerLightningBox = new LightningBox(new Rectangle(safeAreaWidth + 10, safeAreaHeight + 38, screenSafeWidth - 30, 150));

                #endregion

                #region Collision Categories for physics

                // Collision Categories:
                //
                // 1 = player paddle
                // 10 = ball
                // 11 = gravity ball
                // 12 = obstacle
                // 13 = computer paddle
                // 14 = bullet
                // 20 = screen borders
                // 21 = player lightning box
                // 22 = borders for obstacles
                // 23 = computer lightning box

                #endregion

                #region Create Player Paddle Actor Template

                Actor playerPaddleTemplate = new Actor(ScreenManager.Game,
                    new ActorBehavior[] { new GamePadPaddleController(actorManager),
                                          new DistortGridBehavior(actorManager, grid, DistortGridDetail.High),
                                          new ConstrainToRectangleBehavior(actorManager, playerLightningBox.Rectangle,
                                                                           Physics.Enums.CollisionCategories.Cat21) },
                    content, "paddle", 100, 100 * 0.174089069f);

                playerPaddleTemplate.PhysicsBody =
                    Physics.Dynamics.BodyFactory.Instance.CreateRectangleBody(playerPaddleTemplate.Sprite.Width,
                                                                              playerPaddleTemplate.Sprite.Height, 1);

                playerPaddleTemplate.PhysicsGeom =
                    Physics.Collisions.GeomFactory.Instance.CreateRectangleGeom(playerPaddleTemplate.PhysicsBody,
                    playerPaddleTemplate.Sprite.Width, playerPaddleTemplate.Sprite.Height);

                playerPaddleTemplate.PhysicsGeom.CollisionCategories = Physics.Enums.CollisionCategories.Cat1;
                playerPaddleTemplate.PhysicsGeom.CollidesWith = Physics.Enums.CollisionCategories.Cat1 |
                                                          Physics.Enums.CollisionCategories.Cat10 |
                                                          Physics.Enums.CollisionCategories.Cat11;

                playerPaddleTemplate.PhysicsBody.LinearDragCoefficient = 0.01f;
                playerPaddleTemplate.PhysicsBody.RotationalDragCoefficient = 300.0f;

                actorManager.AddActorTemplate(playerPaddleTemplate, "Player Paddle");

                #endregion

                #region Create Computer Paddle Actor Template

                Actor computerPaddleTemplate = new Actor(ScreenManager.Game,
                    new ActorBehavior[] { new DistortGridBehavior(actorManager, grid, DistortGridDetail.High),
                                          new ConstrainToRectangleBehavior(actorManager, computerLightningBox.Rectangle, 
                                                                           Physics.Enums.CollisionCategories.Cat23),
                                          new AIPaddleController(actorManager) },
                    content, "paddle2", 100, 100 * 0.174089069f);

                computerPaddleTemplate.PhysicsBody =
                    Physics.Dynamics.BodyFactory.Instance.CreateRectangleBody(playerPaddleTemplate.Sprite.Width,
                                                                              playerPaddleTemplate.Sprite.Height, 1);

                computerPaddleTemplate.PhysicsGeom =
                    Physics.Collisions.GeomFactory.Instance.CreateRectangleGeom(playerPaddleTemplate.PhysicsBody,
                    playerPaddleTemplate.Sprite.Width, playerPaddleTemplate.Sprite.Height);

                computerPaddleTemplate.PhysicsGeom.CollisionCategories = Physics.Enums.CollisionCategories.Cat13;
                computerPaddleTemplate.PhysicsGeom.CollidesWith = Physics.Enums.CollisionCategories.Cat13 |
                    Physics.Enums.CollisionCategories.Cat10 | Physics.Enums.CollisionCategories.Cat11;

                computerPaddleTemplate.PhysicsBody.LinearDragCoefficient = 0.01f;
                computerPaddleTemplate.PhysicsBody.RotationalDragCoefficient = 300.0f;

                actorManager.AddActorTemplate(computerPaddleTemplate, "Computer Paddle");

                #endregion

                #region Create Obstacle Actor Template

                Actor obstacleActorTemplate = new Actor(ScreenManager.Game,
                    new ActorBehavior[] { new DistortGridBehavior(actorManager, grid, DistortGridDetail.High),
                                          new ConstrainToRectangleBehavior(actorManager, new Rectangle(
                        computerLightningBox.Rectangle.X, 
                        computerLightningBox.Rectangle.Y + computerLightningBox.Rectangle.Height, 
                        computerLightningBox.Rectangle.Width, 
                        playerLightningBox.Rectangle.Y - (computerLightningBox.Rectangle.Y + computerLightningBox.Rectangle.Height)),
                                                                           Physics.Enums.CollisionCategories.Cat22),
                                          new ShootOffRandomlyBehavior(actorManager, 300, 350),
                                          new InteractWithBlackHolesBehavior(actorManager) },
                    content, "obstacle", 45.0f, 45.0f);

                obstacleActorTemplate.PhysicsBody =
                    Physics.Dynamics.BodyFactory.Instance.CreateRectangleBody(obstacleActorTemplate.Sprite.Width,
                                                                              obstacleActorTemplate.Sprite.Height, 0.001f);

                obstacleActorTemplate.PhysicsGeom =
                    Physics.Collisions.GeomFactory.Instance.CreateRectangleGeom(obstacleActorTemplate.PhysicsBody,
                    obstacleActorTemplate.Sprite.Width, obstacleActorTemplate.Sprite.Height);

                obstacleActorTemplate.PhysicsGeom.CollisionCategories = Physics.Enums.CollisionCategories.Cat12;
                obstacleActorTemplate.PhysicsGeom.CollidesWith = Physics.Enums.CollisionCategories.Cat1 |
                    Physics.Enums.CollisionCategories.Cat10 | Physics.Enums.CollisionCategories.Cat11 |
                    Physics.Enums.CollisionCategories.Cat12;

                obstacleActorTemplate.PhysicsBody.LinearDragCoefficient = 0.0f;

                actorManager.AddActorTemplate(obstacleActorTemplate, "Obstacle");

                #endregion

                #region Create Ball Actor Template

                float ballDiameter = 25.0f;

                Actor ballTemplate = new Actor(ScreenManager.Game,
                    new ActorBehavior[] { new DistortGridBehavior(actorManager, grid, DistortGridDetail.High),
                                          new ConstrainToRectangleBehavior(actorManager, new Rectangle(safeAreaWidth, safeAreaHeight, screenSafeWidth, screenSafeHeight),
                                                                           Physics.Enums.CollisionCategories.Cat20),
                                          new ShootOffRandomlyBehavior(actorManager, 300, 301.0f),
                                          new InteractWithBlackHolesBehavior(actorManager) },
                    content, "ball", ballDiameter, ballDiameter);

                ballTemplate.PhysicsBody =
                    Physics.Dynamics.BodyFactory.Instance.CreateCircleBody(ballDiameter / 2.0f, 0.001f);

                ballTemplate.PhysicsGeom =
                    Physics.Collisions.GeomFactory.Instance.CreateCircleGeom(ballTemplate.PhysicsBody, ballDiameter / 2.0f, 16);

                ballTemplate.PhysicsBody.LinearDragCoefficient = 0.0f;

                ballTemplate.PhysicsGeom.CollisionCategories = Physics.Enums.CollisionCategories.Cat10;
                ballTemplate.PhysicsGeom.CollidesWith = Physics.Enums.CollisionCategories.Cat1 |
                                                        Physics.Enums.CollisionCategories.Cat10 |
                                                        Physics.Enums.CollisionCategories.Cat13;

                actorManager.AddActorTemplate(ballTemplate, "Ball");

                #endregion

                #region Create Gravity Ball Actor Template

                float gravityBallDiameter = 55.0f;

                Actor gravBallTemplate = new Actor(ScreenManager.Game,
                    new ActorBehavior[] { new BlackHoleBehavior(actorManager, grid),
                                          new ConstrainToRectangleBehavior(actorManager, new Rectangle(safeAreaWidth, safeAreaHeight, screenSafeWidth, screenSafeHeight),
                                                                           Physics.Enums.CollisionCategories.Cat20),
                                          new LiveTemporarilyBehavior(actorManager, TimeSpan.FromSeconds(2.9f), TimeSpan.FromSeconds(0.5)) },
                    content, "GravityBall", gravityBallDiameter, 2.0f);

                gravBallTemplate.PhysicsBody =
                    Physics.Dynamics.BodyFactory.Instance.CreateCircleBody(gravityBallDiameter / 2.0f, 3.0f);

                gravBallTemplate.PhysicsGeom =
                    Physics.Collisions.GeomFactory.Instance.CreateCircleGeom(gravBallTemplate.PhysicsBody,
                    gravityBallDiameter / 2.0f, 16);

                gravBallTemplate.PhysicsBody.LinearDragCoefficient = 0.0f;

                gravBallTemplate.PhysicsGeom.CollisionCategories = Physics.Enums.CollisionCategories.Cat11;
                gravBallTemplate.PhysicsGeom.CollidesWith = Physics.Enums.CollisionCategories.Cat11 |
                    Physics.Enums.CollisionCategories.Cat10 | Physics.Enums.CollisionCategories.Cat1 |
                    Physics.Enums.CollisionCategories.Cat13;

                actorManager.AddActorTemplate(gravBallTemplate, "Gravity Ball");

                #endregion

                #region Create Bullet Actor Template

                float bulletDiameter = 5.0f;

                Actor bulletTemplate = new Actor(ScreenManager.Game,
                    new ActorBehavior[] { new ConstrainToRectangleBehavior(actorManager, new Rectangle(safeAreaWidth, safeAreaHeight, screenSafeWidth, screenSafeHeight),
                                                                           Physics.Enums.CollisionCategories.Cat20),
                                          new DieUponCollisionBehavior(actorManager),
                                          new InteractWithBlackHolesBehavior(actorManager) },
                    content, "projectile", bulletDiameter, bulletDiameter);

                bulletTemplate.PhysicsBody =
                    Physics.Dynamics.BodyFactory.Instance.CreateCircleBody(bulletDiameter / 2.0f, 0.001f);

                bulletTemplate.PhysicsGeom =
                    Physics.Collisions.GeomFactory.Instance.CreateCircleGeom(bulletTemplate.PhysicsBody, bulletDiameter / 2.0f, 16);

                bulletTemplate.PhysicsBody.LinearDragCoefficient = 0.0f;

                bulletTemplate.PhysicsGeom.CollisionCategories = Physics.Enums.CollisionCategories.Cat14;
                bulletTemplate.PhysicsGeom.CollidesWith = Physics.Enums.CollisionCategories.Cat1 |
                    Physics.Enums.CollisionCategories.Cat10 | Physics.Enums.CollisionCategories.Cat11 |
                    Physics.Enums.CollisionCategories.Cat12 | Physics.Enums.CollisionCategories.Cat13;

                actorManager.AddActorTemplate(bulletTemplate, "Bullet");

                #endregion

                #region Place basic gameplay objects

                playerPaddle = actorManager.InstantiateTemplate("Player Paddle");
                playerPaddle.PhysicsBody.Position = new Vector2(safeAreaWidth + screenSafeWidth / 2.0f, screenHeight - safeAreaHeight - 38 - 150 / 2);

                computerPaddle = actorManager.InstantiateTemplate("Computer Paddle");
                computerPaddle.PhysicsBody.Position = new Vector2(safeAreaWidth + screenSafeWidth / 2.0f, safeAreaHeight + 38 + 150 / 2);

                ball = actorManager.InstantiateTemplate("Ball");
                ball.PhysicsBody.Position = new Vector2(safeAreaWidth + screenSafeWidth / 2, safeAreaHeight + screenSafeHeight / 2);
                PauseBallForNewRound();

                ShootOffRandomlyBehavior shootOffBehavior = (ShootOffRandomlyBehavior)ball.GetBehavior(typeof(ShootOffRandomlyBehavior));
                shootOffBehavior.JustStarting = false;

                AIPaddleController ai = (AIPaddleController)computerPaddle.GetBehavior(typeof(AIPaddleController));
                ai.Ball = ball;

                PopulateArenaWithObstacles();

                #endregion
            }

            // Set the projection matrix for the line batch
            lineBatch.SetProjection(Matrix.CreateOrthographicOffCenter(
                0.0f, ScreenManager.GraphicsDevice.Viewport.Width,
                ScreenManager.GraphicsDevice.Viewport.Height, 0.0f, 0.0f, 1.0f));
        }

        /// <summary>
        /// Frees graphics content
        /// </summary>
        public override void UnloadGraphicsContent(bool unloadAllContent)
        {
            if (unloadAllContent)
            {
                if (spriteBatch != null)
                {
                    spriteBatch.Dispose();
                    spriteBatch = null;
                }

                if (lineBatch != null)
                {
                    lineBatch.Dispose();
                    lineBatch = null;
                }

                content.Unload();
                actorManager.Unload();
            }
        }

        /// <summary>
        /// This places obstacles around the arena.
        /// </summary>
        private void PopulateArenaWithObstacles()
        {
            Actor obstacle1 = actorManager.InstantiateTemplate("Obstacle");
            obstacle1.PhysicsBody.Position = new Vector2(safeAreaWidth + screenSafeWidth / 4, safeAreaHeight + screenSafeHeight / 2);

            Actor obstacle2 = actorManager.InstantiateTemplate("Obstacle");
            obstacle2.PhysicsBody.Position = new Vector2(safeAreaWidth + screenSafeWidth / 2, safeAreaHeight + screenSafeHeight / 4 + 60);

            Actor obstacle3 = actorManager.InstantiateTemplate("Obstacle");
            obstacle3.PhysicsBody.Position = new Vector2(safeAreaWidth + screenSafeWidth / 2, safeAreaHeight + screenSafeHeight * 3 / 4 - 60);

            Actor obstacle4 = actorManager.InstantiateTemplate("Obstacle");
            obstacle4.PhysicsBody.Position = new Vector2(safeAreaWidth + screenSafeWidth * 3 / 4, safeAreaHeight + screenSafeHeight / 2);
        }

        #endregion

        #region Update and Draw

        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive property, so the game
        /// will stop updating when the pause menu is active or if the player tabs away to a different
        /// application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Don't do anything if the game is paused.
            if (isPaused)
                return;

            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Random random = new Random();

            if (gameTime.TotalGameTime.Seconds % 5 == 0)
                starfield.SetTargetPosition(new Vector2(
                    (float)random.NextDouble() * (float)screenWidth,
                    (float)random.NextDouble() * (float)screenHeight));

            starfield.Update(elapsedTime);

            grid.Update(elapsedTime);

            Physics.PhysicsSimulator physicsSimulator =
                (Physics.PhysicsSimulator)ScreenManager.Game.Services.GetService(typeof(Physics.PhysicsSimulator));
            physicsSimulator.Update(gameTime.ElapsedGameTime.Milliseconds * 0.001f);

            // Check if the ball has passed either of the paddles.

            if (ball.PhysicsBody.Position.Y < computerLightningBox.Rectangle.Y)
            {
                // The player scored.
                level++;

                // For every two levels gained, we give the player one extra life.

                if ((level - 1) % 2 == 0)
                    lives++;

                // Let's update the AI's knowledge of the player's level.

                AIPaddleController ai = (AIPaddleController)computerPaddle.GetBehavior(typeof(AIPaddleController));
                ai.Level = level;

                GamePadPaddleController gamePad = (GamePadPaddleController)playerPaddle.GetBehavior(typeof(GamePadPaddleController));
                gamePad.Level = level;

                inRoundTransit = true;
                PauseBallForNewRound();
                countdownSprite.CurrentAnimation.Reset();
            }
            else if (ball.PhysicsBody.Position.Y > playerLightningBox.Rectangle.Y + playerLightningBox.Rectangle.Height)
            {
                // The computer scored.
                lives--;

                inRoundTransit = true;
                PauseBallForNewRound();
                countdownSprite.CurrentAnimation.Reset();
            }

            // Check if the player has lost all his lives

            if (lives <= 0)
            {
                // Transition to the game over screen.
                isPaused = true;
                LoadingScreen.Load(ScreenManager, LoadGameOverScreen);
            }

            // If we're transitioning between rounds, make the countdown timer run.

            if (inRoundTransit)
            {
                countdownSprite.Update(gameTime);

                countdownTimer += gameTime.ElapsedGameTime;

                if (countdownTimer >= countdownTimeSpan)
                {
                    inRoundTransit = false;
                    countdownTimer = TimeSpan.Zero;

                    ShootBallOff();
                }
            }
        }

        /// <summary>
        /// This loads the main menu screen.
        /// </summary>
        void LoadGameOverScreen(object sender, EventArgs e)
        {
            ScreenManager.AddScreen(new GameOverScreen(level));
        }

        /// <summary>
        /// This places the ball back in its initial position, and makes it stick there while the timer is
        /// counting down for the next round.
        /// </summary>
        private void PauseBallForNewRound()
        {
            ball.PhysicsBody.Position = new Vector2(safeAreaWidth + screenSafeWidth / 2, safeAreaHeight + screenSafeHeight / 2);
            ball.PhysicsBody.IsStatic = true;
        }

        /// <summary>
        /// This shoots the ball off again after the countdown timer has expired for the next round.
        /// </summary>
        private void ShootBallOff()
        {
            ball.PhysicsBody.IsStatic = false;

            ShootOffRandomlyBehavior shootOffBehavior = (ShootOffRandomlyBehavior)ball.GetBehavior(typeof(ShootOffRandomlyBehavior));
            shootOffBehavior.JustStarting = true;
        }

        /// <summary>
        /// Lets the game respond to player input.
        /// </summary>
        public override void HandleInput()
        {
        }

        /// <summary>
        /// Draws the game.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            spriteBatch.Begin();
            starfield.Draw(spriteBatch, starTexture);
            spriteBatch.End();

            lineBatch.Begin();
            grid.Draw(lineBatch, new Color(50, 50, 255, 20), new Color(50, 50, 255, 240));
            playerLightningBox.Draw(lineBatch, Color.Red, 25, 5.0f);
            computerLightningBox.Draw(lineBatch, Color.Yellow, 25, 5.0f);
            lineBatch.End();

            actorManager.Update(gameTime, spriteBatch);

            // If we're transitioning between rounds, draw the countdown animation.

            if (inRoundTransit)
            {
                countdownSprite.Draw(spriteBatch, new Vector2(safeAreaWidth + screenSafeWidth / 2, safeAreaHeight + screenSafeHeight / 2 - 100),
                                     0.0f, 1.0f, new Color(255, 255, 255, 255), new Vector2(150, 50));
            }

            // Finally draw the player's statistics on the screen.

            spriteBatch.Begin();

            // Make the text vertically centered.
            Vector2 origin = new Vector2(0, ScreenManager.Font.LineSpacing / 2);

            spriteBatch.DrawString(ScreenManager.Font, "Level: " + level.ToString(),
                new Vector2(safeAreaWidth + 30, safeAreaHeight + 5), Color.Green);
            spriteBatch.DrawString(ScreenManager.Font, "Lives: " + lives.ToString(),
                new Vector2(safeAreaWidth + screenSafeWidth - 160, safeAreaHeight + 5), Color.Green);

            spriteBatch.End();

            // If the game is transitioning on or off, fade it out to black
            if (TransitionPosition > 0)
                ScreenManager.FadeBackBufferToBlack(ScreenManager.SpriteBatch, 255 - TransitionAlpha);
        }

        #endregion
    }
}
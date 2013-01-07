#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using FarseerGames.FarseerPhysics;
#endregion

namespace FieldPong
{
    /// <summary>
    /// This class just glues together the various components of the game. The exciting stuff is in all
    /// the game screens.
    /// </summary>
    public class FieldPong : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;
        ContentManager content;
        FrameRateCounter fpsCounter;
        BloomPostProcessor bloomProcessor;
        ScreenManager screenManager;
        PhysicsSimulator physicsSimulator;

        AudioEngine audioEngine;
        WaveBank waveBank;
        SoundBank soundBank;

        Cue backgroundMusic = null;

        #endregion

        #region Initialization

        public FieldPong()
        {
            graphics = new GraphicsDeviceManager(this);
            content = new ContentManager(Services);

            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            graphics.MinimumPixelShaderProfile = ShaderProfile.PS_2_0;
            graphics.SynchronizeWithVerticalRetrace = true;

            physicsSimulator = new PhysicsSimulator(new Vector2(0));
            physicsSimulator.AllowedPenetration = 0.3f;
            physicsSimulator.BiasFactor = 1.0f;
            Services.AddService(typeof(PhysicsSimulator), physicsSimulator);

            screenManager = new ScreenManager(this);
            Components.Add(screenManager);

            bloomProcessor = new BloomPostProcessor(this);
            Components.Add(bloomProcessor);

            // Uncomment this to monitor the FPS:

            //fpsCounter = new FrameRateCounter(this);
            //Components.Add(fpsCounter);

            audioEngine = new AudioEngine("Content\\Audio\\FieldPongAudio.xgs");
            waveBank = new WaveBank(audioEngine, "Content\\Audio\\Wave Bank.xwb");
            soundBank = new SoundBank(audioEngine, "Content\\Audio\\Sound Bank.xsb");

            backgroundMusic = soundBank.GetCue("GameSong0010 16 Bit");
            backgroundMusic.Play();
        }


        /// <summary>
        /// Overriden from Game.Initialize(). Sets up the ScreenManager.
        /// </summary>
        protected override void Initialize()
        {
            graphics.ToggleFullScreen();
            base.Initialize();

            screenManager.AddScreen(new MainMenuScreen());
        }

        /// <summary>
        /// Unload your graphics content.  If unloadAllContent is true, you should
        /// unload content from both ResourceManagementMode pools.  Otherwise, just
        /// unload ResourceManagementMode.Manual content.  Manual content will get
        /// Disposed by the GraphicsDevice during a Reset.
        /// </summary>
        /// <param name="unloadAllContent">Which type of content to unload.</param>
        protected override void UnloadGraphicsContent(bool unloadAllContent)
        {
            if (unloadAllContent)
            {
                content.Unload();
            }
        }

        #endregion

        #region Update and Draw

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            // All the real action occurs in the screen manager

            base.Draw(gameTime);
        }

        #endregion
    }
}

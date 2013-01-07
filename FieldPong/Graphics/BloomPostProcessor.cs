#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace FieldPong
{
    /// <summary>
    /// A game component that applies bloom lighting (neon glow) to the scene.
    /// </summary>
    class BloomPostProcessor : DrawableGameComponent
    {
        #region Fields

        ContentManager content;
        SpriteBatch spriteBatch;

        Effect bloomExtractEffect;
        Effect bloomCombineEffect;
        Effect gaussianBlurEffect;

        ResolveTexture2D resolveTarget;
        RenderTarget2D renderTarget1;
        RenderTarget2D renderTarget2;

        BloomSettings settings = BloomSettings.Default();

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new BloomPostProcessor.
        /// </summary>
        public BloomPostProcessor(Game game)
            : base(game)
        {
            if (game == null)
                throw new ArgumentNullException("game");

            content = new ContentManager(game.Services);
        }

        /// <summary>
        /// Loads the graphics content and effects.
        /// </summary>
        protected override void LoadGraphicsContent(bool loadAllContent)
        {
            if (loadAllContent)
            {
                spriteBatch = new SpriteBatch(GraphicsDevice);

                bloomExtractEffect = content.Load<Effect>("Content/BloomExtract");
                bloomCombineEffect = content.Load<Effect>("Content/BloomCombine");
                gaussianBlurEffect = content.Load<Effect>("Content/GaussianBlur");
            }

            // Look up the resolution and format of our main backbuffer
            PresentationParameters pp = GraphicsDevice.PresentationParameters;

            int width = pp.BackBufferWidth;
            int height = pp.BackBufferHeight;

            SurfaceFormat format = pp.BackBufferFormat;

            // Create a texture for reading back the back buffer contents
            resolveTarget = new ResolveTexture2D(GraphicsDevice, width, height, 1, format);

            // Create two render targets for the bloom processing. These are half the size of the back buffer, in order
            // to minimize fill rate costs. Reducing the resolution in this way doesn't hurt quality, because we are going
            // to be blurring the bloom images in any case.
            width /= 2;
            height /= 2;

            renderTarget1 = new RenderTarget2D(GraphicsDevice, width, height, 1, format);
            renderTarget2 = new RenderTarget2D(GraphicsDevice, width, height, 1, format);
        }

        /// <summary>
        /// Frees graphics content.
        /// </summary>
        protected override void UnloadGraphicsContent(bool unloadAllContent)
        {
            if (unloadAllContent)
                content.Unload();

            resolveTarget.Dispose();
            renderTarget1.Dispose();
            renderTarget2.Dispose();
        }

        #endregion

        #region Draw

        /// <summary>
        /// Grabs a scene that has already been rendered and uses postprocess magic to add a glowing bloom effect
        /// over the top of it.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            // Resolve the scene into a texture, so we can use it as input data for the bloom effect
            GraphicsDevice.ResolveBackBuffer(resolveTarget);

            // Pass 1: draw the scene into render target 1, using a shader that extracts only the brightest
            // parts of the image.
            bloomExtractEffect.Parameters["BloomThreshold"].SetValue(settings.BloomThreshold);

            DrawFullscreenQuad(resolveTarget, renderTarget1, bloomExtractEffect);

            // Pass 2: draw from render target 1 into render target 2, using a shader to apply a horizontal 
            // gaussian blur filter
            SetBlurEffectParameters(1.0f / (float)renderTarget1.Width, 0);

            DrawFullscreenQuad(renderTarget1.GetTexture(), renderTarget2, gaussianBlurEffect);

            // Pass 3: draw from render target 2 back into render target 1, using a shader to apply a vertical
            // gaussian blur filter
            SetBlurEffectParameters(0, 1.0f / (float)renderTarget1.Height);

            DrawFullscreenQuad(renderTarget2.GetTexture(), renderTarget1, gaussianBlurEffect);

            // Pass 4: draw both render target 1 and the original scene image back into the main back buffer, using
            // a shader that combines them to produce the final bloomed result
            GraphicsDevice.SetRenderTarget(0, null);

            EffectParameterCollection parameters = bloomCombineEffect.Parameters;

            parameters["BloomIntensity"].SetValue(settings.BloomIntensity);
            parameters["BaseIntensity"].SetValue(settings.BaseIntensity);
            parameters["BloomSaturation"].SetValue(settings.BloomSaturation);
            parameters["BaseSaturation"].SetValue(settings.BaseSaturation);

            GraphicsDevice.Textures[1] = resolveTarget;

            Viewport viewport = GraphicsDevice.Viewport;

            DrawFullscreenQuad(renderTarget1.GetTexture(), viewport.Width, viewport.Height, bloomCombineEffect);
        }

        /// <summary>
        /// Helper for drawing a texture into a render target, using a custom shader for postprocessing effects.
        /// </summary>
        private void DrawFullscreenQuad(Texture2D texture, RenderTarget2D renderTarget, Effect effect)
        {
            GraphicsDevice.SetRenderTarget(0, renderTarget);

            DrawFullscreenQuad(texture, renderTarget.Width, renderTarget.Height, effect);

            GraphicsDevice.SetRenderTarget(0, null);
        }

        /// <summary>
        /// Helper for drawing a texture into a render target, using a custom shader for postprocessing effects.
        /// </summary>
        private void DrawFullscreenQuad(Texture2D texture, int width, int height, Effect effect)
        {
            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.None);

            effect.Begin();
            effect.CurrentTechnique.Passes[0].Begin();

            // Draw the quad
            spriteBatch.Draw(texture, new Rectangle(0, 0, width, height), Color.White);
            spriteBatch.End();

            effect.CurrentTechnique.Passes[0].End();
            effect.End();
        }

        /// <summary>
        /// Computes sample weightings and texture coordinate offsets for one pass of a separable 
        /// gaussian blur filter.
        /// </summary>
        private void SetBlurEffectParameters(float dx, float dy)
        {
            // Look up the sample weight and offset effect parameters
            EffectParameter weightsParameter, offsetsParameter;

            weightsParameter = gaussianBlurEffect.Parameters["SampleWeights"];
            offsetsParameter = gaussianBlurEffect.Parameters["SampleOffsets"];

            // Look up how many samples our gaussian blur effect supports
            int sampleCount = weightsParameter.Elements.Count;

            // Create temporary arrays for computing our filter settings
            float[] sampleWeights = new float[sampleCount];
            Vector2[] sampleOffsets = new Vector2[sampleCount];

            // The first sample always has a zero offset
            sampleWeights[0] = ComputeGaussian(0);
            sampleOffsets[0] = new Vector2(0);

            // Maintain a sum of all the weighting values
            float totalWeights = sampleWeights[0];

            // Add pairs of additional sample taps, positioned alonga line in both directions from the center
            for (int i = 0; i < sampleCount / 2; ++i)
            {
                // Store weights for the positive and negative taps
                float weight = ComputeGaussian(i + 1);

                sampleWeights[i * 2 + 1] = weight;
                sampleWeights[i * 2 + 2] = weight;

                totalWeights += weight * 2;

                // To get the maximum amount of blurring from a limited number of
                // pixel shader samples, we take advantage of the bilinear filtering
                // hardware inside the texture fetch unit. If we position our texture
                // coordinates exactly halfway between two texels, the filtering unit
                // will average them for us, giving two samples for the price of one.
                // This allows us to step in units of two texels per sample, rather
                // than just one at a time. The 1.5 offset kicks things off by
                // positioning us nicely in between two texels.
                float sampleOffset = i * 2 + 1.5f;

                Vector2 delta = new Vector2(dx, dy) * sampleOffset;

                // Store texture coordinate offsets for the positive and negative taps
                sampleOffsets[i * 2 + 1] = delta;
                sampleOffsets[i * 2 + 2] = -delta;
            }

            // Normalize the list of sample weights, so they will always sum to one
            for (int i = 0; i < sampleWeights.Length; ++i)
            {
                sampleWeights[i] /= totalWeights;
            }

            // Tell the effect about our new filter settings
            weightsParameter.SetValue(sampleWeights);
            offsetsParameter.SetValue(sampleOffsets);
        }

        /// <summary>
        /// Evaluates a single point on the Gaussian fall off curve.
        /// </summary>
        private float ComputeGaussian(float n)
        {
            float theta = settings.BlurAmount;

            return (float)((1.0 / Math.Sqrt(2 * Math.PI * theta)) * Math.Exp(-(n * n) / (2 * theta * theta)));
        }

        #endregion
    }
}

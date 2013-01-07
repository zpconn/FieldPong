namespace FieldPong
{
    /// <summary>
    /// Holds all the settings used to tweak the bloom effect.
    /// </summary>
    public class BloomSettings
    {
        #region Fields

        /// <summary>
        /// The bright-pass threshold. A pixel's brightness must exceed this value to be bloomed.
        /// </summary>
        public readonly float BloomThreshold;

        /// <summary>
        /// How much blurring is applied to the bloom image.
        /// </summary>
        public readonly float BlurAmount;

        /// <summary>
        /// Controls the amount of the bloom and base images that will be mixed into the final scene.
        /// </summary>
        public readonly float BloomIntensity;
        public readonly float BaseIntensity;

        /// <summary>
        /// Independently control the color saturation of the bloom and base images.
        /// </summary>
        public readonly float BloomSaturation;
        public readonly float BaseSaturation;

        #endregion

        #region Initialization

        public BloomSettings(float bloomThreshold, float blurAmount, float bloomIntensity, float baseIntensity,
                             float bloomSaturation, float baseSaturation)
        {
            BloomThreshold = bloomThreshold;
            BlurAmount = blurAmount;
            BloomIntensity = bloomIntensity;
            BaseIntensity = baseIntensity;
            BloomSaturation = bloomSaturation;
            BaseSaturation = baseSaturation;
        }

        #endregion

        #region Preset configurations

        /// <summary>
        /// Returns the default configuration for the bloom postprocessor.
        /// </summary>
        /// <returns></returns>
        public static BloomSettings Default()
        {
            return new BloomSettings(0f, 2, 1.5f, 1, 2, 3);
        }

        #endregion
    }
}
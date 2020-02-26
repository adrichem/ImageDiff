namespace Adrichem.ImageDiff.Algorithms
{
    using SkiaSharp;

    /// <summary>
    /// Options to control how differences are detected.
    /// </summary>
    public class DiffOptions 
    {
        /// <summary>
        /// What color to ignore when comparing pixels.
        /// </summary>
        public SKColor? IgnoreColor { get; set; }

        /// <summary>
        /// What color to use when highlighting differences.
        /// </summary>
        public SKColor DiffColor { get; set; } = SKColors.Red;
 
    }
}

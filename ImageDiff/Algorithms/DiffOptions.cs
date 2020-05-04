namespace Adrichem.ImageDiff.Algorithms
{
    using System.Drawing;

    /// <summary>
    /// Options to control how differences are detected.
    /// </summary>
    public class DiffOptions 
    {
        /// <summary>
        /// What color to ignore when comparing pixels.
        /// </summary>
        public Color? IgnoreColor { get; set; }

        /// <summary>
        /// What color to use when highlighting differences.
        /// </summary>
        public Color DiffColor { get; set; } = Color.FromArgb(0xff,0x00,0x00);
 
    }
}

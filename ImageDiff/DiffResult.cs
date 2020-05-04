namespace Adrichem.ImageDiff
{
    using System.Drawing;

    /// <summary>
    /// The result of an image comparison.
    /// </summary>
    public class DiffResult
    {
        /// <summary>
        /// Were images different?
        /// </summary>
        public bool Different { get; internal set;}
        /// <summary>
        /// An image that highlights the detected differences.
        /// </summary>
        public Bitmap DiffImage { get; internal set; }
    }
}

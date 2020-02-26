namespace Adrichem.ImageDiff.Algorithms
{
    using System;
    using SkiaSharp;

    /// <summary>
    /// Compares 2 images based on equality of their pixel's RGBA values.
    /// </summary>
    public class SimpleDiffer : IImageDiffer
    {
        /// <summary>
        /// The options to use when diffing the images.
        /// </summary>
        public DiffOptions Options { get; set; } = new DiffOptions();

        /// <summary>
        /// Performs the comparison.
        /// </summary>
        /// <param name="Image1">The 1st image</param>
        /// <param name="Image2">The 2nd image</param>
        /// <returns><seealso cref="DiffResult"/></returns>
        public DiffResult Diff(SKBitmap Image1, SKBitmap Image2)
        {
            if (null == Image1) throw new ArgumentNullException(nameof(Image1));
            if (null == Image2) throw new ArgumentNullException(nameof(Image2));
            if (null == Options) throw new ArgumentNullException(nameof(Options));

            if (Image1.Height != Image2.Height || Image1.Width != Image2.Width)
            {
                return new DiffResult
                {
                    Different = true
                };
            }

            uint NumDifferentPixels = 0;
            var DiffImage = new SKBitmap(Image1.Width, Image1.Height);

            for (int x = 0; x < Image1.Width; x++)
            {
                for (int y = 0; y < Image1.Height; y++)
                {
                    var Pixel1 = Image1.GetPixel(x, y);
                    var Pixel2 = Image2.GetPixel(x, y);

                    if (null != Options.IgnoreColor && (Options.IgnoreColor == Pixel1 || Options.IgnoreColor == Pixel2))
                    {

                        DiffImage.SetPixel(x, y, AlgorithmHelpers.MakeGrayAndTenPercentTransparant(Options.IgnoreColor.Value));
                        continue;
                    }

                    if (Pixel1 != Pixel2)
                    {
                        NumDifferentPixels++;
                        DiffImage.SetPixel(x, y, Options.DiffColor);
                    }
                    else
                    {
                        DiffImage.SetPixel(x, y, AlgorithmHelpers.MakeGrayAndTenPercentTransparant(Pixel1));
                    }
                }
            }

            return new DiffResult
            {
                Different = NumDifferentPixels != 0,
                DiffImage = DiffImage
            };
        }
    }
}

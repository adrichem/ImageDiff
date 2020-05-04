namespace Adrichem.ImageDiff.Algorithms
{
    using System;
    using System.Drawing;

    /// <summary>
    /// Uses perceptual color difference metrics to determine if images look the same according to the paper
    /// "Measuring perceived color difference using YIQ NTSC transmission color space in mobile applications" 
    /// by Y. Kotsarenko and F. Ramos
    /// </summary>
    public class PerceptualDiffer
    {
        public PerceptualDiffOptions Options { get; set; } = new PerceptualDiffOptions();

        public DiffResult Diff(Bitmap Image1, Bitmap Image2)
        {
            if (null == Image1) throw new ArgumentNullException(nameof(Image1));
            if (null == Image2) throw new ArgumentNullException(nameof(Image2));
            if (null == Options) throw new ArgumentNullException(nameof(Options));
            if (Math.Abs(Options.Threshold) > 1.0f) throw new ArgumentException(nameof(Options.Threshold));
            
            if (Image1.Height != Image2.Height || Image1.Width != Image2.Width)
            {
                return new DiffResult
                {
                    Different = true
                };
            }

            //Determine if each pixel is same or different
            uint NumDifferentPixels = 0;
            var DiffImage = new Bitmap(Image1.Width, Image1.Height);

            // Max acceptable square distance between two colors
            // 35215 is the maximum possible value for the YIQ difference metric
            float MaxDifference = 35215 * Options.Threshold * Options.Threshold;
            
	        for(int y = 0; y < Image1.Height; y++)
            {
		        for( int x = 0; x < Image1.Width; x++) 
                {
                    var Pixel1 = Image1.GetPixel(x, y);
                    var Pixel2 = Image2.GetPixel(x, y);

			        if(null != Options.IgnoreColor && (Options.IgnoreColor == Pixel1 || Options.IgnoreColor == Pixel2))
                    {
                        DiffImage.SetPixel(x, y, AlgorithmHelpers.MakeGrayAndTenPercentTransparant(Options.IgnoreColor.Value));
                        continue;
                    }

                    float Difference = PerceptualColorDifference(Pixel1, Pixel2);
			        if (Math.Round(Difference,2) > MaxDifference)
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
                DiffImage = DiffImage,
            };
        }


        private float PerceptualColorDifference(Color Pixel1, Color Pixel2) 
        {

	        byte r1 = ColorHelpers.Blend(Pixel1.R, 255, Pixel1.A);
            byte g1 = ColorHelpers.Blend(Pixel1.G, 255, Pixel1.A);
            byte b1 = ColorHelpers.Blend(Pixel1.B, 255, Pixel1.A);

            byte r2 = ColorHelpers.Blend(Pixel2.R, 255, Pixel2.A);
            byte g2 = ColorHelpers.Blend(Pixel2.G, 255, Pixel2.A);
            byte b2 = ColorHelpers.Blend(Pixel2.B, 255, Pixel2.A);

            float y = ColorHelpers.rgb2y(r1, g1, b1) - ColorHelpers.rgb2y(r2, g2, b2);
            float i = ColorHelpers.rgb2i(r1, g1, b1) - ColorHelpers.rgb2i(r2, g2, b2);
            float q = ColorHelpers.rgb2q(r1, g1, b1) - ColorHelpers.rgb2q(r2, g2, b2);

            return 0.5053f * y * y + 0.299f * i * i + 0.1957f * q * q;
        }
    }
}

namespace Adrichem.ImageDiff.Algorithms
{
    using System.Drawing;
  
    internal static class AlgorithmHelpers
    {
        public static Color MakeGrayAndTenPercentTransparant(Color p)
        {
            byte val = ColorHelpers.Blend(
                ColorHelpers.Luminosity(p.R, p.G, p.B, p.A),
                255,
                26);
            return Color.FromArgb(255, val, val, val);
        }
    }
}

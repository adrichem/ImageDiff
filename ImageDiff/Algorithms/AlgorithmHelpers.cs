namespace Adrichem.ImageDiff.Algorithms
{
    using SkiaSharp;
  
    internal static class AlgorithmHelpers
    {
        public static SKColor MakeGrayAndTenPercentTransparant(SKColor p)
        {
            byte val = ColorHelpers.Blend(
                ColorHelpers.Luminosity(p.Red, p.Green, p.Blue, p.Alpha),
                255,
                26);
            return new SKColor(val, val, val, 255);
        }
    }
}

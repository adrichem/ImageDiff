namespace Adrichem.ImageDiff
{
    internal static class ColorHelpers
    {
        public static byte Blend(byte src, byte dest, byte alpha)
        {
           return (byte) (((alpha * (src - dest)) >> 8) + dest);
        }

        public static byte Luminosity(byte R, byte G, byte B, byte A)
        {
            var r = Blend(R, 255, A);
            var g = Blend(G, 255, A);
            var b = Blend(B, 255, A);
            return (byte)rgb2y(r, g, b);
        }

        public static float rgb2y(float r, float g, float b) { return r * 0.29889531f + g * 0.58662247f + b * 0.11448223f; }
        public static float rgb2i(float r, float g, float b) { return r * 0.59597799f - g * 0.27417610f - b * 0.32180189f; }
        public static float rgb2q(float r, float g, float b) { return r * 0.21147017f - g * 0.52261711f + b * 0.31114694f; }


    }
}

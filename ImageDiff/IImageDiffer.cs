namespace Adrichem.ImageDiff
{
    using SkiaSharp;
    public interface IImageDiffer
    {
        DiffResult Diff(SKBitmap Image1, SKBitmap Image2);
    }
}

using System.Drawing;

namespace Adrichem.ImageDiff
{
    
    public interface IImageDiffer
    {
        DiffResult Diff(Bitmap Image1, Bitmap Image2);
    }
}

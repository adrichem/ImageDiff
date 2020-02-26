namespace Adrichem.ImageDiff.Algorithms
{
    /// <summary>
    /// Represents the options for diffing images using a <see cref="Algorithms.PerceptualDiffer"/>
    /// </summary>
    public class PerceptualDiffOptions : DiffOptions 
    {
        /// <summary>
        /// The max percentage in range [0-1] that pixels may 
        /// differ according to the YIQ difference metric  
        /// </summary>
        public float Threshold { get; set; }
    }
}

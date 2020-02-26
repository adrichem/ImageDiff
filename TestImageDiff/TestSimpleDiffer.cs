namespace TestImageDiff
{
    using Adrichem.ImageDiff;
    using Adrichem.ImageDiff.Algorithms;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SkiaSharp;
    using System.Collections.Generic;
    using System.IO;

    class RealImageTest
    {
        public string Name;
        public string Image1;
        public string Image2;
        public string ExpectedResult;
        public SKColor? IgnoreColor;
    }

    [TestClass]
    public class TestSimpleDiffer
    {
        #region various
        [TestMethod]
        public void TestRealImages()
        {
            var Tests = new List<RealImageTest>
            {
                new RealImageTest {
                    Name = "test1",
                    Image1 = @"./Images/test1a.png",
                    Image2 = @"./Images/test1b.png",
                    IgnoreColor = null,
                    ExpectedResult = @"./Images/test1-result.png"
                },
                new RealImageTest {
                    Name = "test-ignorecolor",
                    Image1 = @"./Images/test-ignorecolor-1.png",
                    Image2 = @"./Images/test-ignorecolor-2.png",
                    IgnoreColor = new SKColor(255, 216, 0, 255),
                    ExpectedResult = @"./Images/test-ignorecolor-result.png"
                }
            };
            foreach(var Test in Tests)
            {
                var Image1 = SKBitmap.Decode(Test.Image1);
                var Image2 = SKBitmap.Decode(Test.Image2);
                var ExpectedDiffImage = SKBitmap.Decode(Test.ExpectedResult);

                var Differ = new SimpleDiffer
                {
                    Options = new DiffOptions
                    {
                        IgnoreColor = Test.IgnoreColor
                    }
                };
                var DiffOutcome = Differ.Diff(Image1, Image2);
                var ActualDiffImage = DiffOutcome.DiffImage;

                SKImage
                    .FromBitmap(ActualDiffImage)
                    .Encode()
                    .SaveTo(File.Create(Path.Combine(Path.GetTempPath(), Test.Name + "-result.png")))
                ;
                Assert.IsTrue(DiffOutcome.Different, Test.Name);
                Assert.IsFalse(Differ.Diff(ExpectedDiffImage, ActualDiffImage).Different, Test.Name);
            }

            
        }

        [TestMethod]
        public void TestDiffColor()
        {
            var Differ = new SimpleDiffer();
            var Image1 = new SKBitmap(1, 1);
            var Image2 = new SKBitmap(1, 1);
            Image1.SetPixel(0, 0, SKColors.Red);
            Image2.SetPixel(0, 0, SKColors.Blue);

            var TestColors = new List<SKColor>
            {
                  SKColors.Red
                , SKColors.Green
                , SKColors.Blue
                , SKColors.Black
                , SKColors.White
                , new SKColor(12,12,12,255)
            };

            foreach (var ExpectedDiffColor in TestColors)
            {
                Differ.Options = new DiffOptions
                {
                    DiffColor = ExpectedDiffColor,
                };
                var Result = Differ.Diff(Image1, Image2);
                var ActualDiffColor = Result.DiffImage.GetPixel(0, 0);
                Assert.AreEqual(ExpectedDiffColor, ActualDiffColor);
            }
        }

        [TestMethod]
        public void TestDifferentImageSize()
        {
            var Image1 = new SKBitmap(1, 1);
            var Image2 = new SKBitmap(2, 2);

            var Differ = new SimpleDiffer();
            var Result = Differ.Diff(Image1, Image2);

            Assert.IsTrue(Result.Different);
            Assert.IsNull(Result.DiffImage);
        }
        #endregion

        #region Parameter validation
        [TestMethod]
        public void TestMissingOptions()
        {
            var Differ = new SimpleDiffer
            {
                Options = null
            };

            var Image1 = new SKBitmap(1, 1);
            var Image2 = new SKBitmap(1, 1);
            Image1.SetPixel(0, 0, SKColors.Red);
            Image2.SetPixel(0, 0, SKColors.Blue);

            Assert.ThrowsException<System.ArgumentNullException>(() => Differ.Diff(Image1, Image2));
        }

        [TestMethod]
        public void TestMissingImages()
        {
            var Differ = new SimpleDiffer();
            var Image1 = new SKBitmap(1, 1);
            var Image2 = new SKBitmap(1, 1);
            Image1.SetPixel(0, 0, SKColors.Red);
            Image2.SetPixel(0, 0, SKColors.Blue);

            Assert.ThrowsException<System.ArgumentNullException>(() => Differ.Diff(Image1, null));
            Assert.ThrowsException<System.ArgumentNullException>(() => Differ.Diff(null, Image2));
            Assert.ThrowsException<System.ArgumentNullException>(() => Differ.Diff(null, null));
        }

        #endregion
                     
        #region Ignore Color tests
        [TestMethod]
        public void TestIgnoreColorZeroPixelsAreIgnoreColor()
        {
            var Differ = new SimpleDiffer();
            var Image1 = new SKBitmap(1, 1);
            var Image2 = new SKBitmap(1, 1);
            Image1.SetPixel(0, 0, SKColors.Red);
            Image2.SetPixel(0, 0, SKColors.Blue);

            Differ.Options = new DiffOptions
            {
                IgnoreColor = SKColors.Green
            };
            var Result = Differ.Diff(Image1, Image2);
            Assert.IsTrue(Result.Different);
        }

        [TestMethod]
        public void TestIgnoreColorOnePixelIsIgnoreColor()
        {
            var Differ = new SimpleDiffer();
            var Image1 = new SKBitmap(1, 1);
            var Image2 = new SKBitmap(1, 1);
            Image1.SetPixel(0, 0, SKColors.Red);
            Image2.SetPixel(0, 0, SKColors.Yellow);

            Differ.Options = new DiffOptions
            {
                IgnoreColor = SKColors.Yellow
            };
            var Result = Differ.Diff(Image1, Image2);
            Assert.IsFalse(Result.Different);

            Image1.SetPixel(0, 0, SKColors.Red);
            Image2.SetPixel(0, 0, SKColors.Yellow);

            Differ.Options = new DiffOptions
            {
                IgnoreColor = SKColors.Red
            };
            Result = Differ.Diff(Image1, Image2);
            Assert.IsFalse(Result.Different);

        }

        [TestMethod]
        public void TestIgnoreColorBothPixelsAreIgnoreColor()
        {
            var Differ = new SimpleDiffer();
            var Image1 = new SKBitmap(1, 1);
            var Image2 = new SKBitmap(1, 1);
            Image1.SetPixel(0, 0, SKColors.Yellow);
            Image2.SetPixel(0, 0, SKColors.Yellow);

            Differ.Options = new DiffOptions
            {
                IgnoreColor = SKColors.Yellow
            };
            var Result = Differ.Diff(Image1, Image2);
            Assert.IsFalse(Result.Different);
        }

        [TestMethod]
        public void TestIgnoreColorNotUsed()
        {
            var Differ = new SimpleDiffer();
            var Image1 = new SKBitmap(1, 1);
            var Image2 = new SKBitmap(1, 1);
            Image1.SetPixel(0, 0, SKColors.Red);
            Image2.SetPixel(0, 0, SKColors.Yellow);

            Differ.Options = new DiffOptions
            {
                IgnoreColor = null,
            };
            var Result = Differ.Diff(Image1, Image2);
            Assert.IsTrue(Result.Different);
        }

        #endregion
    }


}


namespace TestImageDiff
{
    using System.Collections.Generic;
    using System.IO;
    using Adrichem.ImageDiff;
    using Adrichem.ImageDiff.Algorithms;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SkiaSharp;

    [TestClass]
    public class TestPerceptualDiffer
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
            foreach (var Test in Tests)
            {
                var Image1 = SKBitmap.Decode(Test.Image1);
                var Image2 = SKBitmap.Decode(Test.Image2);
                var ExpectedDiffImage = SKBitmap.Decode(Test.ExpectedResult);

                var Differ = new PerceptualDiffer
                {
                    Options = new PerceptualDiffOptions
                    {
                        IgnoreColor = Test.IgnoreColor,
                        Threshold = 0.1f
                    }
                };
                var DiffOutcome = Differ.Diff(Image1, Image2);
                var ActualDiffImage = DiffOutcome.DiffImage;

                SKImage
                    .FromBitmap(ActualDiffImage)
                    .Encode()
                    .SaveTo(File.Create(Path.Combine(Path.GetTempPath(), Test.Name + "perceptual-result.png")))
                ;
                Assert.IsTrue(DiffOutcome.Different, Test.Name);
                Assert.IsFalse(Differ.Diff(ExpectedDiffImage, ActualDiffImage).Different, Test.Name);
            }
        }

        [TestMethod]
        public void TestDiffColors()
        {
            var Differ = new PerceptualDiffer();
            var Image1 = new SKBitmap(1, 1);
            var Image2 = new SKBitmap(1, 1);
            Image1.SetPixel(0, 0, SKColors.Red);
            Image2.SetPixel(0, 0, SKColors.Blue);

            var TestSKColors = new List<SKColor>
            {
                  SKColors.Red
                , SKColors.Green
                , SKColors.Blue
                , SKColors.Black
                , SKColors.White
                , new SKColor(12,12,12,255)
            };

            foreach (var ExpectedDiffColor in TestSKColors)
            {
                Differ.Options = new PerceptualDiffOptions
                {
                    DiffColor = ExpectedDiffColor
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

            var Differ = new PerceptualDiffer();
            var Result = Differ.Diff(Image1, Image2);

            Assert.IsTrue(Result.Different);
            Assert.IsNull(Result.DiffImage);
        }

        #endregion

        #region Parameter validation
        [TestMethod]
        public void TestMissingOptions()
        {
            var Differ = new PerceptualDiffer
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
            var Differ = new PerceptualDiffer();
            var Image1 = new SKBitmap(1, 1);
            var Image2 = new SKBitmap(1, 1);
            Image1.SetPixel(0, 0, SKColors.Red);
            Image2.SetPixel(0, 0, SKColors.Blue);

            Assert.ThrowsException<System.ArgumentNullException>(() => Differ.Diff(Image1, null));
            Assert.ThrowsException<System.ArgumentNullException>(() => Differ.Diff(null, Image2));
            Assert.ThrowsException<System.ArgumentNullException>(() => Differ.Diff(null, null));
        }

        #endregion

        #region Threshold tests
        [TestMethod]
        public void TestThresholdBoundaryZero()
        {
            var Differ = new PerceptualDiffer
            {
                Options = new PerceptualDiffOptions { Threshold = 0.0000f }
            };
            var Image1 = new SKBitmap(1, 1);
            var Image2 = new SKBitmap(1, 1);

            //True outcome
            Image1.SetPixel(0, 0, SKColors.Blue);
            Image2.SetPixel(0, 0, SKColors.Blue);
            var Result = Differ.Diff(Image1, Image2);
            Assert.IsFalse(Result.Different);

            //False outcome
            Image1.SetPixel(0, 0, new SKColor(5,0,0,255));
            Image2.SetPixel(0, 0, new SKColor(6,0,0,255));
            Result = Differ.Diff(Image1, Image2);
            Assert.IsTrue(Result.Different);

        }

        [TestMethod]
        public void TestThresholdBoundaryBetweenZeroAndOne()
        {
            var Differ = new PerceptualDiffer()
            {
                Options = new PerceptualDiffOptions
                {
                    Threshold = 0.1f,
                }
            };

            var Image1 = new SKBitmap(1, 1);
            var Image2 = new SKBitmap(1, 1);
            DiffResult Result;

            //True outcome
            Image1.SetPixel(0, 0, new SKColor(255, 0, 0, 255));
            Image2.SetPixel(0, 0, new SKColor(255, 255, 1,255));
            Result = Differ.Diff(Image1, Image2);
            Assert.IsTrue(Result.Different);

            //False outcome
            Image1.SetPixel(0, 0, new SKColor(255, 0, 0,255));
            Image2.SetPixel(0, 0, new SKColor(255, 1, 1,255));
            Result = Differ.Diff(Image1, Image2);
            Assert.IsFalse(Result.Different);
        }

        [TestMethod]
        public void TestThresholdBoundaryNegative()
        {
            var Differ = new PerceptualDiffer
            {
                Options = new PerceptualDiffOptions { Threshold = -0.1f }
            };
            var Image1 = new SKBitmap(1, 1);
            var Image2 = new SKBitmap(1, 1);
            DiffResult Result;

            //True outcome
            Image1.SetPixel(0, 0, SKColors.Red);
            Image2.SetPixel(0, 0, SKColors.Blue);
            Result = Differ.Diff(Image1, Image2);
            Assert.IsTrue(Result.Different);

            //False outcome
            Image1.SetPixel(0, 0, SKColors.Blue);
            Image2.SetPixel(0, 0, SKColors.Blue);
            Result = Differ.Diff(Image1, Image2);
            Assert.IsFalse(Result.Different);
           
        }

        [TestMethod]
        public void TestThresholdBoundaryMax()
        {
            var Differ = new PerceptualDiffer
            {
                Options = new PerceptualDiffOptions { Threshold = 1.009f }
            };
            var Image1 = new SKBitmap(1, 1);
            var Image2 = new SKBitmap(1, 1);
            Assert.ThrowsException<System.ArgumentException>(() => Differ.Diff(Image1, Image2));


            Differ.Options.Threshold = 1.00f;
            Differ.Diff(Image1, Image2);

            Differ.Options.Threshold = 0.999f;
            Differ.Diff(Image1, Image2);
        }

        #endregion

        #region Ignorecolor tests
        [TestMethod]
        public void TestIgnoreColorNoPixelsAreIgnoreColor()
        {
            var Differ = new PerceptualDiffer();
            var Image1 = new SKBitmap(1, 1);
            var Image2 = new SKBitmap(1, 1);
            Image1.SetPixel(0, 0, SKColors.Red);
            Image2.SetPixel(0, 0, SKColors.Blue);

            Differ.Options = new PerceptualDiffOptions
            {
                IgnoreColor = SKColors.Green
            };
            var Result = Differ.Diff(Image1, Image2);
            Assert.IsTrue(Result.Different);
        }

        [TestMethod]
        public void TestIgnoreColorOnePixelIsIgnoreColor()
        {
            var Differ = new PerceptualDiffer();
            var Image1 = new SKBitmap(1, 1);
            var Image2 = new SKBitmap(1, 1);
            Image1.SetPixel(0, 0, SKColors.Red);
            Image2.SetPixel(0, 0, SKColors.Yellow);

            Differ.Options = new PerceptualDiffOptions
            {
                IgnoreColor = SKColors.Yellow
            };
            var Result = Differ.Diff(Image1, Image2);
            Assert.IsFalse(Result.Different);

        }

        [TestMethod]
        public void TestIgnoreColorBothPixelsAreIgnoreColors()
        {
            var Differ = new PerceptualDiffer();
            var Image1 = new SKBitmap(1, 1);
            var Image2 = new SKBitmap(1, 1);
            Image1.SetPixel(0, 0, SKColors.Yellow);
            Image2.SetPixel(0, 0, SKColors.Yellow);

            Differ.Options = new PerceptualDiffOptions
            {
                IgnoreColor = SKColors.Yellow
            };
            var Result = Differ.Diff(Image1, Image2);
            Assert.IsFalse(Result.Different);
        }

         [TestMethod]
        public void TestIgnoreColorNotUsed()
        {
            var Differ = new PerceptualDiffer();
            var Image1 = new SKBitmap(1, 1);
            var Image2 = new SKBitmap(1, 1);
            Image1.SetPixel(0, 0, SKColors.Red);
            Image2.SetPixel(0, 0, SKColors.Yellow);

            Differ.Options = new PerceptualDiffOptions
            {
                IgnoreColor = null,
            };
            var Result = Differ.Diff(Image1, Image2);
            Assert.IsTrue(Result.Different);
        }

        #endregion
    }
}

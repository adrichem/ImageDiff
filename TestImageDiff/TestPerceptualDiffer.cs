namespace TestImageDiff
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using Adrichem.ImageDiff;
    using Adrichem.ImageDiff.Algorithms;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestPerceptualDiffer
    {
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
                    IgnoreColor = Color.FromArgb(255, 255, 216, 0),
                    ExpectedResult = @"./Images/test-ignorecolor-result.png"
                }
            };
            foreach (var Test in Tests)
            {
                var Image1 = new Bitmap(Image.FromFile(Test.Image1));
                var Image2 = new Bitmap(Image.FromFile(Test.Image2));
                var ExpectedDiffImage =  new Bitmap(Image.FromFile(Test.ExpectedResult));

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
                ActualDiffImage.Save(Path.Combine(Path.GetTempPath(), Test.Name + "perceptual-result.png"), ImageFormat.Png);
                
                Assert.IsTrue(DiffOutcome.Different, Test.Name);
                Assert.IsFalse(Differ.Diff(ExpectedDiffImage, ActualDiffImage).Different, Test.Name);
            }
        }


        #region various
        [TestMethod]
        public void TestDiffColors()
        {
            var Differ = new PerceptualDiffer();
            var Image1 = new Bitmap(1, 1);
            var Image2 = new Bitmap(1, 1);
            Image1.SetPixel(0, 0, Color.Red);
            Image2.SetPixel(0, 0, Color.Blue);

            var TestSKColors = new List<Color>
            {
                  Color.FromArgb(255,0,0)
                , Color.FromArgb(0,255,0)
                , Color.FromArgb(0,0,255)
                , Color.FromArgb(0,0,0)
                , Color.FromArgb(255,255,255)
                , Color.FromArgb(255, 12,12,12)
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
            var Image1 = new Bitmap(1, 1);
            var Image2 = new Bitmap(2, 2);

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

            var Image1 = new Bitmap(1, 1);
            var Image2 = new Bitmap(1, 1);
            Image1.SetPixel(0, 0, Color.Red);
            Image2.SetPixel(0, 0, Color.Blue);

            Assert.ThrowsException<System.ArgumentNullException>(() => Differ.Diff(Image1, Image2));
        }

        [TestMethod]
        public void TestMissingImages()
        {
            var Differ = new PerceptualDiffer();
            var Image1 = new Bitmap(1, 1);
            var Image2 = new Bitmap(1, 1);
            Image1.SetPixel(0, 0, Color.Red);
            Image2.SetPixel(0, 0, Color.Blue);

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
            var Image1 = new Bitmap(1, 1);
            var Image2 = new Bitmap(1, 1);

            //True outcome
            Image1.SetPixel(0, 0, Color.Blue);
            Image2.SetPixel(0, 0, Color.Blue);
            var Result = Differ.Diff(Image1, Image2);
            Assert.IsFalse(Result.Different);

            //False outcome
            Image1.SetPixel(0, 0, Color.FromArgb(5,0,0));
            Image2.SetPixel(0, 0, Color.FromArgb(6,0,0));
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

            var Image1 = new Bitmap(1, 1);
            var Image2 = new Bitmap(1, 1);
            DiffResult Result;

            //True outcome
            Image1.SetPixel(0, 0, Color.FromArgb(255, 0, 0));
            Image2.SetPixel(0, 0, Color.FromArgb(255, 255, 1));
            Result = Differ.Diff(Image1, Image2);
            Assert.IsTrue(Result.Different);

            //False outcome
            Image1.SetPixel(0, 0, Color.FromArgb(255, 0, 0));
            Image2.SetPixel(0, 0, Color.FromArgb(255, 1, 1));
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
            var Image1 = new Bitmap(1, 1);
            var Image2 = new Bitmap(1, 1);
            DiffResult Result;

            //True outcome
            Image1.SetPixel(0, 0, Color.Red);
            Image2.SetPixel(0, 0, Color.Blue);
            Result = Differ.Diff(Image1, Image2);
            Assert.IsTrue(Result.Different);

            //False outcome
            Image1.SetPixel(0, 0, Color.Blue);
            Image2.SetPixel(0, 0, Color.Blue);
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
            var Image1 = new Bitmap(1, 1);
            var Image2 = new Bitmap(1, 1);
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
            var Image1 = new Bitmap(1, 1);
            var Image2 = new Bitmap(1, 1);
            Image1.SetPixel(0, 0, Color.Red);
            Image2.SetPixel(0, 0, Color.Blue);

            Differ.Options = new PerceptualDiffOptions
            {
                IgnoreColor = Color.Green
            };
            var Result = Differ.Diff(Image1, Image2);
            Assert.IsTrue(Result.Different);
        }

        [TestMethod]
        public void TestIgnoreColorOnePixelIsIgnoreColor()
        {
            var Differ = new PerceptualDiffer();
            var Image1 = new Bitmap(1, 1);
            var Image2 = new Bitmap(1, 1);
            Image1.SetPixel(0, 0, Color.Red);
            Image2.SetPixel(0, 0, Color.FromArgb(255,255,0));

            Differ.Options = new PerceptualDiffOptions
            {
                IgnoreColor = Color.FromArgb(255, 255, 0)
            };
            var Result = Differ.Diff(Image1, Image2);
            Assert.IsFalse(Result.Different);

        }

        [TestMethod]
        public void TestIgnoreColorBothPixelsAreIgnoreColors()
        {
            var Differ = new PerceptualDiffer();
            var Image1 = new Bitmap(1, 1);
            var Image2 = new Bitmap(1, 1);
            Image1.SetPixel(0, 0, Color.Yellow);
            Image2.SetPixel(0, 0, Color.Yellow);

            Differ.Options = new PerceptualDiffOptions
            {
                IgnoreColor = Color.Yellow
            };
            var Result = Differ.Diff(Image1, Image2);
            Assert.IsFalse(Result.Different);
        }

         [TestMethod]
        public void TestIgnoreColorNotUsed()
        {
            var Differ = new PerceptualDiffer();
            var Image1 = new Bitmap(1, 1);
            var Image2 = new Bitmap(1, 1);
            Image1.SetPixel(0, 0, Color.Red);
            Image2.SetPixel(0, 0, Color.Yellow);

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

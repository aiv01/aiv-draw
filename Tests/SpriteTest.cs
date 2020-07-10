using Aiv.Draw;
using NUnit.Framework;

namespace Tests
{
    public class SpriteTest
    {
        [Test]
        public void ReadSprite_2x2_RGBA()
        {
            /*  2 x 2
                Red Gre
                Blu Yel
             */
            string filePath = TestUtil.FullPath("rgba-2x2.png");
            Sprite sprite = new Sprite(filePath);

            Assert.AreEqual(2, sprite.Width);
            Assert.AreEqual(2, sprite.Height);
            Assert.AreEqual(PixelFormat.RGBA, sprite.Format);
            Assert.AreEqual(32, sprite.Depth);
            Assert.AreEqual(2 * 2 * 4, sprite.Bitmap.Length);

            //Red
            Assert.AreEqual(sprite.Bitmap[0], 255);
            Assert.AreEqual(sprite.Bitmap[1], 0);
            Assert.AreEqual(sprite.Bitmap[2], 0);
            Assert.AreEqual(sprite.Bitmap[3], 255);

            //Green
            Assert.AreEqual(sprite.Bitmap[4], 0);
            Assert.AreEqual(sprite.Bitmap[5], 255);
            Assert.AreEqual(sprite.Bitmap[6], 0);
            Assert.AreEqual(sprite.Bitmap[7], 255);

            //Blue
            Assert.AreEqual(sprite.Bitmap[8],  0);
            Assert.AreEqual(sprite.Bitmap[9],  0);
            Assert.AreEqual(sprite.Bitmap[10], 255);
            Assert.AreEqual(sprite.Bitmap[11], 255);

            //Yellow
            Assert.AreEqual(sprite.Bitmap[12], 255);
            Assert.AreEqual(sprite.Bitmap[13], 255);
            Assert.AreEqual(sprite.Bitmap[14], 0);
            Assert.AreEqual(sprite.Bitmap[15], 255);
        }

        [Test]
        public void ReadSprite_2x2_RGB()
        {
            /*  2 x 2 
                Red Gre
                Blu Yel
             */
            string filePath = TestUtil.FullPath("rgb-2x2.png");
            Sprite sprite = new Sprite(filePath);

            Assert.AreEqual(2, sprite.Width);
            Assert.AreEqual(2, sprite.Height);
            Assert.AreEqual(2 * 2 * 3, sprite.Bitmap.Length);
            Assert.AreEqual(PixelFormat.RGB, sprite.Format);
            Assert.AreEqual(24, sprite.Depth);

            //Red
            Assert.AreEqual(sprite.Bitmap[0], 255);
            Assert.AreEqual(sprite.Bitmap[1], 0);
            Assert.AreEqual(sprite.Bitmap[2], 0);

            //Green
            Assert.AreEqual(sprite.Bitmap[3], 0);
            Assert.AreEqual(sprite.Bitmap[4], 255);
            Assert.AreEqual(sprite.Bitmap[5], 0);

            //Blue
            Assert.AreEqual(sprite.Bitmap[6], 0);
            Assert.AreEqual(sprite.Bitmap[7], 0);
            Assert.AreEqual(sprite.Bitmap[8], 255);

            //Yellow
            Assert.AreEqual(sprite.Bitmap[9], 255);
            Assert.AreEqual(sprite.Bitmap[10], 255);
            Assert.AreEqual(sprite.Bitmap[11], 0);
        }
    }
}

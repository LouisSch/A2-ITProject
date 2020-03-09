using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProjetCode
{
    [TestClass]
    public class UnitTest
    {
        private byte[] headerPart = { 66, 77, 230, 4, 0, 0, 0, 0, 0, 0, 54, 0, 0, 0 };
        private byte[] headerInfoPart = { 40, 0, 0, 0, 20, 0, 0, 0, 20, 0, 0, 0, 1, 0, 24, 0, 0, 0, 0, 0, 176, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        [TestMethod]
        public void TestLEToDec()
        {
            Assert.AreEqual(1254, MyImage.ConvertLEToDec(MyImage.GetLittleEndian(headerPart, 2, 4)));
        }

        [TestMethod]
        public void TestDecToLE()
        {
            CollectionAssert.AreEqual(MyImage.GetLittleEndian(headerPart, 2, 4), MyImage.ConvertDecToLE(1254, 4));
        }

        [TestMethod]
        public void TestGetLittleEndian()
        {
            CollectionAssert.AreEqual(new byte[4] { 230, 4, 0, 0 }, MyImage.GetLittleEndian(headerPart, 2, 4));
        }

        [TestMethod]
        public void TestIsBitMap()
        {
            Assert.AreEqual(true, MyImage.IsBitMap(headerPart));
        }

        [TestMethod]
        public void TestCompleteWith()
        {
            byte[] headerToCompare = new byte[40] { 40, 0, 0, 0, 20, 0, 0, 0, 20, 0, 0, 0, 1, 0, 24, 0, 0, 0, 0, 0, 176, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            MyImage.CompleteWith(headerToCompare, 0, 22, 18);

            CollectionAssert.AreEqual(headerInfoPart, headerToCompare);
        }
    }
}

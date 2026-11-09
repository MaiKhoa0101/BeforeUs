using System;
using NUnit.Framework;

namespace BeforeUs.Core.Tests
{
    [TestFixture]
    public class MathUtilTests
    {
        [Test]
        public void Clamp_KepGiaTriVaoKhoang()
        {
            Assert.AreEqual(0.05, MathUtil.Clamp(-1.0, 0.05, 0.92), 1e-12);
            Assert.AreEqual(0.92, MathUtil.Clamp(5.0, 0.05, 0.92), 1e-12);
            Assert.AreEqual(0.50, MathUtil.Clamp(0.50, 0.05, 0.92), 1e-12);
        }

        [Test]
        public void Clamp_SoNguyen()
        {
            Assert.AreEqual(1, MathUtil.Clamp(-3, 1, 5));
            Assert.AreEqual(5, MathUtil.Clamp(9, 1, 5));
            Assert.AreEqual(3, MathUtil.Clamp(3, 1, 5));
        }

        [Test]
        public void Clamp_MinLonHonMax_ThiBaoLoi()
        {
            Assert.Throws<ArgumentException>(() => MathUtil.Clamp(1.0, 2.0, 1.0));
        }
    }
}

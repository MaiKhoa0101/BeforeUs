using System;
using System.Collections.Generic;
using BeforeUs.Core;
using NUnit.Framework;

namespace BeforeUs.Core.Tests
{
    /// <summary>C2. Tiêu chí "xong khi" của HANDOFF: cùng seed cho cùng chuỗi kết quả.</summary>
    [TestFixture]
    public class RandomTests
    {
        [Test]
        public void CungSeed_ChoCungChuoi()
        {
            IRandom a = new XorShift128Random(12345);
            IRandom b = new XorShift128Random(12345);

            for (int i = 0; i < 1000; i++)
            {
                Assert.AreEqual(a.NextDouble(), b.NextDouble(), 0.0, "lần rút thứ " + i);
            }
        }

        [Test]
        public void SeedKhacNhau_ChoChuoiKhacNhau()
        {
            IRandom a = new XorShift128Random(1);
            IRandom b = new XorShift128Random(2);

            int same = 0;
            for (int i = 0; i < 100; i++)
            {
                if (a.NextDouble() == b.NextDouble())
                {
                    same++;
                }
            }

            Assert.Less(same, 5, "hai seed khác nhau mà trùng quá nhiều");
        }

        [Test]
        public void LuuVaKhoiPhucTrangThai_TiepDungChoCu()
        {
            XorShift128Random rng = new XorShift128Random(777);
            for (int i = 0; i < 50; i++)
            {
                rng.NextDouble();
            }

            string snapshot = rng.SaveState();
            double[] expected = Draw(rng, 100);

            XorShift128Random restored = new XorShift128Random(1);
            restored.LoadState(snapshot);
            double[] actual = Draw(restored, 100);

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void KhoiPhucVaoChinhNo_CungCho()
        {
            XorShift128Random rng = new XorShift128Random(42);
            rng.NextInt(0, 100);
            string snapshot = rng.SaveState();

            double[] first = Draw(rng, 20);
            rng.LoadState(snapshot);
            double[] second = Draw(rng, 20);

            CollectionAssert.AreEqual(first, second);
        }

        [Test]
        public void TrangThaiHong_ThiBaoLoi()
        {
            XorShift128Random rng = new XorShift128Random(1);

            Assert.Throws<ArgumentNullException>(() => rng.LoadState(null));
            Assert.Throws<ArgumentException>(() => rng.LoadState("mt19937:1,2,3,4"));
            Assert.Throws<ArgumentException>(() => rng.LoadState("xs128:1,2,3"));
            Assert.Throws<ArgumentException>(() => rng.LoadState("xs128:zz,2,3,4"));
            Assert.Throws<ArgumentException>(() => rng.LoadState("xs128:00000000,00000000,00000000,00000000"));
        }

        [Test]
        public void NextDouble_LuonNamTrongNuaKhoangDonVi()
        {
            IRandom rng = new XorShift128Random(99);
            for (int i = 0; i < 200000; i++)
            {
                double value = rng.NextDouble();
                if (value < 0.0 || value >= 1.0)
                {
                    Assert.Fail("NextDouble trả " + value);
                }
            }
        }

        [Test]
        public void NextInt_TrongKhoangVaPhuHetKhoang()
        {
            IRandom rng = new XorShift128Random(5);
            bool[] seen = new bool[6];

            for (int i = 0; i < 20000; i++)
            {
                int value = rng.NextInt(3, 9);
                Assert.GreaterOrEqual(value, 3);
                Assert.Less(value, 9);
                seen[value - 3] = true;
            }

            CollectionAssert.DoesNotContain(seen, false, "có giá trị trong khoảng không bao giờ rút được");
        }

        [Test]
        public void NextInt_KhoangRong_ThiBaoLoi()
        {
            IRandom rng = new XorShift128Random(1);
            Assert.Throws<ArgumentOutOfRangeException>(() => rng.NextInt(5, 5));
            Assert.Throws<ArgumentOutOfRangeException>(() => rng.NextInt(5, 4));
        }

        [Test]
        public void NextInt_PhanBoDeu_TrongSaiSoThongKe()
        {
            // Lemire có vòng loại bỏ, nên phân bố phải đều. Kiểm thô: 6 mặt, 60.000 lần rút.
            IRandom rng = new XorShift128Random(2024);
            int[] counts = new int[6];
            const int draws = 60000;

            for (int i = 0; i < draws; i++)
            {
                counts[rng.NextInt(0, 6)]++;
            }

            foreach (int count in counts)
            {
                Assert.That(count, Is.InRange(draws / 6 - 600, draws / 6 + 600));
            }
        }

        [Test]
        public void TrangThaiLuuRa_LaChuoiOnDinhVaDocDuoc()
        {
            XorShift128Random rng = new XorShift128Random(3);
            string state = rng.SaveState();

            StringAssert.StartsWith("xs128:", state);
            Assert.AreEqual(4, state.Substring("xs128:".Length).Split(',').Length);
            Assert.AreEqual(state, rng.SaveState(), "lưu hai lần liên tiếp phải cho cùng chuỗi");
        }

        private static double[] Draw(IRandom rng, int count)
        {
            List<double> values = new List<double>(count);
            for (int i = 0; i < count; i++)
            {
                values.Add(rng.NextDouble());
            }

            return values.ToArray();
        }
    }
}

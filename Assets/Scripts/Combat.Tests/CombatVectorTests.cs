using NUnit.Framework;

namespace BeforeUs.Combat.Tests
{
    [TestFixture]
    public sealed class CombatVectorTests
    {
        [Test]
        public void KhoangCach_TinhDung()
        {
            Assert.That(
                new CombatVector(0.0, 0.0).DistanceTo(new CombatVector(3.0, 4.0)),
                Is.EqualTo(5.0).Within(1e-12));
        }

        [Test]
        public void DiVePhiaDich_KhongBaoGioDiQua()
        {
            CombatVector moved = new CombatVector(0.0, 0.0).MovedToward(new CombatVector(1.0, 0.0), 10.0);

            Assert.That(moved, Is.EqualTo(new CombatVector(1.0, 0.0)));
        }

        [Test]
        public void DiVePhiaDich_DungQuangDuongChoTruoc()
        {
            CombatVector moved = new CombatVector(0.0, 0.0).MovedToward(new CombatVector(10.0, 0.0), 2.5);

            Assert.That(moved.X, Is.EqualTo(2.5).Within(1e-12));
            Assert.That(moved.Y, Is.EqualTo(0.0).Within(1e-12));
        }

        [Test]
        public void DiVePhiaDich_QuangDuongKhongDuong_ThiDungYen()
        {
            CombatVector start = new CombatVector(4.0, 2.0);

            Assert.That(start.MovedToward(new CombatVector(10.0, 2.0), 0.0), Is.EqualTo(start));
            Assert.That(start.MovedToward(new CombatVector(10.0, 2.0), -1.0), Is.EqualTo(start));
        }

        [Test]
        public void DiVePhiaChinhMinh_ThiDungYen()
        {
            CombatVector start = new CombatVector(4.0, 2.0);

            Assert.That(start.MovedToward(start, 5.0), Is.EqualTo(start));
        }

        [Test]
        public void VungThaQuan_ChanDungBienSan()
        {
            BattleField field = TestBattles.LureField();

            Assert.That(field.IsInsideDeployZone(new CombatVector(8.0, 3.0)), Is.True);
            Assert.That(field.IsInsideDeployZone(new CombatVector(0.5, 3.0)), Is.False, "trái vùng thả");
            Assert.That(field.IsInsideDeployZone(new CombatVector(18.0, 3.0)), Is.False, "phải vùng thả");
            Assert.That(field.IsInsideDeployZone(new CombatVector(8.0, -1.0)), Is.False, "dưới mép sân");
            Assert.That(field.IsInsideDeployZone(new CombatVector(8.0, 99.0)), Is.False, "trên mép sân");
        }
    }
}

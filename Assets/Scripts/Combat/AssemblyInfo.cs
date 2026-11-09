using System.Runtime.CompilerServices;

// Cho phép test dựng BattleReport trực tiếp để kiểm tra chính sách tổn thất một cách cô lập,
// thay vì phải dàn dựng cả một trận chỉ để ép ra đúng danh sách người ngã. Cùng lối với Core
// (Assets/Scripts/Core/AssemblyInfo.cs). Tên assembly giống nhau ở cả hai đường build
// (asmdef BeforeUs.Combat.Tests trong Unity, AssemblyName BeforeUs.Combat.Tests ở tools/CombatTests).
[assembly: InternalsVisibleTo("BeforeUs.Combat.Tests")]

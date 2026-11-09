using System.Runtime.CompilerServices;

// Cho phép test dựng TurnEventContext trực tiếp để kiểm tra hệ sự kiện một cách cô lập,
// thay vì phải chạy qua cả một lượt. Tên assembly giống nhau ở cả hai đường build
// (asmdef BeforeUs.Core.Tests trong Unity, AssemblyName BeforeUs.Core.Tests ở tools/CoreTests).
[assembly: InternalsVisibleTo("BeforeUs.Core.Tests")]

//
// OpenMode.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
//
namespace Microsoft.VisualBasic {
	public enum OpenMode : int {
		Input = 1,
		Output = 2,
		Random = 4,
		Append = 8,
		Binary = 32
	};
}

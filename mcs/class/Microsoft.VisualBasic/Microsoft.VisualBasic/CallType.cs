//
// CallType.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
//
namespace Microsoft.VisualBasic {
	public enum CallType : int {
		Method = 1,
		Get = 2,
		Let = 4,
		Set = 8
	};
}

//
// TriState.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
//
namespace Microsoft.VisualBasic {
	/// <summary>
	/// When you call number-formatting functions, you can use the following enumeration 
	/// members in your code in place of the actual values.
	/// </summary>
	public enum TriState : int {
		False = 0,
		UseDefault = -2,
		True = -1
	};
}

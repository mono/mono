//
// CompareMethod.cs
//
// Author:
//   Martin Adoue (martin@cwanet.com)
//
// (C) 2002 Martin Adoue
//
namespace Microsoft.VisualBasic {

	/// <summary>
	/// The CompareMethod enumeration contains constants used to determine the 
	/// way strings are compared when using functions such as InStr and StrComp. 
	/// These constants can be used anywhere in your code.
	/// </summary>
	public enum CompareMethod : int {
		/// <summary>
		/// Performs a binary comparison
		/// </summary>
		Binary = 0,	
		/// <summary>
		/// Performs a textual comparison
		/// </summary>
		Text = 1	
	};
}

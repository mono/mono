//
// FileAttribute.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
//
namespace Microsoft.VisualBasic {
	[System.FlagsAttribute] 
	public enum FileAttribute : int {
		Normal = 0,
		ReadOnly = 1,
		Hidden = 2,
		System = 4,
		Volume = 8,
		Directory = 16,
		Archive = 32
	};
}

//
// MsgBoxStyle.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
//
namespace Microsoft.VisualBasic {
	[System.FlagsAttribute] 
	public enum MsgBoxStyle : int {
		ApplicationModal = 0,
		DefaultButton1 = 0,
		OKOnly = 0,
		OKCancel = 1,
		AbortRetryIgnore = 2,
		YesNoCancel = 3,
		YesNo = 4,
		RetryCancel = 5,
		Critical = 16,
		Question = 32,
		Exclamation = 48,
		Information = 64,
		DefaultButton2 = 256,
		DefaultButton3 = 512,
		SystemModal = 4096,
		MsgBoxHelp = 16384,
		MsgBoxSetForeground = 65536,
		MsgBoxRight = 524288,
		MsgBoxRtlReading = 1048576
	};
}

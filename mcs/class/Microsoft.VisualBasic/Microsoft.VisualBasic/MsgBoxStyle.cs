//
// MsgBoxStyle.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
namespace Microsoft.VisualBasic {
	[System.Flags] 
#if NET_2_0
	[System.Serializable]
#endif
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

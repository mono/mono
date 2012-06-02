//
// System.Net.FtpStatusCode.cs
//
// Author:
// 	Carlos Alberto Cortez (calberto.oortez@gmail.com)
//
// (c) Copyright 2005 Novell, Inc. (http://www.ximian.com)
//

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

namespace System.Net
{
	public enum FtpStatusCode
	{
		Undefined = 0,
		RestartMarker = 110,
		ServiceTemporarilyNotAvailable = 120,
		DataAlreadyOpen = 125,
		OpeningData = 150,
		CommandOK = 200,
		CommandExtraneous = 202,
		DirectoryStatus = 212,
		FileStatus = 213,
		SystemType = 215,
		SendUserCommand = 220,
		ClosingControl = 221,
		ClosingData = 226,
		EnteringPassive = 227,
		LoggedInProceed = 230,
		ServerWantsSecureSession = 234,
		FileActionOK = 250,
		PathnameCreated = 257,
		SendPasswordCommand = 331,
		NeedLoginAccount = 332,
		FileCommandPending = 350,
		ServiceNotAvailable = 421,
		CantOpenData = 425,
		ConnectionClosed = 426,
		ActionNotTakenFileUnavailableOrBusy = 450,
		ActionAbortedLocalProcessingError = 451,
		ActionNotTakenInsufficientSpace = 452,
		CommandSyntaxError = 500,
		ArgumentSyntaxError = 501,
		CommandNotImplemented = 502,
		BadCommandSequence = 503,
		NotLoggedIn = 530,
		AccountNeeded = 532,
		ActionNotTakenFileUnavailable = 550,
		ActionAbortedUnknownPageType = 551,
		FileActionAborted = 552,
		ActionNotTakenFilenameNotAllowed = 553
	}
}



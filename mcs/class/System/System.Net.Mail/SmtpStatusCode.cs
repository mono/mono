//
// System.Net.Mail.SmtpStatusCode.cs
//
// Author:
//	Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004
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

namespace System.Net.Mail {
	public enum SmtpStatusCode
	{
		BadCommandSequence = 503,
		CannotVerifyUserWillAttemptDelivery = 252,
		ClientNotPermitted = 454,
		CommandNotImplemented = 502,
		CommandParameterNotImplemented = 504,
		CommandUnrecognized = 500,
		ExceededStorageAllocation = 552,
		GeneralFailure = -1,
		HelpMessage = 214,
		InsufficientStorage = 452,
		LocalErrorInProcessing = 451,
		MailboxBusy = 450,
		MailboxNameNotAllowed = 553,
		MailboxUnavailable = 550,
		Ok = 250,
		ServiceClosingTransmissionChannel = 221,
		ServiceNotAvailable = 421,
		ServiceReady = 220,
		StartMailInput = 354,
		SyntaxError = 501,
		SystemStatus = 211,
		TransactionFailed = 554,
		UserNotLocalTryAlternatePath = 551,
		UserNotLocalWillForward = 251,
		MustIssueStartTlsFirst = 530,
	}
}


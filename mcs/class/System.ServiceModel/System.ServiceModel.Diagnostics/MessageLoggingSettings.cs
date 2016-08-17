//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2011 Novell, Inc.  http://www.novell.com
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
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Configuration;

namespace System.ServiceModel.Diagnostics
{
	internal class MessageLoggingSettings
	{
		public MessageLoggingSettings ()
		{
#if !MOBILE && !XAMMAC_4_5
			var e = ConfigUtil.DiagnosticSection.MessageLogging;
			LogEntireMessage = e.LogEntireMessage;
			LogKnownPii = e.LogKnownPii;
			LogMalformedMessages = e.LogMalformedMessages;
			LogMessagesAtServiceLevel = e.LogMessagesAtServiceLevel;
			LogMessagesAtTransportLevel = e.LogMessagesAtTransportLevel;
			MaxMessagesToLog = e.MaxMessagesToLog;
			MaxSizeOfMessageToLog = e.MaxSizeOfMessageToLog;
#endif
		}

		[MonoTODO]
		public bool LogEntireMessage { get; set; }
		[MonoTODO] // how is it used?
		public bool LogKnownPii { get; set; }
		[MonoTODO]
		public bool LogMalformedMessages { get; set; }
		[MonoTODO]
		public bool LogMessagesAtServiceLevel { get; set; }
		[MonoTODO]
		public bool LogMessagesAtTransportLevel { get; set; }
		[MonoTODO]
		public int MaxMessagesToLog { get; set; }
		[MonoTODO]
		public int MaxSizeOfMessageToLog { get; set; }
	}
}

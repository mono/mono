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
using System.ServiceModel.Channels;

namespace System.ServiceModel.Diagnostics
{
	// This class is based on .NET WCF trace log MessageLogTraceRecord element.
	// (maybe not fully compatible; I only saw trace xml in its intrinsic namespace)
	internal class MessageLogTraceRecord
	{
		public MessageLogTraceRecord ()
		{
		}
		
		public MessageLogTraceRecord (MessageLogSourceKind kind, Type type, MessageBuffer msgbuf)
		{
			Time = DateTime.Now;
			Source = kind;
			Type = type;
			Message = msgbuf;
		}
		public DateTime Time { get; set; }
		public MessageLogSourceKind Source { get; set; }
		public Type Type { get; set; }
		public MessageBuffer Message { get; set; }
	}
}

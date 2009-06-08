//
// ReliableMessagingVersion.cs
//
// Author:
//	Igor Zelmanovich <igorz@mainsoft.com>
//
// Copyright (C) 2008 Mainsoft, Inc.  http://www.mainsoft.com
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
using System.Text;

namespace System.ServiceModel
{
	[MonoTODO]
	public abstract class ReliableMessagingVersion
	{
		static ReliableMessagingVersion _default = new FakeReliableMessagingVersion ();
		static ReliableMessagingVersion _wsReliableMessaging11 = new FakeReliableMessagingVersion ();
		static ReliableMessagingVersion _wsReliableMessagingFebruary2005 = new FakeReliableMessagingVersion ();

		public static ReliableMessagingVersion Default {
			get { return _default; }
		}

		public static ReliableMessagingVersion WSReliableMessaging11 {
			get { return _wsReliableMessaging11; }
		}

		public static ReliableMessagingVersion WSReliableMessagingFebruary2005 {
			get { return _wsReliableMessagingFebruary2005; }
		}

		[MonoTODO ("must be replaces with a correct implementation")]
		class FakeReliableMessagingVersion : ReliableMessagingVersion
		{
		}
	}
}

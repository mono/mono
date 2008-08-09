//
// System.ServiceProcess.SessionChangeDescription
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.
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

#if NET_2_0

namespace System.ServiceProcess
{
	public struct SessionChangeDescription
	{
		SessionChangeReason reason;
		int id;

		internal SessionChangeDescription (SessionChangeReason reason, int id)
		{
			this.reason = reason;
			this.id = id;
		}

		public SessionChangeReason Reason {
			get { return reason; }
		}

		public int SessionId {
			get { return id; }
		}

		public static bool operator == (SessionChangeDescription a, SessionChangeDescription b)
		{
			return a.Equals (b);
		}

		public static bool operator != (SessionChangeDescription a, SessionChangeDescription b)
		{
			return !a.Equals (b);
		}

		public override bool Equals (Object obj)
		{
			return (obj is SessionChangeDescription) ? Equals ((SessionChangeDescription) obj) : false;
		}

		public bool Equals (SessionChangeDescription changeDescription)
		{
			return reason == changeDescription.reason && id == changeDescription.id;
		}

		public override int GetHashCode ()
		{
			return (((int) reason ^ 5) << 16) + id;
		}
	}
}

#endif

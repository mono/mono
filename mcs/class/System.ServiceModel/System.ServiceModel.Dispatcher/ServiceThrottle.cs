//
// ServiceThrottle.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
namespace System.ServiceModel.Dispatcher
{
	public sealed class ServiceThrottle
	{
		internal ServiceThrottle (ChannelDispatcher owner)
		{
			if (owner == null)
				throw new ArgumentNullException ("owner");
			this.owner = owner;
		}

		ChannelDispatcher owner;
		int max_call = 16, max_session = 10, max_instance = 26;

		public int MaxConcurrentCalls {
			get { return max_call; }
			set {
				CheckState ();
				max_call = value;
			}
		}

		public int MaxConcurrentSessions {
			get { return max_session; }
			set {
				CheckState ();
				max_session = value;
			}
		}

		public int MaxConcurrentInstances {
			get { return max_instance; }
			set {
				CheckState ();
				max_instance = value;
			}
		}

		void CheckState ()
		{
			if (owner.State != CommunicationState.Created)
				throw new InvalidOperationException ("Cannot change throttling settings after ChannelDispatcher got opened.");
		}
	}
}

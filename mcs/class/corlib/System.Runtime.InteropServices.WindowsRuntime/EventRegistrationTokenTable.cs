#if NET_4_5
//
// EventRegistrationTokenTable.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Runtime.CompilerServices;

namespace System.Runtime.InteropServices.WindowsRuntime
{
	[MonoTODO]
	public sealed class EventRegistrationTokenTable<T>
		where T : class
	{
		public EventRegistrationTokenTable ()
		{
			throw new NotImplementedException ();
		}

		public T InvocationList {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public EventRegistrationToken AddEventHandler (T handler)
		{
			throw new NotImplementedException ();
		}

		public static EventRegistrationTokenTable<T> GetOrCreateEventRegistrationTokenTable(ref EventRegistrationTokenTable<T> refEventTable)
		{
			throw new NotImplementedException ();
		}

		public void RemoveEventHandler (T handler)
		{
			throw new NotImplementedException ();
		}

		public void RemoveEventHandler (EventRegistrationToken token)
		{
			throw new NotImplementedException ();
		}
	}
}
#endif


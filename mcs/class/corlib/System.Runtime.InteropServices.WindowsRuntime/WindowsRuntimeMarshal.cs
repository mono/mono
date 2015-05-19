//
// WindowsRuntimeMarshal.cs
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
	public static class WindowsRuntimeMarshal
	{
		public static void AddEventHandler<T> (	Func<T, EventRegistrationToken> addMethod, Action<EventRegistrationToken> removeMethod, T handler)
		{
			throw new NotImplementedException ();
		}

		public static void FreeHString (IntPtr ptr)
		{
			throw new NotImplementedException ();
		}

		public static IActivationFactory GetActivationFactory (Type type)
		{
			throw new NotImplementedException ();
		}

		public static string PtrToStringHString (IntPtr ptr)
		{
			throw new NotImplementedException ();
		}

		public static void RemoveAllEventHandlers(Action<EventRegistrationToken> removeMethod)
		{
			throw new NotImplementedException ();
		}

		public static void RemoveEventHandler<T> (Action<EventRegistrationToken> removeMethod, T handler)
		{
			throw new NotImplementedException ();
		}

		public static IntPtr StringToHString (string s)
		{
			throw new NotImplementedException ();
		}

		internal static bool ReportUnhandledError (Exception e)
		{
			return false;
		}
	}
}


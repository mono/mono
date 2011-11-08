//
// System.TimeZoneInfo helper for MonoTouch
// 	because the devices cannot access the file system to read the data
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2011 Xamarin Inc.
//
// The class can be either constructed from a string (from user code)
// or from a handle (from iphone-sharp.dll internal calls).  This
// delays the creation of the actual managed string until actually
// required
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

#if (INSIDE_CORLIB && MONOTOUCH)

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;

namespace System {

	public partial class TimeZoneInfo {
		
		static Type nstimezone;
		
		static Type NSTimeZone {
			get {
				if (nstimezone == null)
					nstimezone = Type.GetType ("MonoTouch.Foundation.NSTimeZone, monotouch, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
				return nstimezone;
			}
		}
		
		static ReadOnlyCollection<string> GetMonoTouchNames ()
		{
			try {
				var p = NSTimeZone.GetProperty ("KnownTimeZoneNames", BindingFlags.Static | BindingFlags.Public);
				var m = p.GetGetMethod ();
				return (ReadOnlyCollection<string>) m.Invoke (null, null);
			}
			catch (TargetInvocationException tie) {
				throw tie.InnerException;
			}
		}
		
		static Stream GetMonoTouchDefault ()
		{
			try {
				var m = NSTimeZone.GetMethod ("_GetDefault", BindingFlags.Static | BindingFlags.NonPublic);
				return (Stream) m.Invoke (null, null);
			}
			catch (TargetInvocationException tie) {
				throw tie.InnerException;
			}
		}

		static Stream GetMonoTouchData (string name)
		{
			try {
				var m = NSTimeZone.GetMethod ("_GetData", BindingFlags.Static | BindingFlags.NonPublic);
				return (Stream) m.Invoke (null, new object[] { name });
			}
			catch (TargetInvocationException tie) {
				throw tie.InnerException;
			}
		}
	}
}

#endif

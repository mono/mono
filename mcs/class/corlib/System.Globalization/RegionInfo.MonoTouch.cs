//
// System.Globalization.RegionInfo helper for MonoTouch
// 	because the devices cannot access the file system to read the data
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2012 Xamarin Inc.
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

#if MONOTOUCH

using System;
using System.Reflection;

namespace System.Globalization {

	public partial class RegionInfo {
		
		static Type nslocale;
		
		static Type NSLocale {
			get {
				if (nslocale == null)
					nslocale = Type.GetType ("MonoTouch.Foundation.NSLocale, monotouch, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
				return nslocale;
			}
		}
		
		static RegionInfo CreateFromNSLocale ()
		{
			try {
				var cl = NSLocale.GetProperty ("CurrentLocale", BindingFlags.Static | BindingFlags.Public).GetGetMethod ();
				var cc = NSLocale.GetProperty ("CountryCode", BindingFlags.Instance | BindingFlags.Public).GetGetMethod ();
				
				object current = cl.Invoke (null, null);
				return new RegionInfo ((string) cc.Invoke (current, null));
			}
			catch (TargetInvocationException tie) {
				throw tie.InnerException;
			}
		}
	}
}

#endif
//
// System.TimeZoneInfo helper for MonoTouch
// 	because the devices cannot access the file system to read the data
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2011-2013 Xamarin Inc.
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

#if MONOTOUCH || XAMMAC

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;

namespace System {

	public partial class TimeZoneInfo {

		[DllImport ("__Internal")]
		extern static string xamarin_timezone_get_local_name ();

		static TimeZoneInfo CreateLocal ()
		{
			using (Stream stream = GetMonoTouchData (null)) {
				return BuildFromStream (xamarin_timezone_get_local_name (), stream);
			}
		}

		static TimeZoneInfo FindSystemTimeZoneByIdCore (string id)
		{
			using (Stream stream = GetMonoTouchData (id)) {
				return BuildFromStream (id, stream);
			}
		}

		static void GetSystemTimeZonesCore (List<TimeZoneInfo> systemTimeZones)
		{
			foreach (string name in GetMonoTouchNames ()) {
				using (Stream stream = GetMonoTouchData (name, false)) {
					if (stream == null)
						continue;
					systemTimeZones.Add (BuildFromStream (name, stream));
				}
			}
		}

		[DllImport ("__Internal")]
		extern static IntPtr xamarin_timezone_get_names (ref int count);

		static ReadOnlyCollection<string> GetMonoTouchNames ()
		{
			int count = 0;
			IntPtr array = xamarin_timezone_get_names (ref count);
			string [] names = new string [count];
			for (int i = 0, offset = 0; i < count; i++, offset += IntPtr.Size) {
				IntPtr p = Marshal.ReadIntPtr (array, offset);
				names [i] = Marshal.PtrToStringAnsi (p);
				Marshal.FreeHGlobal (p);
			}
			Marshal.FreeHGlobal (array);
			return new ReadOnlyCollection<string> (names);
		}

		[DllImport ("__Internal")]
		extern static IntPtr xamarin_timezone_get_data (string name, ref int size);

		static Stream GetMonoTouchData (string name, bool throw_on_error = true)
		{
			int size = 0;
			IntPtr data = xamarin_timezone_get_data (name, ref size);
			if (size <= 0) {
				if (throw_on_error)
					throw new TimeZoneNotFoundException (name);
				return null;
			}

			unsafe {
				return new HGlobalUnmanagedMemoryStream ((byte*) data, size, data);
			}
		}
	}
}

#endif

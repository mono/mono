#if WASM

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace System {

	public partial class TimeZoneInfo {

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static string xamarin_timezone_get_local_name ();

		static TimeZoneInfo CreateLocal ()
		{
			using (Stream stream = GetMonoWasmData (null)) {
				return BuildFromStream (xamarin_timezone_get_local_name (), stream);
			}
		}

		static TimeZoneInfo FindSystemTimeZoneByIdCore (string id)
		{
			using (Stream stream = GetMonoWasmData (id)) {
			 	return BuildFromStream (id, stream);
			}
		}

		static void GetSystemTimeZonesCore (List<TimeZoneInfo> systemTimeZones)
		{
			foreach (string name in GetMonoWasmNames ()) {
				using (Stream stream = GetMonoWasmData (name, false)) {
					if (stream == null)
						continue;
					systemTimeZones.Add (BuildFromStream (name, stream));
				}
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static IntPtr xamarin_timezone_get_names (ref int count);

		static ReadOnlyCollection<string> GetMonoWasmNames ()
		{
			int count = 0;
			IntPtr array = xamarin_timezone_get_names (ref count);
			if (count > 0)
			{
				string [] names = new string [count];
				for (int i = 0, offset = 0; i < count; i++, offset += IntPtr.Size) {
					IntPtr p = Marshal.ReadIntPtr (array, offset);
					names [i] = Marshal.PtrToStringAnsi (p);
					Marshal.FreeHGlobal (p);
				}
				Marshal.FreeHGlobal (array);
				return new ReadOnlyCollection<string> (names);
			}

			return new ReadOnlyCollection<string> (new string[0]);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static IntPtr xamarin_timezone_get_data (string name, ref int size);

		static Stream GetMonoWasmData (string name, bool throw_on_error = true)
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


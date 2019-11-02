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
		extern static IntPtr mono_timezone_get_local_name ();

		static string GetMonoWasmLocalName ()
		{
			IntPtr localNamePtr = mono_timezone_get_local_name ();
			var localName = Marshal.PtrToStringAnsi (localNamePtr);
			Marshal.FreeHGlobal (localNamePtr);
			return localName;
		}

		static TimeZoneInfo CreateLocal ()
		{
			using (Stream stream = GetMonoWasmData (null)) {
				return BuildFromStream (GetMonoWasmLocalName (), stream);
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
		extern static IntPtr mono_timezone_get_names (ref int count);

		static ReadOnlyCollection<string> GetMonoWasmNames ()
		{
			int count = 0;
			IntPtr array = mono_timezone_get_names (ref count);
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
		unsafe extern static IntPtr mono_timezone_get_data (char* name, int name_length, ref int size);

		unsafe static Stream GetMonoWasmData (string name, bool throw_on_error = true)
		{ 
			int size = 0;
			fixed (char* fixed_name = name)
			{
				IntPtr data = mono_timezone_get_data (fixed_name, name?.Length ?? 0, ref size);
				if (size <= 0) {
					if (throw_on_error)
						throw new TimeZoneNotFoundException (name);
					return null;
				}
				return new HGlobalUnmanagedMemoryStream ((byte*) data, size, data);
			}
		}
	}
}

#endif


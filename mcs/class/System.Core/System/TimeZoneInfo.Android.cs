/*
 * System.TimeZoneInfo Android Support
 *
 * Author(s)
 * 	Jonathan Pryor  <jpryor@novell.com>
 * 	The Android Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#if (INSIDE_CORLIB && MONODROID)

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace System {

	partial class TimeZoneInfo {

		/*
		 * Android Timezone support infrastructure.
		 *
		 * This is a C# port of org.apache.harmony.luni.internal.util.ZoneInfoDB:
		 *
		 *    http://android.git.kernel.org/?p=platform/libcore.git;a=blob;f=luni/src/main/java/org/apache/harmony/luni/internal/util/ZoneInfoDB.java;h=3e7bdc3a952b24da535806d434a3a27690feae26;hb=HEAD
		 *
		 * From the ZoneInfoDB source:
		 *
		 *    However, to conserve disk space the data for all time zones are 
		 *    concatenated into a single file, and a second file is used to indicate 
		 *    the starting position of each time zone record.  A third file indicates
		 *    the version of the zoneinfo databse used to generate the data.
		 *
		 * which succinctly describes why we can't just use the LIBC implementation in
		 * TimeZoneInfo.cs -- the "standard Unixy" directory structure is NOT used.
		 */
		static class ZoneInfoDB {
			const int TimeZoneNameLength  = 40;
			const int TimeZoneIntSize     = 4;

			static readonly string ZoneDirectoryName  = Environment.GetEnvironmentVariable ("ANDROID_ROOT") + "/usr/share/zoneinfo/";
			static readonly string ZoneFileName       = ZoneDirectoryName + "zoneinfo.dat";
			static readonly string IndexFileName      = ZoneDirectoryName + "zoneinfo.idx";
			const           string DefaultVersion     = "2007h";
			static readonly string VersionFileName    = ZoneDirectoryName + "zoneinfo.version";

			static readonly object _lock = new object ();

			static readonly string    version;
			static readonly string[]  names;
			static readonly int[]     starts;
			static readonly int[]     lengths;
			static readonly int[]     offsets;

			static ZoneInfoDB ()
			{
				try {
					version = ReadVersion ();
				} catch {
					version = DefaultVersion;
				}

				try {
					ReadDatabase (out names, out starts, out lengths, out offsets);
				} catch {
					names   = new string [0];
					starts  = new int [0];
					lengths = new int [0];
					offsets = new int [0];
				}
			}

			static string ReadVersion ()
			{
				using (var file = new StreamReader (VersionFileName, Encoding.GetEncoding ("iso-8859-1"))) {
					return file.ReadToEnd ().Trim ();
				}
			}

			static void ReadDatabase (out string[] names, out int[] starts, out int[] lengths, out int[] offsets)
			{
				using (var file = File.OpenRead (IndexFileName)) {
					var nbuf = new byte [TimeZoneNameLength];

					int numEntries = (int) (file.Length / (TimeZoneNameLength + 3*TimeZoneIntSize));

					char[]  namebuf = new char [TimeZoneNameLength];

					names   = new string [numEntries];
					starts  = new int [numEntries];
					lengths = new int [numEntries];
					offsets = new int [numEntries];

					for (int i = 0; i < numEntries; ++i) {
						Fill (file, nbuf, nbuf.Length);
						int namelen;
						for (namelen = 0; namelen < nbuf.Length; ++namelen) {
							if (nbuf [namelen] == '\0')
								break;
							namebuf [namelen] = (char) (nbuf [namelen] & 0xFF);
						}

						names   [i] = new string (namebuf, 0, namelen);
						starts  [i] = ReadInt32 (file, nbuf);
						lengths [i] = ReadInt32 (file, nbuf);
						offsets [i] = ReadInt32 (file, nbuf);
					}
				}
			}

			static void Fill (Stream stream, byte[] nbuf, int required)
			{
				int read, offset = 0;
				while (offset < required && (read = stream.Read (nbuf, offset, required - offset)) > 0)
					offset += read;
				if (read != required)
					throw new EndOfStreamException ("Needed to read " + required + " bytes; read " + read + " bytes");
			}

			// From java.io.RandomAccessFioe.readInt(), as we need to use the same
			// byte ordering as Java uses.
			static int ReadInt32 (Stream stream, byte[] nbuf)
			{
				Fill (stream, nbuf, 4);
				return ((nbuf [0] & 0xff) << 24) + ((nbuf [1] & 0xff) << 16) +
					((nbuf [2] & 0xff) << 8) + (nbuf [3] & 0xff);
			}

			internal static string Version {
				get {return version;}
			}

			internal static IEnumerable<string> GetAvailableIds ()
			{
				return GetAvailableIds (0, false);
			}

			internal static IEnumerable<string> GetAvailableIds (int rawOffset)
			{
				return GetAvailableIds (rawOffset, true);
			}

			static IEnumerable<string> GetAvailableIds (int rawOffset, bool checkOffset)
			{
				for (int i = 0; i < offsets.Length; ++i) {
					if (!checkOffset || offsets [i] == rawOffset)
						yield return names [i];
				}
			}

			static TimeZoneInfo _GetTimeZone (string name)
			{
				int start, length;
				using (var stream = GetTimeZoneData (name, out start, out length)) {
					if (stream == null)
						return null;
					byte[] buf = new byte [length];
					Fill (stream, buf, buf.Length);
					return TimeZoneInfo.ParseTZBuffer (name, buf, length);
				}
			}

			static FileStream GetTimeZoneData (string name, out int start, out int length)
			{
				var f = new FileInfo (Path.Combine (ZoneDirectoryName, name));
				if (f.Exists) {
					start   = 0;
					length  = (int) f.Length;
					return f.OpenRead ();
				}

				start = length = 0;

				int i = Array.BinarySearch (names, name, StringComparer.Ordinal);
				if (i < 0)
					return null;

				start   = starts [i];
				length  = lengths [i];

				var stream = File.OpenRead (ZoneFileName);
				stream.Seek (start, SeekOrigin.Begin);

				return stream;
			}

			internal static TimeZoneInfo GetTimeZone (string id)
			{
				if (id != null) {
					if (id == "GMT" || id == "UTC")
						return new TimeZoneInfo (id, TimeSpan.FromSeconds (0), id, id, id, null, true);
					if (id.StartsWith ("GMT"))
						return new TimeZoneInfo (id,
								TimeSpan.FromSeconds (ParseNumericZone (id)),
								id, id, id, null, true);
				}

				try {
					return _GetTimeZone (id);
				} catch (Exception e) {
					return null;
				}
			}

			static int ParseNumericZone (string name)
			{
				if (name == null || !name.StartsWith ("GMT") || name.Length <= 3)
					return 0;

				int sign;
				if (name [3] == '+')
					sign = 1;
				else if (name [3] == '-')
					sign = -1;
				else
					return 0;

				int where;
				int hour = 0;
				bool colon = false;
				for (where = 4; where < name.Length; where++) {
					char c = name [where];

					if (c == ':') {
						where++;
						colon = true;
						break;
					}

					if (c >= '0' && c <= '9')
						hour = hour * 10 + c - '0';
					else
						return 0;
				}

				int min = 0;
				for (; where < name.Length; where++) {
					char c = name [where];

					if (c >= '0' && c <= '9')
						min = min * 10 + c - '0';
					else
						return 0;
				}

				if (colon)
					return sign * (hour * 60 + min) * 60;
				else if (hour >= 100)
					return sign * ((hour / 100) * 60 + (hour % 100)) * 60;
				else
					return sign * (hour * 60) * 60;
			}

			static TimeZoneInfo defaultZone;
			internal static TimeZoneInfo Default {
				get {
					lock (_lock) {
						if (defaultZone != null)
							return defaultZone;
						return defaultZone = GetTimeZone (GetDefaultTimeZoneName ());
					}
				}
			}

			// <sys/system_properties.h>
			[DllImport ("/system/lib/libc.so")]
			static extern int __system_property_get (string name, StringBuilder value);

			const int MaxPropertyNameLength   = 32; // <sys/system_properties.h>
			const int MaxPropertyValueLength  = 92; // <sys/system_properties.h>

			static string GetDefaultTimeZoneName ()
			{
				var buf = new StringBuilder (MaxPropertyValueLength + 1);
				int n = __system_property_get ("persist.sys.timezone", buf);
				if (n > 0)
					return buf.ToString ();
				return null;
			}

#if SELF_TEST
			/*
			 * Compile:
			 *    mcs  /out:tzi.exe "/d:INSIDE_CORLIB;MONODROID;NET_4_0;LIBC;SELF_TEST" System/TimeZone*.cs ../../build/common/Consts.cs
			 * Prep:
			 *    mkdir -p usr/share/zoneinfo
			 *    android_root=`adb shell echo '$ANDROID_ROOT' | tr -d "\r"`
			 *    adb pull $android_root/usr/share/zoneinfo usr/share/zoneinfo
			 * Run:
			 *    ANDROID_ROOT=`pwd` mono tzi.exe
			 */
			static void Main (string[] args)
			{
				Console.WriteLine ("Version: {0}", version);
				for (int i = 0; i < names.Length; ++i) {
					Console.Write ("{0,3}\tname={1,-40} start={2,-10} length={3,-4} offset=0x{4,8}",
							i, names [i], starts [i], lengths [i], offsets [i].ToString ("x8"));
					try {
						TimeZoneInfo zone = _GetTimeZone (names [i]);
						if (zone != null)
							Console.Write (" {0}", zone);
						else {
							Console.Write (" ERROR:null Index? {0}",
									Array.BinarySearch (names, names [i], StringComparer.Ordinal));
						}
					} catch (Exception e) {
						Console.WriteLine ();
						Console.Write ("ERROR: {0}", e);
					}
					Console.WriteLine ();
				}
			}
#endif
		}
	}
}

#endif // MONODROID


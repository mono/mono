
/*
 * System.TimeZoneInfo
 *
 * Author(s)
 * 	Stephane Delcroix <stephane@delcroix.org>
 *
 * Copyright 2011 Xamarin Inc.
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Text;
using System.Globalization;
using System.IO;

using Microsoft.Win32;

namespace System
{
	partial class TimeZoneInfo
	{
		TimeSpan baseUtcOffset;
		public TimeSpan BaseUtcOffset {
			get { return baseUtcOffset; }
		}

		string daylightDisplayName;
		public string DaylightName {
			get { 
				return supportsDaylightSavingTime
					? daylightDisplayName
					: string.Empty;
			}
		}

		string displayName;
		public string DisplayName {
			get { return displayName; }
		}

		string id;
		public string Id {
			get { return id; }
		}

		static TimeZoneInfo local;
		public static TimeZoneInfo Local {
			get { 
				var l = local;
				if (l == null) {
					l = CreateLocal ();
					if (l == null)
						throw new TimeZoneNotFoundException ();

					if (Interlocked.CompareExchange (ref local, l, null) != null)
						l = local;
				}

				return l;
			}
		}

		/*
			TimeZone transitions are stored when there is a change on the base offset.
		*/
		private List<KeyValuePair<DateTime, TimeType>> transitions;

		private static bool readlinkNotFound;

		[DllImport ("libc")]
		private static extern int readlink (string path, byte[] buffer, int buflen);

		private static string readlink (string path)
		{
			if (readlinkNotFound)
				return null;

			byte[] buf = new byte [512];
			int ret;

			try {
				ret = readlink (path, buf, buf.Length);
			} catch (DllNotFoundException) {
				readlinkNotFound = true;
				return null;
			} catch (EntryPointNotFoundException) {
				readlinkNotFound = true;
				return null;
			}

			if (ret == -1) return null;
			char[] cbuf = new char [512];
			int chars = System.Text.Encoding.Default.GetChars (buf, 0, ret, cbuf, 0);
			return new String (cbuf, 0, chars);
		}

		private static bool TryGetNameFromPath (string path, out string name)
		{
			name = null;
			var linkPath = readlink (path);
			if (linkPath != null) {
				if (Path.IsPathRooted(linkPath))
					path = linkPath;
				else
					path = Path.Combine(Path.GetDirectoryName(path), linkPath);
			}

			path = Path.GetFullPath (path);

			if (string.IsNullOrEmpty (TimeZoneDirectory))
				return false;

			var baseDir = TimeZoneDirectory;
			if (baseDir [baseDir.Length-1] != Path.DirectorySeparatorChar)
				baseDir += Path.DirectorySeparatorChar;

			if (!path.StartsWith (baseDir, StringComparison.InvariantCulture))
				return false;

			name = path.Substring (baseDir.Length);
			if (name == "localtime")
				name = "Local";

			return true;
		}

#if (!MONODROID && !MONOTOUCH && !XAMMAC) || MOBILE_DESKTOP_HOST
#if WASM
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static void mono_timezone_get_local_name (ref string name);
#endif
		static TimeZoneInfo CreateLocal ()
		{
#if WIN_PLATFORM
			if (IsWindows && LocalZoneKey != null) {
				string name = (string)LocalZoneKey.GetValue ("TimeZoneKeyName");
				if (name == null)
					name = (string)LocalZoneKey.GetValue ("StandardName"); // windows xp
				name = TrimSpecial (name);
				if (name != null)
					return TimeZoneInfo.FindSystemTimeZoneById (name);
			} else if (IsWindows) {
				return GetLocalTimeZoneInfoWinRTFallback ();
			}
#endif
#if WASM
			string localName = null;
			mono_timezone_get_local_name (ref localName);
			try {
				return FindSystemTimeZoneByFileName (localName, Path.Combine (TimeZoneDirectory, localName));
			} catch {
				return Utc;
			}
#else		
			var tz = Environment.GetEnvironmentVariable ("TZ");
			if (tz != null) {
				if (tz == String.Empty)
					return Utc;
				try {
					return FindSystemTimeZoneByFileName (tz, Path.Combine (TimeZoneDirectory, tz));
				} catch {
					return Utc;
				}
			}

			var tzFilePaths = new string [] {
				"/etc/localtime",
				Path.Combine (TimeZoneDirectory, "localtime")};

			foreach (var tzFilePath in tzFilePaths) {
				try {
					string tzName = null;
					if (!TryGetNameFromPath (tzFilePath, out tzName))
						tzName = "Local";
					return FindSystemTimeZoneByFileName (tzName, tzFilePath);
				} catch (TimeZoneNotFoundException) {
					continue;
				}
			}

			return Utc;
#endif			
		}

		static TimeZoneInfo FindSystemTimeZoneByIdCore (string id)
		{
#if LIBC
			string filepath = Path.Combine (TimeZoneDirectory, id);
			return FindSystemTimeZoneByFileName (id, filepath);
#else
			throw new NotImplementedException ();
#endif
		}

		static void GetSystemTimeZonesCore (List<TimeZoneInfo> systemTimeZones)
		{
#if WIN_PLATFORM
			if (TimeZoneKey != null) {
				foreach (string id in TimeZoneKey.GetSubKeyNames ()) {
					using (RegistryKey subkey = TimeZoneKey.OpenSubKey (id))
					{
						if (subkey == null || subkey.GetValue ("TZI") == null)
							continue;
					}
					systemTimeZones.Add (FindSystemTimeZoneById (id));
				}

				return;
			} else if (IsWindows) {
				systemTimeZones.AddRange (GetSystemTimeZonesWinRTFallback ());
				return;
			}
#endif

#if LIBC
			string[] continents = new string [] {"Africa", "America", "Antarctica", "Arctic", "Asia", "Atlantic", "Australia", "Brazil", "Canada", "Chile", "Europe", "Indian", "Mexico", "Mideast", "Pacific", "US"};
			foreach (string continent in continents) {
				try {
					foreach (string zonepath in Directory.GetFiles (Path.Combine (TimeZoneDirectory, continent))) {
						try {
							string id = String.Format ("{0}/{1}", continent, Path.GetFileName (zonepath));
							systemTimeZones.Add (FindSystemTimeZoneById (id));
						} catch (ArgumentNullException) {
						} catch (TimeZoneNotFoundException) {
						} catch (InvalidTimeZoneException) {
						} catch (Exception) {
							throw;
						}
					}
				} catch {}
			}
#else
			throw new NotImplementedException ("This method is not implemented for this platform");
#endif
		}
#endif // !MONODROID && !MONOTOUCH && !XAMMAC && !WASM

		string standardDisplayName;
		public string StandardName {
			get { return standardDisplayName; }
		}

		bool supportsDaylightSavingTime;
		public bool SupportsDaylightSavingTime {
			get  { return supportsDaylightSavingTime; }
		}

		static TimeZoneInfo utc;
		public static TimeZoneInfo Utc {
			get {
				if (utc == null)
					utc = CreateCustomTimeZone ("UTC", new TimeSpan (0), "UTC", "UTC");
				return utc;
			}
		}
#if LIBC
#if WASM
		const string DefaultTimeZoneDirectory = "/zoneinfo";
#else		
		const string DefaultTimeZoneDirectory = "/usr/share/zoneinfo";
#endif
		static string timeZoneDirectory;
		static string TimeZoneDirectory {
			get {
				if (timeZoneDirectory == null)
					timeZoneDirectory = readlink (DefaultTimeZoneDirectory) ?? DefaultTimeZoneDirectory;
				return timeZoneDirectory;
			}
			set {
				ClearCachedData ();
				timeZoneDirectory = value;
			}
		}
#endif
		private AdjustmentRule [] adjustmentRules;

#if (!MOBILE || !FULL_AOT_DESKTOP || WIN_PLATFORM) && !XAMMAC_4_5
		/// <summary>
		/// Determine whether windows of not (taken Stephane Delcroix's code)
		/// </summary>
		private static bool IsWindows
		{
			get {
				int platform = (int) Environment.OSVersion.Platform;
				return ((platform != 4) && (platform != 6) && (platform != 128));
			}
		}
		
		/// <summary>
		/// Needed to trim misc garbage in MS registry keys
		/// </summary>
		private static string TrimSpecial (string str)
		{
			if (str == null)
				return str;
			var Istart = 0;
			while (Istart < str.Length && !char.IsLetterOrDigit(str[Istart])) Istart++;
			var Iend = str.Length - 1;
			while (Iend > Istart && !char.IsLetterOrDigit(str[Iend]) && str[Iend] != ')') // zone name can include parentheses like "Central Standard Time (Mexico)"
				Iend--;
			
			return str.Substring (Istart, Iend-Istart+1);
		}

#if !FULL_AOT_DESKTOP || WIN_PLATFORM
		static RegistryKey timeZoneKey;
		static RegistryKey TimeZoneKey {
			get {
				if (timeZoneKey != null)
					return timeZoneKey;
				if (!IsWindows)
					return null;
				
				try {
					return timeZoneKey = Registry.LocalMachine.OpenSubKey (
						"SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Time Zones",
						false);
				} catch {
					return null;
				}
			}
		}
		
		static RegistryKey localZoneKey;
		static RegistryKey LocalZoneKey {
			get {
				if (localZoneKey != null)
					return localZoneKey;
				
				if (!IsWindows)
					return null;
				
				try {
					return localZoneKey = Registry.LocalMachine.OpenSubKey (
						"SYSTEM\\CurrentControlSet\\Control\\TimeZoneInformation", false);
				} catch {
					return null;
				}
			}
		}
#endif
#endif // !MOBILE || !FULL_AOT_DESKTOP || WIN_PLATFORM

		private static bool TryAddTicks (DateTime date, long ticks, out DateTime result, DateTimeKind kind = DateTimeKind.Unspecified)
		{
			var resultTicks = date.Ticks + ticks;
			if (resultTicks < DateTime.MinValue.Ticks) {
				result = DateTime.SpecifyKind (DateTime.MinValue, kind);
				return false;
			}

			if (resultTicks > DateTime.MaxValue.Ticks) {
				result = DateTime.SpecifyKind (DateTime.MaxValue, kind);
				return false;
			}

			result = new DateTime (resultTicks, kind);
			return true;
		}

		public static void ClearCachedData ()
		{
			local = null;
			utc = null;
			systemTimeZones = null;
		}

		public static DateTime ConvertTime (DateTime dateTime, TimeZoneInfo destinationTimeZone)
		{
			return ConvertTime (dateTime, dateTime.Kind == DateTimeKind.Utc ? TimeZoneInfo.Utc : TimeZoneInfo.Local, destinationTimeZone);
		}

		public static DateTime ConvertTime (DateTime dateTime, TimeZoneInfo sourceTimeZone, TimeZoneInfo destinationTimeZone)
		{
			if (sourceTimeZone == null)
				throw new ArgumentNullException ("sourceTimeZone");

			if (destinationTimeZone == null)
				throw new ArgumentNullException ("destinationTimeZone");
			
			if (dateTime.Kind == DateTimeKind.Local && sourceTimeZone != TimeZoneInfo.Local)
				throw new ArgumentException ("Kind property of dateTime is Local but the sourceTimeZone does not equal TimeZoneInfo.Local");

			if (dateTime.Kind == DateTimeKind.Utc && sourceTimeZone != TimeZoneInfo.Utc)
				throw new ArgumentException ("Kind property of dateTime is Utc but the sourceTimeZone does not equal TimeZoneInfo.Utc");
			
			if (sourceTimeZone.IsInvalidTime (dateTime))
				throw new ArgumentException ("dateTime parameter is an invalid time");

			if (dateTime.Kind == DateTimeKind.Local && sourceTimeZone == TimeZoneInfo.Local && destinationTimeZone == TimeZoneInfo.Local)
				return dateTime;

			DateTime utc = ConvertTimeToUtc (dateTime, sourceTimeZone);

			if (destinationTimeZone != TimeZoneInfo.Utc) {
				utc = ConvertTimeFromUtc (utc, destinationTimeZone);
				if (dateTime.Kind == DateTimeKind.Unspecified)
					return DateTime.SpecifyKind (utc, DateTimeKind.Unspecified);
			}
			
			return utc;
		}

		public static DateTimeOffset ConvertTime(DateTimeOffset dateTimeOffset, TimeZoneInfo destinationTimeZone) 
		{
			if (destinationTimeZone == null) 
				throw new ArgumentNullException("destinationTimeZone");

			var utcDateTime = dateTimeOffset.UtcDateTime;

			bool isDst;
			var utcOffset =  destinationTimeZone.GetUtcOffset(utcDateTime, out isDst);

			return new DateTimeOffset(DateTime.SpecifyKind(utcDateTime, DateTimeKind.Unspecified) + utcOffset, utcOffset);
		}

		public static DateTime ConvertTimeBySystemTimeZoneId (DateTime dateTime, string destinationTimeZoneId)
		{
			return ConvertTime (dateTime, FindSystemTimeZoneById (destinationTimeZoneId));
		}

		public static DateTime ConvertTimeBySystemTimeZoneId (DateTime dateTime, string sourceTimeZoneId, string destinationTimeZoneId)
		{
			TimeZoneInfo source_tz;
			if (dateTime.Kind == DateTimeKind.Utc && sourceTimeZoneId == TimeZoneInfo.Utc.Id) {
				source_tz = Utc;
			} else {
				source_tz = FindSystemTimeZoneById (sourceTimeZoneId);
			}

			return ConvertTime (dateTime, source_tz, FindSystemTimeZoneById (destinationTimeZoneId));
		}

		public static DateTimeOffset ConvertTimeBySystemTimeZoneId (DateTimeOffset dateTimeOffset, string destinationTimeZoneId)
		{
			return ConvertTime (dateTimeOffset, FindSystemTimeZoneById (destinationTimeZoneId));
		}

		private DateTime ConvertTimeFromUtc (DateTime dateTime)
		{
			if (dateTime.Kind == DateTimeKind.Local)
				throw new ArgumentException ("Kind property of dateTime is Local");

			if (this == TimeZoneInfo.Utc)
				return DateTime.SpecifyKind (dateTime, DateTimeKind.Utc);

			var utcOffset = GetUtcOffset (dateTime);

			var kind = (this == TimeZoneInfo.Local)? DateTimeKind.Local : DateTimeKind.Unspecified;

			DateTime result;
			if (!TryAddTicks (dateTime, utcOffset.Ticks, out result, kind))
				return DateTime.SpecifyKind (DateTime.MaxValue, kind);

			return result;
		}

		public static DateTime ConvertTimeFromUtc (DateTime dateTime, TimeZoneInfo destinationTimeZone)
		{
			if (destinationTimeZone == null)
				throw new ArgumentNullException ("destinationTimeZone");

			return destinationTimeZone.ConvertTimeFromUtc (dateTime);
		}

		public static DateTime ConvertTimeToUtc (DateTime dateTime)
		{
			if (dateTime.Kind == DateTimeKind.Utc)
				return dateTime;

			return ConvertTimeToUtc (dateTime, TimeZoneInfo.Local);
		}

		static internal DateTime ConvertTimeToUtc(DateTime dateTime, TimeZoneInfoOptions flags)
		{
			return ConvertTimeToUtc (dateTime, TimeZoneInfo.Local, flags);
 		}

		public static DateTime ConvertTimeToUtc (DateTime dateTime, TimeZoneInfo sourceTimeZone)
		{
			return ConvertTimeToUtc (dateTime, sourceTimeZone, TimeZoneInfoOptions.None);
		}

		static DateTime ConvertTimeToUtc (DateTime dateTime, TimeZoneInfo sourceTimeZone, TimeZoneInfoOptions flags)
		{
			if ((flags & TimeZoneInfoOptions.NoThrowOnInvalidTime) == 0) {
				if (sourceTimeZone == null)
					throw new ArgumentNullException ("sourceTimeZone");

				if (dateTime.Kind == DateTimeKind.Utc && sourceTimeZone != TimeZoneInfo.Utc)
					throw new ArgumentException ("Kind property of dateTime is Utc but the sourceTimeZone does not equal TimeZoneInfo.Utc");

				if (dateTime.Kind == DateTimeKind.Local && sourceTimeZone != TimeZoneInfo.Local)
					throw new ArgumentException ("Kind property of dateTime is Local but the sourceTimeZone does not equal TimeZoneInfo.Local");

				if (sourceTimeZone.IsInvalidTime (dateTime))
					throw new ArgumentException ("dateTime parameter is an invalid time");
			}

			if (dateTime.Kind == DateTimeKind.Utc)
				return dateTime;

			bool isDst;
			var utcOffset = sourceTimeZone.GetUtcOffset (dateTime, out isDst);

			DateTime utcDateTime;
			TryAddTicks (dateTime, -utcOffset.Ticks, out utcDateTime, DateTimeKind.Utc);
			return utcDateTime;
		}

		static internal TimeSpan GetDateTimeNowUtcOffsetFromUtc(DateTime time, out Boolean isAmbiguousLocalDst)
		{
			bool isDaylightSavings;
			return GetUtcOffsetFromUtc(time, TimeZoneInfo.Local, out isDaylightSavings, out isAmbiguousLocalDst);
		}

		public static TimeZoneInfo CreateCustomTimeZone (string id, TimeSpan baseUtcOffset, string displayName, string standardDisplayName) 
		{
			return CreateCustomTimeZone (id, baseUtcOffset, displayName, standardDisplayName, null, null, true);
		}

		public static TimeZoneInfo CreateCustomTimeZone (string id, TimeSpan baseUtcOffset, string displayName, string standardDisplayName, string daylightDisplayName, TimeZoneInfo.AdjustmentRule [] adjustmentRules)
		{
			return CreateCustomTimeZone (id, baseUtcOffset, displayName, standardDisplayName, daylightDisplayName, adjustmentRules, false);
		}

		public static TimeZoneInfo CreateCustomTimeZone ( string id, TimeSpan baseUtcOffset, string displayName, string standardDisplayName, string daylightDisplayName, TimeZoneInfo.AdjustmentRule [] adjustmentRules, bool disableDaylightSavingTime)
		{
			return new TimeZoneInfo (id, baseUtcOffset, displayName, standardDisplayName, daylightDisplayName, adjustmentRules, disableDaylightSavingTime);
		}

		public override bool Equals (object obj)
		{
			return Equals (obj as TimeZoneInfo);
		}

		public bool Equals (TimeZoneInfo other)
		{
			if (other == null)
				return false;

			return other.Id == this.Id && HasSameRules (other);
		}

		public static TimeZoneInfo FindSystemTimeZoneById (string id)
		{
			//FIXME: this method should check for cached values in systemTimeZones
			if (id == null)
				throw new ArgumentNullException ("id");
#if WIN_PLATFORM
			if (TimeZoneKey != null)
			{
				if (id == "Coordinated Universal Time")
					id = "UTC"; //windows xp exception for "StandardName" property
				RegistryKey key = TimeZoneKey.OpenSubKey (id, false);
				if (key == null)
					throw new TimeZoneNotFoundException ();
				return FromRegistryKey(id, key);
			} else if (IsWindows) {
				return FindSystemTimeZoneByIdWinRTFallback (id);
			}
#endif
			// Local requires special logic that already exists in the Local property (bug #326)
			if (id == "Local")
				return Local;

			return FindSystemTimeZoneByIdCore (id);
		}

#if LIBC
		private static TimeZoneInfo FindSystemTimeZoneByFileName (string id, string filepath)
		{
			FileStream stream = null;
			try {
				stream = File.OpenRead (filepath);	
			} catch (Exception ex) {
				throw new TimeZoneNotFoundException ("Couldn't read time zone file " + filepath, ex);
			}
			try {
				return BuildFromStream (id, stream);
			} finally {
				if (stream != null)
					stream.Dispose();
			}
		}
#endif

#if WIN_PLATFORM
		private static TimeZoneInfo FromRegistryKey (string id, RegistryKey key)
		{
			byte [] reg_tzi = (byte []) key.GetValue ("TZI");

			if (reg_tzi == null)
				throw new InvalidTimeZoneException ();

			int bias = BitConverter.ToInt32 (reg_tzi, 0);
			TimeSpan baseUtcOffset = new TimeSpan (0, -bias, 0);

			string display_name = (string) key.GetValue ("Display");
			string standard_name = (string) key.GetValue ("Std");
			string daylight_name = (string) key.GetValue ("Dlt");

			List<AdjustmentRule> adjustmentRules = new List<AdjustmentRule> ();

			RegistryKey dst_key = key.OpenSubKey ("Dynamic DST", false);
			if (dst_key != null) {
				int first_year = (int) dst_key.GetValue ("FirstEntry");
				int last_year = (int) dst_key.GetValue ("LastEntry");
				int year;

				for (year=first_year; year<=last_year; year++) {
					byte [] dst_tzi = (byte []) dst_key.GetValue (year.ToString ());
					if (dst_tzi != null) {
						int start_year = year == first_year ? 1 : year;
						int end_year = year == last_year ? 9999 : year;
						ParseRegTzi(adjustmentRules, start_year, end_year, dst_tzi);
					}
				}
			}
			else
				ParseRegTzi(adjustmentRules, 1, 9999, reg_tzi);

			return CreateCustomTimeZone (id, baseUtcOffset, display_name, standard_name, daylight_name, ValidateRules (adjustmentRules));
		}

		private static void ParseRegTzi (List<AdjustmentRule> adjustmentRules, int start_year, int end_year, byte [] buffer)
		{
			//int standard_bias = BitConverter.ToInt32 (buffer, 4); /* not sure how to handle this */
			int daylight_bias = BitConverter.ToInt32 (buffer, 8);

			int standard_year = BitConverter.ToInt16 (buffer, 12);
			int standard_month = BitConverter.ToInt16 (buffer, 14);
			int standard_dayofweek = BitConverter.ToInt16 (buffer, 16);
			int standard_day = BitConverter.ToInt16 (buffer, 18);
			int standard_hour = BitConverter.ToInt16 (buffer, 20);
			int standard_minute = BitConverter.ToInt16 (buffer, 22);
			int standard_second = BitConverter.ToInt16 (buffer, 24);
			int standard_millisecond = BitConverter.ToInt16 (buffer, 26);

			int daylight_year = BitConverter.ToInt16 (buffer, 28);
			int daylight_month = BitConverter.ToInt16 (buffer, 30);
			int daylight_dayofweek = BitConverter.ToInt16 (buffer, 32);
			int daylight_day = BitConverter.ToInt16 (buffer, 34);
			int daylight_hour = BitConverter.ToInt16 (buffer, 36);
			int daylight_minute = BitConverter.ToInt16 (buffer, 38);
			int daylight_second = BitConverter.ToInt16 (buffer, 40);
			int daylight_millisecond = BitConverter.ToInt16 (buffer, 42);

			if (standard_month == 0 || daylight_month == 0)
				return;

			DateTime start_date;
			DateTime start_timeofday = new DateTime (1, 1, 1, daylight_hour, daylight_minute, daylight_second, daylight_millisecond);
			TransitionTime start_transition_time;

			start_date = new DateTime (start_year, 1, 1);
			if (daylight_year == 0) {
				start_transition_time = TransitionTime.CreateFloatingDateRule (
					start_timeofday, daylight_month, daylight_day,
					(DayOfWeek) daylight_dayofweek);
			}
			else {
				start_transition_time = TransitionTime.CreateFixedDateRule (
					start_timeofday, daylight_month, daylight_day);
			}

			DateTime end_date;
			DateTime end_timeofday = new DateTime (1, 1, 1, standard_hour, standard_minute, standard_second, standard_millisecond);
			TransitionTime end_transition_time;

			end_date = new DateTime (end_year, 12, 31);
			if (standard_year == 0) {
				end_transition_time = TransitionTime.CreateFloatingDateRule (
					end_timeofday, standard_month, standard_day,
					(DayOfWeek) standard_dayofweek);
			}
			else {
				end_transition_time = TransitionTime.CreateFixedDateRule (
					end_timeofday, standard_month, standard_day);
			}

			TimeSpan daylight_delta = new TimeSpan(0, -daylight_bias, 0);

			adjustmentRules.Add (AdjustmentRule.CreateAdjustmentRule (
				start_date, end_date, daylight_delta,
				start_transition_time, end_transition_time));
		}
#endif

		public AdjustmentRule [] GetAdjustmentRules ()
		{
			if (!supportsDaylightSavingTime || adjustmentRules == null)
				return new AdjustmentRule [0];
			else
				return (AdjustmentRule []) adjustmentRules.Clone ();
		}

		public TimeSpan [] GetAmbiguousTimeOffsets (DateTime dateTime)
		{
			if (!IsAmbiguousTime (dateTime))
				throw new ArgumentException ("dateTime is not an ambiguous time");

			AdjustmentRule rule = GetApplicableRule (dateTime);
			if (rule != null)
				return new TimeSpan[] {baseUtcOffset, baseUtcOffset + rule.DaylightDelta};
			else
				return new TimeSpan[] {baseUtcOffset, baseUtcOffset};
		}

		public TimeSpan [] GetAmbiguousTimeOffsets (DateTimeOffset dateTimeOffset)
		{
			if (!IsAmbiguousTime (dateTimeOffset))
				throw new ArgumentException ("dateTimeOffset is not an ambiguous time");

			throw new NotImplementedException ();
		}

		public override int GetHashCode ()
		{
			int hash_code = Id.GetHashCode ();
			foreach (AdjustmentRule rule in GetAdjustmentRules ())
				hash_code ^= rule.GetHashCode ();
			return hash_code;
		}

		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");
			info.AddValue ("Id", id);
			info.AddValue ("DisplayName", displayName);
			info.AddValue ("StandardName", standardDisplayName);
			info.AddValue ("DaylightName", daylightDisplayName);
			info.AddValue ("BaseUtcOffset", baseUtcOffset);
			info.AddValue ("AdjustmentRules", adjustmentRules);
			info.AddValue ("SupportsDaylightSavingTime", SupportsDaylightSavingTime);
		}

		static ReadOnlyCollection<TimeZoneInfo> systemTimeZones;

		public static ReadOnlyCollection<TimeZoneInfo> GetSystemTimeZones ()
		{
			if (systemTimeZones == null) {
				var tz = new List<TimeZoneInfo> ();
				GetSystemTimeZonesCore (tz);
				Interlocked.CompareExchange (ref systemTimeZones, new ReadOnlyCollection<TimeZoneInfo> (tz), null);
			}

			return systemTimeZones;
		}

		public TimeSpan GetUtcOffset (DateTime dateTime)
		{
			bool isDST;
			return GetUtcOffset (dateTime, out isDST);
		}

		public TimeSpan GetUtcOffset (DateTimeOffset dateTimeOffset)
		{
			bool isDST;
			return GetUtcOffset (dateTimeOffset.UtcDateTime, out isDST);
		}

		private TimeSpan GetUtcOffset (DateTime dateTime, out bool isDST, bool forOffset = false)
		{
			isDST = false;

			TimeZoneInfo tz = this;
			if (dateTime.Kind == DateTimeKind.Utc)
				tz = TimeZoneInfo.Utc;

			if (dateTime.Kind == DateTimeKind.Local)
				tz = TimeZoneInfo.Local;

			bool isTzDst;
			var tzOffset = GetUtcOffsetHelper (dateTime, tz, out isTzDst, forOffset);

			if (tz == this) {
				isDST = isTzDst;
				return tzOffset;
			}

			DateTime utcDateTime;
			if (!TryAddTicks (dateTime, -tzOffset.Ticks, out utcDateTime, DateTimeKind.Utc))
				return BaseUtcOffset;

			return GetUtcOffsetHelper (utcDateTime, this, out isDST, forOffset);
		}

		// This is an helper method used by the method above, do not use this on its own.
		private static TimeSpan GetUtcOffsetHelper (DateTime dateTime, TimeZoneInfo tz, out bool isDST, bool forOffset = false)
		{
			if (dateTime.Kind == DateTimeKind.Local && tz != TimeZoneInfo.Local)
				throw new Exception ();

			isDST = false;

			if (tz == TimeZoneInfo.Utc)
				return TimeSpan.Zero;

			TimeSpan offset;
			if (tz.TryGetTransitionOffset(dateTime, out offset, out isDST, forOffset))
				return offset;

			if (dateTime.Kind == DateTimeKind.Utc) {
				var utcRule = tz.GetApplicableRule (dateTime);
				if (utcRule != null && tz.IsInDST (utcRule, dateTime)) {
					isDST = true;
					return tz.BaseUtcOffset + utcRule.DaylightDelta;
				}

				return tz.BaseUtcOffset;
			}

			DateTime stdUtcDateTime;
			if (!TryAddTicks (dateTime, -tz.BaseUtcOffset.Ticks, out stdUtcDateTime, DateTimeKind.Utc))
				return tz.BaseUtcOffset;

			var tzRule = tz.GetApplicableRule (stdUtcDateTime);

			DateTime dstUtcDateTime = DateTime.MinValue;
			if (tzRule != null) {
				if (!TryAddTicks (stdUtcDateTime, -tzRule.DaylightDelta.Ticks, out dstUtcDateTime, DateTimeKind.Utc))
					return tz.BaseUtcOffset;
			}

			if (tzRule != null && tz.IsInDST (tzRule, dateTime)) {
				// Replicate what .NET does when given a time which falls into the hour which is lost when
				// DST starts. isDST should be false and the offset should be BaseUtcOffset without the
				// DST delta while in that hour.
				if (forOffset)
					isDST = true;
				if (tz.IsInDST (tzRule, dstUtcDateTime)) {
					isDST = true;
					return tz.BaseUtcOffset + tzRule.DaylightDelta;
				} else {
					return tz.BaseUtcOffset;
				}
			}

			return tz.BaseUtcOffset;
		}

		public bool HasSameRules (TimeZoneInfo other)
		{
			if (other == null)
				throw new ArgumentNullException ("other");

			if ((this.adjustmentRules == null) != (other.adjustmentRules == null))
				return false;

			if (this.adjustmentRules == null)
      				return true;

			if (this.BaseUtcOffset != other.BaseUtcOffset)
				return false;

			if (this.adjustmentRules.Length != other.adjustmentRules.Length)
				return false;

			for (int i = 0; i < adjustmentRules.Length; i++) {
				if (! (this.adjustmentRules [i]).Equals (other.adjustmentRules [i]))
					return false;
			}
			
			return true;
		}

		public bool IsAmbiguousTime (DateTime dateTime)
		{
			if (dateTime.Kind == DateTimeKind.Local && IsInvalidTime (dateTime))
				throw new ArgumentException ("Kind is Local and time is Invalid");

			if (this == TimeZoneInfo.Utc)
				return false;
			
			if (dateTime.Kind == DateTimeKind.Utc)
				dateTime = ConvertTimeFromUtc (dateTime);

			if (dateTime.Kind == DateTimeKind.Local && this != TimeZoneInfo.Local)
				dateTime = ConvertTime (dateTime, TimeZoneInfo.Local, this);

			AdjustmentRule rule = GetApplicableRule (dateTime);
			if (rule != null) {
				DateTime tpoint = TransitionPoint (rule.DaylightTransitionEnd, dateTime.Year);
				if (dateTime > tpoint - rule.DaylightDelta && dateTime <= tpoint)
					return true;
			}
				
			return false;
		}

		public bool IsAmbiguousTime (DateTimeOffset dateTimeOffset)
		{
			throw new NotImplementedException ();
		}

		private bool IsInDST (AdjustmentRule rule, DateTime dateTime)
		{
			// Check whether we're in the dateTime year's DST period
			if (IsInDSTForYear (rule, dateTime, dateTime.Year))
				return true;

			// We might be in the dateTime previous year's DST period
			return dateTime.Year > 1 && IsInDSTForYear (rule, dateTime, dateTime.Year - 1);
		}

		bool IsInDSTForYear (AdjustmentRule rule, DateTime dateTime, int year)
		{
			DateTime DST_start = TransitionPoint (rule.DaylightTransitionStart, year);
			DateTime DST_end = TransitionPoint (rule.DaylightTransitionEnd, year + ((rule.DaylightTransitionStart.Month < rule.DaylightTransitionEnd.Month) ? 0 : 1));
			if (dateTime.Kind == DateTimeKind.Utc) {
				DST_start -= BaseUtcOffset;
				DST_end -= BaseUtcOffset;
			}
			DST_end -= rule.DaylightDelta;
			return (dateTime >= DST_start && dateTime < DST_end);
		}
		
		public bool IsDaylightSavingTime (DateTime dateTime)
		{
			if (dateTime.Kind == DateTimeKind.Local && IsInvalidTime (dateTime))
				throw new ArgumentException ("dateTime is invalid and Kind is Local");

			if (this == TimeZoneInfo.Utc)
				return false;
			
			if (!SupportsDaylightSavingTime)
				return false;

			bool isDst;
			GetUtcOffset (dateTime, out isDst);

			return isDst;
		}

		internal bool IsDaylightSavingTime (DateTime dateTime, TimeZoneInfoOptions flags)
		{
			return IsDaylightSavingTime (dateTime);
		}

		public bool IsDaylightSavingTime (DateTimeOffset dateTimeOffset)
		{
			var dateTime = dateTimeOffset.DateTime;
			
			if (dateTime.Kind == DateTimeKind.Local && IsInvalidTime (dateTime))
				throw new ArgumentException ("dateTime is invalid and Kind is Local");

			if (this == TimeZoneInfo.Utc)
				return false;
			
			if (!SupportsDaylightSavingTime)
				return false;

			bool isDst;
			GetUtcOffset (dateTime, out isDst, true);

			return isDst;
		}

		internal DaylightTime GetDaylightChanges (int year)
		{
			DateTime start = DateTime.MinValue, end = DateTime.MinValue;
			TimeSpan delta = new TimeSpan ();

			if (transitions != null) {
				end = DateTime.MaxValue;
				for (var i =  transitions.Count - 1; i >= 0; i--) {
					var pair = transitions [i];
					DateTime ttime = pair.Key;
					TimeType ttype = pair.Value;

					if (ttime.Year > year)
						continue;
					if (ttime.Year < year)
						break;

					if (ttype.IsDst) {
						// DaylightTime.Delta is relative to the current BaseUtcOffset.
						delta =  new TimeSpan (0, 0, ttype.Offset) - BaseUtcOffset;
						start = ttime;
					} else {
						end = ttime;
					}
				}

				// DaylightTime.Start is relative to the Standard time.
				if (!TryAddTicks (start, BaseUtcOffset.Ticks, out start))
					start = DateTime.MinValue;

				// DaylightTime.End is relative to the DST time.
				if (!TryAddTicks (end, BaseUtcOffset.Ticks + delta.Ticks, out end))
					end = DateTime.MinValue;
			} else {
				AdjustmentRule first = null, last = null;

				// Rule start/end dates are either very specific or very broad depending on the platform
				//   2015-10-04..2016-04-03 - Rule for a time zone in southern hemisphere on non-Windows platforms
				//   2016-03-27..2016-10-03 - Rule for a time zone in northern hemisphere on non-Windows platforms
				//   0001-01-01..9999-12-31 - Rule for a time zone on Windows

				foreach (var rule in GetAdjustmentRules ()) {
					if (rule.DateStart.Year > year || rule.DateEnd.Year < year)
						continue;
					if (rule.DateStart.Year <= year && (first == null || rule.DateStart.Year > first.DateStart.Year))
						first = rule;
					if (rule.DateEnd.Year >= year && (last == null || rule.DateEnd.Year < last.DateEnd.Year))
						last = rule;
				}

				if (first == null || last == null)
					return new DaylightTime (new DateTime (), new DateTime (), new TimeSpan ());

				start = TransitionPoint (first.DaylightTransitionStart, year);
				end = TransitionPoint (last.DaylightTransitionEnd, year);
				delta = first.DaylightDelta;
			}

			if (start == DateTime.MinValue || end == DateTime.MinValue)
				return new DaylightTime (new DateTime (), new DateTime (), new TimeSpan ());

			return new DaylightTime (start, end, delta);
		}

		public bool IsInvalidTime (DateTime dateTime)
		{
			if (dateTime.Kind == DateTimeKind.Utc)
				return false;
			if (dateTime.Kind == DateTimeKind.Local && this != Local)
				return false;

			AdjustmentRule rule = GetApplicableRule (dateTime);
			if (rule != null) {
				DateTime tpoint = TransitionPoint (rule.DaylightTransitionStart, dateTime.Year);
				if (dateTime >= tpoint && dateTime < tpoint + rule.DaylightDelta)
					return true;
			}

			return false;
		}

		void IDeserializationCallback.OnDeserialization (object sender)
		{
			try {
					TimeZoneInfo.Validate (id, baseUtcOffset, adjustmentRules);
				} catch (ArgumentException ex) {
					throw new SerializationException ("invalid serialization data", ex);
				}
 		}

		private static void Validate (string id, TimeSpan baseUtcOffset, AdjustmentRule [] adjustmentRules)
		{
			if (id == null)
				throw new ArgumentNullException ("id");

			if (id == String.Empty)
				throw new ArgumentException ("id parameter is an empty string");

			if (baseUtcOffset.Ticks % TimeSpan.TicksPerMinute != 0)
				throw new ArgumentException ("baseUtcOffset parameter does not represent a whole number of minutes");

			if (baseUtcOffset > new TimeSpan (14, 0, 0) || baseUtcOffset < new TimeSpan (-14, 0, 0))
				throw new ArgumentOutOfRangeException ("baseUtcOffset parameter is greater than 14 hours or less than -14 hours");

#if STRICT
			if (id.Length > 32)
				throw new ArgumentException ("id parameter shouldn't be longer than 32 characters");
#endif

			if (adjustmentRules != null && adjustmentRules.Length != 0) {
				AdjustmentRule prev = null;
				foreach (AdjustmentRule current in adjustmentRules) {
					if (current == null)
						throw new InvalidTimeZoneException ("one or more elements in adjustmentRules are null");

					if ((baseUtcOffset + current.DaylightDelta < new TimeSpan (-14, 0, 0)) ||
							(baseUtcOffset + current.DaylightDelta > new TimeSpan (14, 0, 0)))
						throw new InvalidTimeZoneException ("Sum of baseUtcOffset and DaylightDelta of one or more object in adjustmentRules array is greater than 14 or less than -14 hours;");

					if (prev != null && prev.DateStart > current.DateStart)
						throw new InvalidTimeZoneException ("adjustment rules specified in adjustmentRules parameter are not in chronological order");
					
					if (prev != null && prev.DateEnd > current.DateStart)
						throw new InvalidTimeZoneException ("some adjustment rules in the adjustmentRules parameter overlap");

					if (prev != null && prev.DateEnd == current.DateStart)
						throw new InvalidTimeZoneException ("a date can have multiple adjustment rules applied to it");

					prev = current;
				}
			}
		}
		
		public override string ToString ()
		{
			return DisplayName;
		}

		private TimeZoneInfo (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");
			id = (string) info.GetValue ("Id", typeof (string));
			displayName = (string) info.GetValue ("DisplayName", typeof (string));
			standardDisplayName = (string) info.GetValue ("StandardName", typeof (string));
			daylightDisplayName = (string) info.GetValue ("DaylightName", typeof (string));
			baseUtcOffset = (TimeSpan) info.GetValue ("BaseUtcOffset", typeof (TimeSpan));
			adjustmentRules = (TimeZoneInfo.AdjustmentRule []) info.GetValue ("AdjustmentRules", typeof (TimeZoneInfo.AdjustmentRule []));
			supportsDaylightSavingTime = (bool) info.GetValue ("SupportsDaylightSavingTime", typeof (bool));
		}

		private TimeZoneInfo (string id, TimeSpan baseUtcOffset, string displayName, string standardDisplayName, string daylightDisplayName, TimeZoneInfo.AdjustmentRule [] adjustmentRules, bool disableDaylightSavingTime)
		{
			if (id == null)
				throw new ArgumentNullException ("id");

			if (id == String.Empty)
				throw new ArgumentException ("id parameter is an empty string");

			if (baseUtcOffset.Ticks % TimeSpan.TicksPerMinute != 0)
				throw new ArgumentException ("baseUtcOffset parameter does not represent a whole number of minutes");

			if (baseUtcOffset > new TimeSpan (14, 0, 0) || baseUtcOffset < new TimeSpan (-14, 0, 0))
				throw new ArgumentOutOfRangeException ("baseUtcOffset parameter is greater than 14 hours or less than -14 hours");

#if STRICT
			if (id.Length > 32)
				throw new ArgumentException ("id parameter shouldn't be longer than 32 characters");
#endif

			bool supportsDaylightSavingTime = !disableDaylightSavingTime;

			if (adjustmentRules != null && adjustmentRules.Length != 0) {
				AdjustmentRule prev = null;
				foreach (AdjustmentRule current in adjustmentRules) {
					if (current == null)
						throw new InvalidTimeZoneException ("one or more elements in adjustmentRules are null");

					if ((baseUtcOffset + current.DaylightDelta < new TimeSpan (-14, 0, 0)) ||
							(baseUtcOffset + current.DaylightDelta > new TimeSpan (14, 0, 0)))
						throw new InvalidTimeZoneException ("Sum of baseUtcOffset and DaylightDelta of one or more object in adjustmentRules array is greater than 14 or less than -14 hours;");

					if (prev != null && prev.DateStart > current.DateStart)
						throw new InvalidTimeZoneException ("adjustment rules specified in adjustmentRules parameter are not in chronological order");
					
					if (prev != null && prev.DateEnd > current.DateStart)
						throw new InvalidTimeZoneException ("some adjustment rules in the adjustmentRules parameter overlap");

					if (prev != null && prev.DateEnd == current.DateStart)
						throw new InvalidTimeZoneException ("a date can have multiple adjustment rules applied to it");

					prev = current;
				}
			} else {
				supportsDaylightSavingTime = false;
			}
			
			this.id = id;
			this.baseUtcOffset = baseUtcOffset;
			this.displayName = displayName ?? id;
			this.standardDisplayName = standardDisplayName ?? id;
			this.daylightDisplayName = daylightDisplayName;
			this.supportsDaylightSavingTime = supportsDaylightSavingTime;
			this.adjustmentRules = adjustmentRules;
		}

		private AdjustmentRule GetApplicableRule (DateTime dateTime)
		{
			//Applicable rules are in standard time
			DateTime date = dateTime;

			if (dateTime.Kind == DateTimeKind.Local && this != TimeZoneInfo.Local) {
				if (!TryAddTicks (date.ToUniversalTime (), BaseUtcOffset.Ticks, out date))
					return null;
			} else if (dateTime.Kind == DateTimeKind.Utc && this != TimeZoneInfo.Utc) {
				if (!TryAddTicks (date, BaseUtcOffset.Ticks, out date))
					return null;
			}

			// get the date component of the datetime
			date = date.Date;

			if (adjustmentRules != null) {
				foreach (AdjustmentRule rule in adjustmentRules) {
					if (rule.DateStart > date)
						return null;
					if (rule.DateEnd < date)
						continue;
					return rule;
				}
			}
			return null;
		}

		private bool TryGetTransitionOffset (DateTime dateTime, out TimeSpan offset, out bool isDst, bool forOffset = false)
		{
			offset = BaseUtcOffset;
			isDst = false;

			if (transitions == null)
				return false;

			//Transitions are in UTC
			DateTime date = dateTime;

			if (dateTime.Kind == DateTimeKind.Local && this != TimeZoneInfo.Local) {
				if (!TryAddTicks (date.ToUniversalTime (), BaseUtcOffset.Ticks, out date, DateTimeKind.Utc))
					return false;
			}

			var isUtc = false;
			if (dateTime.Kind != DateTimeKind.Utc) {
				if (!TryAddTicks (date, -BaseUtcOffset.Ticks, out date, DateTimeKind.Utc))
					return false;
			} else
				isUtc = true;


			AdjustmentRule current = GetApplicableRule (date);
			if (current != null) {
				DateTime tStart = TransitionPoint (current.DaylightTransitionStart, date.Year);
				DateTime tEnd = TransitionPoint (current.DaylightTransitionEnd, date.Year);
				TryAddTicks (tStart, -BaseUtcOffset.Ticks, out tStart, DateTimeKind.Utc);
				TryAddTicks (tEnd, -BaseUtcOffset.Ticks, out tEnd, DateTimeKind.Utc);
				if ((date >= tStart) && (date <= tEnd)) {
					if (forOffset)
						isDst = true;
					offset = baseUtcOffset; 
					if (isUtc || (date >= new DateTime (tStart.Ticks + current.DaylightDelta.Ticks, DateTimeKind.Utc)))
					{
						offset += current.DaylightDelta;
						isDst = true;
					}

					if (date >= new DateTime (tEnd.Ticks - current.DaylightDelta.Ticks, DateTimeKind.Utc))
					{
						offset = baseUtcOffset;
						isDst = false;
					}

					return true;
				}
			}
			return false;
		}

		private static DateTime TransitionPoint (TransitionTime transition, int year)
		{
			if (transition.IsFixedDateRule) {
				var daysInMonth = DateTime.DaysInMonth (year, transition.Month);
				var transitionDay = transition.Day <= daysInMonth ? transition.Day : daysInMonth;
				return new DateTime (year, transition.Month, transitionDay) + transition.TimeOfDay.TimeOfDay;
			}

			DayOfWeek first = (new DateTime (year, transition.Month, 1)).DayOfWeek;
			int day = 1 + (transition.Week - 1) * 7 + (transition.DayOfWeek - first + 7) % 7;
			if (day >  DateTime.DaysInMonth (year, transition.Month))
				day -= 7;
			if (day < 1)
				day += 7;
			return new DateTime (year, transition.Month, day) + transition.TimeOfDay.TimeOfDay;
		}

		static AdjustmentRule[] ValidateRules (List<AdjustmentRule> adjustmentRules)
		{
			if (adjustmentRules == null || adjustmentRules.Count == 0)
				return null;

			AdjustmentRule prev = null;
			foreach (AdjustmentRule current in adjustmentRules.ToArray ()) {
				if (prev != null && prev.DateEnd > current.DateStart) {
					adjustmentRules.Remove (current);
				}
				prev = current;
			}
			return adjustmentRules.ToArray ();
		}

#if LIBC || MONOTOUCH
		const int BUFFER_SIZE = 16384; //Big enough for any tz file (on Oct 2008, all tz files are under 10k)
		
		private static TimeZoneInfo BuildFromStream (string id, Stream stream)
		{
			byte [] buffer = new byte [BUFFER_SIZE];
			int length = stream.Read (buffer, 0, BUFFER_SIZE);
			
			if (!ValidTZFile (buffer, length))
				throw new InvalidTimeZoneException ("TZ file too big for the buffer");

			try {
				return ParseTZBuffer (id, buffer, length);
			} catch (InvalidTimeZoneException) {
				throw;
			} catch (Exception e) {
				throw new InvalidTimeZoneException ("Time zone information file contains invalid data", e);
			}
		}

		private static bool ValidTZFile (byte [] buffer, int length)
		{
			StringBuilder magic = new StringBuilder ();

			for (int i = 0; i < 4; i++)
				magic.Append ((char)buffer [i]);
			
			if (magic.ToString () != "TZif")
				return false;

			if (length >= BUFFER_SIZE)
				return false;

			return true;
		}

		static int SwapInt32 (int i)
		{
			return (((i >> 24) & 0xff)
				| ((i >> 8) & 0xff00)
				| ((i << 8) & 0xff0000)
				| (((i & 0xff) << 24)));
		}

		static int ReadBigEndianInt32 (byte [] buffer, int start)
		{
			int i = BitConverter.ToInt32 (buffer, start);
			if (!BitConverter.IsLittleEndian)
				return i;

			return SwapInt32 (i);
		}

		private static TimeZoneInfo ParseTZBuffer (string id, byte [] buffer, int length)
		{
			//Reading the header. 4 bytes for magic, 16 are reserved
			int ttisgmtcnt = ReadBigEndianInt32 (buffer, 20);
			int ttisstdcnt = ReadBigEndianInt32 (buffer, 24);
			int leapcnt = ReadBigEndianInt32 (buffer, 28);
			int timecnt = ReadBigEndianInt32 (buffer, 32);
			int typecnt = ReadBigEndianInt32 (buffer, 36);
			int charcnt = ReadBigEndianInt32 (buffer, 40);

			if (length < 44 + timecnt * 5 + typecnt * 6 + charcnt + leapcnt * 8 + ttisstdcnt + ttisgmtcnt)
				throw new InvalidTimeZoneException ();

			Dictionary<int, string> abbreviations = ParseAbbreviations (buffer, 44 + 4 * timecnt + timecnt + 6 * typecnt, charcnt);
			Dictionary<int, TimeType> time_types = ParseTimesTypes (buffer, 44 + 4 * timecnt + timecnt, typecnt, abbreviations);
			List<KeyValuePair<DateTime, TimeType>> transitions = ParseTransitions (buffer, 44, timecnt, time_types);

			if (time_types.Count == 0)
				throw new InvalidTimeZoneException ();

			if (time_types.Count == 1 && time_types[0].IsDst)
				throw new InvalidTimeZoneException ();

			TimeSpan baseUtcOffset = new TimeSpan (0);
			TimeSpan dstDelta = new TimeSpan (0);
			string standardDisplayName = null;
			string daylightDisplayName = null;
			bool dst_observed = false;
			DateTime dst_start = DateTime.MinValue;
			List<AdjustmentRule> adjustmentRules = new List<AdjustmentRule> ();
			bool storeTransition = false;

			for (int i = 0; i < transitions.Count; i++) {
				var pair = transitions [i];
				DateTime ttime = pair.Key;
				TimeType ttype = pair.Value;
				if (!ttype.IsDst) {
					if (standardDisplayName != ttype.Name)
						standardDisplayName = ttype.Name;
					if (baseUtcOffset.TotalSeconds != ttype.Offset) {
						baseUtcOffset = new TimeSpan (0, 0, ttype.Offset);
						if (adjustmentRules.Count > 0) // We ignore AdjustmentRules but store transitions.
							storeTransition = true;
						adjustmentRules = new List<AdjustmentRule> ();
						dst_observed = false;
					}
					if (dst_observed) {
						//FIXME: check additional fields for this:
						//most of the transitions are expressed in GMT 
						dst_start += baseUtcOffset;
						DateTime dst_end = ttime + baseUtcOffset + dstDelta;

						//some weird timezone (America/Phoenix) have end dates on Jan 1st
						if (dst_end.Date == new DateTime (dst_end.Year, 1, 1) && dst_end.Year > dst_start.Year)
							dst_end -= new TimeSpan (24, 0, 0);

						/*
						 * AdjustmentRule specifies a DST period that starts and ends within a year.
						 * When we have a DST period longer than a year, the generated AdjustmentRule may not be usable.
						 * Thus we fallback to the transitions.
						 */
						if (dst_start.AddYears (1) < dst_end)
							storeTransition = true;

						DateTime dateStart, dateEnd;
						if (dst_start.Month < 7)
							dateStart = new DateTime (dst_start.Year, 1, 1);
						else
							dateStart = new DateTime (dst_start.Year, 7, 1);

						if (dst_end.Month >= 7)
							dateEnd = new DateTime (dst_end.Year, 12, 31);
						else
							dateEnd = new DateTime (dst_end.Year, 6, 30);

						
						TransitionTime transition_start = TransitionTime.CreateFixedDateRule (new DateTime (1, 1, 1) + dst_start.TimeOfDay, dst_start.Month, dst_start.Day);
						TransitionTime transition_end = TransitionTime.CreateFixedDateRule (new DateTime (1, 1, 1) + dst_end.TimeOfDay, dst_end.Month, dst_end.Day);
						if  (transition_start != transition_end) //y, that happened in Argentina in 1943-1946
							adjustmentRules.Add (AdjustmentRule.CreateAdjustmentRule (dateStart, dateEnd, dstDelta, transition_start, transition_end));
					}
					dst_observed = false;
				} else {
					if (daylightDisplayName != ttype.Name)
						daylightDisplayName = ttype.Name;
					if (dstDelta.TotalSeconds != ttype.Offset - baseUtcOffset.TotalSeconds) {
						// Round to nearest minute, since it's not possible to create an adjustment rule
						// with sub-minute precision ("The TimeSpan parameter cannot be specified more precisely than whole minutes.")
						// This happens for instance with Europe/Dublin, which had an offset of 34 minutes and 39 seconds in 1916.
						dstDelta = new TimeSpan (0, 0, ttype.Offset) - baseUtcOffset;
						if (dstDelta.Ticks % TimeSpan.TicksPerMinute != 0)
							dstDelta = TimeSpan.FromMinutes ((long) (dstDelta.TotalMinutes + 0.5f));
					}

					dst_start = ttime;
					dst_observed = true;
				}
			}

			TimeZoneInfo tz;
			if (adjustmentRules.Count == 0 && !storeTransition) {
				if (standardDisplayName == null) {
					var t = time_types [0];
					standardDisplayName = t.Name;
					baseUtcOffset = new TimeSpan (0, 0, t.Offset);
				}
				tz = CreateCustomTimeZone (id, baseUtcOffset, id, standardDisplayName);
			} else {
				tz = CreateCustomTimeZone (id, baseUtcOffset, id, standardDisplayName, daylightDisplayName, ValidateRules (adjustmentRules));
			}

			if (storeTransition && transitions.Count > 0) {
				tz.transitions = transitions;
			}
			tz.supportsDaylightSavingTime = adjustmentRules.Count > 0;

			return tz;
		}

		static Dictionary<int, string> ParseAbbreviations (byte [] buffer, int index, int count)
		{
			var abbrevs = new Dictionary<int, string> ();
			int abbrev_index = 0;
			var sb = new StringBuilder ();
			for (int i = 0; i < count; i++) {
				char c = (char) buffer [index + i];
				if (c != '\0')
					sb.Append (c);
				else {
					abbrevs.Add (abbrev_index, sb.ToString ());
					//Adding all the substrings too, as it seems to be used, at least for Africa/Windhoek
					//j == sb.Length empty substring also needs to be added #31432
					for (int j = 1; j <= sb.Length; j++)
						abbrevs.Add (abbrev_index + j, sb.ToString (j, sb.Length - j));
					abbrev_index = i + 1;
					sb = new StringBuilder ();
				}
			}
			return abbrevs;
		}

		static Dictionary<int, TimeType> ParseTimesTypes (byte [] buffer, int index, int count, Dictionary<int, string> abbreviations)
		{
			var types = new Dictionary<int, TimeType> (count);
			for (int i = 0; i < count; i++) {
				int offset = ReadBigEndianInt32 (buffer, index + 6 * i);

				//
				// The official tz database contains timezone with GMT offsets
				// not only in whole hours/minutes but in seconds. This happens for years
				// before 1901. For example
				//
				// NAME		        GMTOFF   RULES	FORMAT	UNTIL
				// Europe/Madrid	-0:14:44 -	LMT	1901 Jan  1  0:00s
				//
				// .NET as of 4.6.2 cannot handle that and uses hours/minutes only, so
				// we remove seconds to not crash later
				//
				offset = (offset / 60) * 60;

				byte is_dst = buffer [index + 6 * i + 4];
				byte abbrev = buffer [index + 6 * i + 5];
				types.Add (i, new TimeType (offset, (is_dst != 0), abbreviations [(int)abbrev]));
			}
			return types;
		}

		static List<KeyValuePair<DateTime, TimeType>> ParseTransitions (byte [] buffer, int index, int count, Dictionary<int, TimeType> time_types)
		{
			var list = new List<KeyValuePair<DateTime, TimeType>> (count);
			for (int i = 0; i < count; i++) {
				int unixtime = ReadBigEndianInt32 (buffer, index + 4 * i);
				DateTime ttime = DateTimeFromUnixTime (unixtime);
				byte ttype = buffer [index + 4 * count + i];
				list.Add (new KeyValuePair<DateTime, TimeType> (ttime, time_types [(int)ttype]));
			}
			return list;
		}

		static DateTime DateTimeFromUnixTime (long unix_time)
		{
			DateTime date_time = new DateTime (1970, 1, 1);
			return date_time.AddSeconds (unix_time);
		}

#region reference sources
		// Shortcut for TimeZoneInfo.Local.GetUtcOffset
		internal static TimeSpan GetLocalUtcOffset(DateTime dateTime, TimeZoneInfoOptions flags)
		{
			bool dst;
			return Local.GetUtcOffset (dateTime, out dst);
		}

		internal TimeSpan GetUtcOffset(DateTime dateTime, TimeZoneInfoOptions flags)
		{
			bool dst;
			return GetUtcOffset (dateTime, out dst);
		}

		static internal TimeSpan GetUtcOffsetFromUtc (DateTime time, TimeZoneInfo zone, out Boolean isDaylightSavings, out Boolean isAmbiguousLocalDst)
		{
			isDaylightSavings = false;
			isAmbiguousLocalDst = false;
			TimeSpan baseOffset = zone.BaseUtcOffset;

			if (zone.IsAmbiguousTime (time)) {
				isAmbiguousLocalDst = true;
//				return baseOffset;
			}

			return zone.GetUtcOffset (time, out isDaylightSavings);
		}
#endregion
	}

	class TimeType {
		public readonly int Offset;
		public readonly bool IsDst;
		public string Name;

		public TimeType (int offset, bool is_dst, string abbrev)
		{
			this.Offset = offset;
			this.IsDst = is_dst;
			this.Name = abbrev;
		}

		public override string ToString ()
		{
			return "offset: " + Offset + "s, is_dst: " + IsDst + ", zone name: " + Name;
		}
#else
	}
#endif
	}
}

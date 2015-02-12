
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
using System.Text;
using System.Globalization;

#if LIBC || MONODROID
using System.IO;
using Mono;
#endif

using Microsoft.Win32;

namespace System
{
#if MOBILE
	[TypeForwardedFrom (Consts.AssemblySystem_Core)]
#else
	[TypeForwardedFrom (Consts.AssemblySystemCore_3_5)]
#endif
	[SerializableAttribute]
	public
	sealed partial class TimeZoneInfo : IEquatable<TimeZoneInfo>, ISerializable, IDeserializationCallback
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

		static TimeZoneInfo CreateLocal ()
		{
#if MONODROID
			return AndroidTimeZones.Local;
#elif MONOTOUCH
			using (Stream stream = GetMonoTouchData (null)) {
				return BuildFromStream ("Local", stream);
			}
#else
#if !NET_2_1
			if (IsWindows && LocalZoneKey != null) {
				string name = (string)LocalZoneKey.GetValue ("TimeZoneKeyName");
				name = TrimSpecial (name);
				if (name != null)
					return TimeZoneInfo.FindSystemTimeZoneById (name);
			}
#endif

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

			try {
				return FindSystemTimeZoneByFileName ("Local", "/etc/localtime");	
			} catch {
				try {
					return FindSystemTimeZoneByFileName ("Local", Path.Combine (TimeZoneDirectory, "localtime"));	
				} catch {
					return null;
				}
			}
#endif
		}

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
		static string timeZoneDirectory;
		static string TimeZoneDirectory {
			get {
				if (timeZoneDirectory == null)
					timeZoneDirectory = "/usr/share/zoneinfo";
				return timeZoneDirectory;
			}
			set {
				ClearCachedData ();
				timeZoneDirectory = value;
			}
		}
#endif
		private AdjustmentRule [] adjustmentRules;

#if !NET_2_1
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
			var Istart = 0;
			while (Istart < str.Length && !char.IsLetterOrDigit(str[Istart])) Istart++;
			var Iend = str.Length - 1;
			while (Iend > Istart && !char.IsLetterOrDigit(str[Iend])) Iend--;
			
			return str.Substring (Istart, Iend-Istart+1);
		}
		
		static RegistryKey timeZoneKey;
		static RegistryKey TimeZoneKey {
			get {
				if (timeZoneKey != null)
					return timeZoneKey;
				if (!IsWindows)
					return null;
				
				return timeZoneKey = Registry.LocalMachine.OpenSubKey (
					"SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Time Zones",
					false);
			}
		}
		
		static RegistryKey localZoneKey;
		static RegistryKey LocalZoneKey {
			get {
				if (localZoneKey != null)
					return localZoneKey;
				
				if (!IsWindows)
					return null;
				
				return localZoneKey = Registry.LocalMachine.OpenSubKey (
					"SYSTEM\\CurrentControlSet\\Control\\TimeZoneInformation", false);
			}
		}
#endif

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
			AdjustmentRule rule = destinationTimeZone.GetApplicableRule (utcDateTime);
		
			if (rule != null && destinationTimeZone.IsDaylightSavingTime(utcDateTime)) {
				var offset = destinationTimeZone.BaseUtcOffset + rule.DaylightDelta;
				return new DateTimeOffset(DateTime.SpecifyKind(utcDateTime, DateTimeKind.Unspecified) + offset, offset);
			}
			else {
				return new DateTimeOffset(DateTime.SpecifyKind(utcDateTime, DateTimeKind.Unspecified) + destinationTimeZone.BaseUtcOffset, destinationTimeZone.BaseUtcOffset);
			}
		}

		public static DateTime ConvertTimeBySystemTimeZoneId (DateTime dateTime, string destinationTimeZoneId)
		{
			return ConvertTime (dateTime, FindSystemTimeZoneById (destinationTimeZoneId));
		}

		public static DateTime ConvertTimeBySystemTimeZoneId (DateTime dateTime, string sourceTimeZoneId, string destinationTimeZoneId)
		{
			return ConvertTime (dateTime, FindSystemTimeZoneById (sourceTimeZoneId), FindSystemTimeZoneById (destinationTimeZoneId));
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
			
			//FIXME: do not rely on DateTime implementation !
			if (this == TimeZoneInfo.Local) 
			{
				return dateTime.ToLocalTime ();
			}


			AdjustmentRule rule = GetApplicableRule (dateTime);
			if (rule != null && IsDaylightSavingTime (DateTime.SpecifyKind (dateTime, DateTimeKind.Utc)))
				return DateTime.SpecifyKind (dateTime + BaseUtcOffset + rule.DaylightDelta , DateTimeKind.Unspecified);
			else
				return DateTime.SpecifyKind (dateTime + BaseUtcOffset, DateTimeKind.Unspecified);
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

			if (sourceTimeZone.IsAmbiguousTime (dateTime) || !sourceTimeZone.IsDaylightSavingTime (dateTime)) {
				var ticks = dateTime.Ticks - sourceTimeZone.BaseUtcOffset.Ticks;
				if (ticks < DateTime.MinValue.Ticks)
					ticks = DateTime.MinValue.Ticks;
				else if (ticks > DateTime.MaxValue.Ticks)
					ticks = DateTime.MaxValue.Ticks;

				return new DateTime (ticks, DateTimeKind.Utc);
			}
			
				AdjustmentRule rule = sourceTimeZone.GetApplicableRule (dateTime);
				if (rule != null)
					return DateTime.SpecifyKind (dateTime - sourceTimeZone.BaseUtcOffset - rule.DaylightDelta, DateTimeKind.Utc);
				else
					return DateTime.SpecifyKind (dateTime - sourceTimeZone.BaseUtcOffset, DateTimeKind.Utc);
			
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
#if !NET_2_1
			if (TimeZoneKey != null)
			{
				RegistryKey key = TimeZoneKey.OpenSubKey (id, false);
				if (key == null)
					throw new TimeZoneNotFoundException ();
				return FromRegistryKey(id, key);
			}
#endif
			// Local requires special logic that already exists in the Local property (bug #326)
			if (id == "Local")
				return Local;
#if MONOTOUCH
			using (Stream stream = GetMonoTouchData (id)) {
				return BuildFromStream (id, stream);
			}
#elif MONODROID
			var timeZoneInfo = AndroidTimeZones.GetTimeZone (id, id);
			if (timeZoneInfo == null)
				throw new TimeZoneNotFoundException ();
			return timeZoneInfo;
#elif LIBC
			string filepath = Path.Combine (TimeZoneDirectory, id);
			return FindSystemTimeZoneByFileName (id, filepath);
#else
			throw new NotImplementedException ();
#endif
		}

#if LIBC
		private static TimeZoneInfo FindSystemTimeZoneByFileName (string id, string filepath)
		{
			if (!File.Exists (filepath))
				throw new TimeZoneNotFoundException ();

			using (FileStream stream = File.OpenRead (filepath)) {
				return BuildFromStream (id, stream);
			}
		}
#endif
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
			} catch (Exception e) {
				throw new InvalidTimeZoneException (e.Message);
			}
		}
#endif

#if !NET_2_1
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

			return CreateCustomTimeZone (id, baseUtcOffset, display_name, standard_name, daylight_name, ValidateRules (adjustmentRules).ToArray ());
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

			if (daylight_year == 0) {
				start_date = new DateTime (start_year, 1, 1);
				start_transition_time = TransitionTime.CreateFloatingDateRule (
					start_timeofday, daylight_month, daylight_day,
					(DayOfWeek) daylight_dayofweek);
			}
			else {
				start_date = new DateTime (daylight_year, daylight_month, daylight_day,
					daylight_hour, daylight_minute, daylight_second, daylight_millisecond);
				start_transition_time = TransitionTime.CreateFixedDateRule (
					start_timeofday, daylight_month, daylight_day);
			}

			DateTime end_date;
			DateTime end_timeofday = new DateTime (1, 1, 1, standard_hour, standard_minute, standard_second, standard_millisecond);
			TransitionTime end_transition_time;

			if (standard_year == 0) {
				end_date = new DateTime (end_year, 12, 31);
				end_transition_time = TransitionTime.CreateFloatingDateRule (
					end_timeofday, standard_month, standard_day,
					(DayOfWeek) standard_dayofweek);
			}
			else {
				end_date = new DateTime (standard_year, standard_month, standard_day,
					standard_hour, standard_minute, standard_second, standard_millisecond);
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
			if (!supportsDaylightSavingTime)
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

		//FIXME: change this to a generic Dictionary and allow caching for FindSystemTimeZoneById
		private static List<TimeZoneInfo> systemTimeZones;
		public static ReadOnlyCollection<TimeZoneInfo> GetSystemTimeZones ()
		{
			if (systemTimeZones == null) {
				systemTimeZones = new List<TimeZoneInfo> ();
#if !NET_2_1
				if (TimeZoneKey != null) {
					foreach (string id in TimeZoneKey.GetSubKeyNames ()) {
						try {
							systemTimeZones.Add (FindSystemTimeZoneById (id));
						} catch {}
					}

					return new ReadOnlyCollection<TimeZoneInfo> (systemTimeZones);
				}
#endif
#if MONODROID
			foreach (string id in AndroidTimeZones.GetAvailableIds ()) {
				var tz = AndroidTimeZones.GetTimeZone (id, id);
				if (tz != null)
					systemTimeZones.Add (tz);
			}
#elif MONOTOUCH
				if (systemTimeZones.Count == 0) {
					foreach (string name in GetMonoTouchNames ()) {
						using (Stream stream = GetMonoTouchData (name, false)) {
							if (stream == null)
								continue;
							systemTimeZones.Add (BuildFromStream (name, stream));
						}
					}
				}
#elif LIBC
				string[] continents = new string [] {"Africa", "America", "Antarctica", "Arctic", "Asia", "Atlantic", "Brazil", "Canada", "Chile", "Europe", "Indian", "Mexico", "Mideast", "Pacific", "US"};
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
			return new ReadOnlyCollection<TimeZoneInfo> (systemTimeZones);
		}

		public TimeSpan GetUtcOffset (DateTime dateTime)
		{
			bool isDST;
			return GetUtcOffset (dateTime, out isDST);
		}

		public TimeSpan GetUtcOffset (DateTimeOffset dateTimeOffset)
		{
			throw new NotImplementedException ();
		}

		private TimeSpan GetUtcOffset (DateTime dateTime, out bool isDST)
		{
			isDST = false;

			TimeZoneInfo tz = this;
			if (dateTime.Kind == DateTimeKind.Utc)
				tz = TimeZoneInfo.Utc;

			if (dateTime.Kind == DateTimeKind.Local)
				tz = TimeZoneInfo.Local;

			bool isTzDst;
			var tzOffset = GetUtcOffset (dateTime, tz, out isTzDst);

			if (tz == this) {
				isDST = isTzDst;
				return tzOffset;
			}

			var utcTicks = dateTime.Ticks - tzOffset.Ticks;
			if (utcTicks < 0 || utcTicks > DateTime.MaxValue.Ticks)
				return BaseUtcOffset;

			var utcDateTime = new DateTime (utcTicks, DateTimeKind.Utc);

			return GetUtcOffset (utcDateTime, this, out isDST);
		}

		private static TimeSpan GetUtcOffset (DateTime dateTime, TimeZoneInfo tz, out bool isDST)
		{
			if (dateTime.Kind == DateTimeKind.Local && tz != TimeZoneInfo.Local)
				throw new Exception ();

			isDST = false;

			if (tz == TimeZoneInfo.Utc)
				return TimeSpan.Zero;

			TimeSpan offset;
			if (tz.TryGetTransitionOffset(dateTime, out offset, out isDST))
				return offset;

			if (dateTime.Kind == DateTimeKind.Utc) {
				var utcRule = tz.GetApplicableRule (dateTime);
				if (utcRule != null && tz.IsInDST (utcRule, dateTime)) {
					isDST = true;
					return tz.BaseUtcOffset + utcRule.DaylightDelta;
				}

				return tz.BaseUtcOffset;
			}

			var stdTicks = dateTime.Ticks - tz.BaseUtcOffset.Ticks;
			if (stdTicks < 0 || stdTicks > DateTime.MaxValue.Ticks)
				return tz.BaseUtcOffset;

			var stdUtcDateTime = new DateTime (stdTicks, DateTimeKind.Utc);
			var tzRule = tz.GetApplicableRule (stdUtcDateTime);

			DateTime dstUtcDateTime = DateTime.MinValue;
			if (tzRule != null) {
				var dstTicks = stdUtcDateTime.Ticks - tzRule.DaylightDelta.Ticks;
				if (dstTicks < 0 || dstTicks > DateTime.MaxValue.Ticks)
					return tz.BaseUtcOffset;

				dstUtcDateTime = new DateTime (dstTicks, DateTimeKind.Utc);
			}

			if (tzRule != null && tz.IsInDST (tzRule, stdUtcDateTime) && tz.IsInDST (tzRule, dstUtcDateTime)) {
				isDST = true;
				return tz.BaseUtcOffset + tzRule.DaylightDelta;
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
				if (dateTime > tpoint - rule.DaylightDelta  && dateTime <= tpoint)
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
			return IsInDSTForYear (rule, dateTime, dateTime.Year - 1);
		}

		bool IsInDSTForYear (AdjustmentRule rule, DateTime dateTime, int year)
		{
			DateTime DST_start = TransitionPoint (rule.DaylightTransitionStart, year);
			DateTime DST_end = TransitionPoint (rule.DaylightTransitionEnd, year + ((rule.DaylightTransitionStart.Month < rule.DaylightTransitionEnd.Month) ? 0 : 1));
			if (dateTime.Kind == DateTimeKind.Utc) {
				DST_start -= BaseUtcOffset;
				DST_end -= (BaseUtcOffset + rule.DaylightDelta);
			}

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
			throw new NotImplementedException ();
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

			if (dateTime.Kind == DateTimeKind.Local && this != TimeZoneInfo.Local)
				date = date.ToUniversalTime () + BaseUtcOffset;
			else if (dateTime.Kind == DateTimeKind.Utc && this != TimeZoneInfo.Utc)
				date = date + BaseUtcOffset;

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

		private bool TryGetTransitionOffset (DateTime dateTime, out TimeSpan offset,out bool isDst)
		{
			offset = BaseUtcOffset;
			isDst = false;

			if (transitions == null)
				return false;

			//Transitions are in UTC
			DateTime date = dateTime;

			if (dateTime.Kind == DateTimeKind.Local && this != TimeZoneInfo.Local)
				date = date.ToUniversalTime () + BaseUtcOffset;

			if (dateTime.Kind != DateTimeKind.Utc) {
				if (date.Ticks < BaseUtcOffset.Ticks)
					return false;
				date = date - BaseUtcOffset;
			}

			for (var i =  transitions.Count - 1; i >= 0; i--) {
				var pair = transitions [i];
				DateTime ttime = pair.Key;
				TimeType ttype = pair.Value;

				if (ttime > date)
					continue;

				offset =  new TimeSpan (0, 0, ttype.Offset);
				isDst = ttype.IsDst;

				return true;
			}

			return false;
		}

		private static DateTime TransitionPoint (TransitionTime transition, int year)
		{
			if (transition.IsFixedDateRule)
				return new DateTime (year, transition.Month, transition.Day) + transition.TimeOfDay.TimeOfDay;

			DayOfWeek first = (new DateTime (year, transition.Month, 1)).DayOfWeek;
			int day = 1 + (transition.Week - 1) * 7 + (transition.DayOfWeek - first + 7) % 7;
			if (day >  DateTime.DaysInMonth (year, transition.Month))
				day -= 7;
			if (day < 1)
				day += 7;
			return new DateTime (year, transition.Month, day) + transition.TimeOfDay.TimeOfDay;
		}

		static List<AdjustmentRule> ValidateRules (List<AdjustmentRule> adjustmentRules)
		{
			AdjustmentRule prev = null;
			foreach (AdjustmentRule current in adjustmentRules.ToArray ()) {
				if (prev != null && prev.DateEnd > current.DateStart) {
					adjustmentRules.Remove (current);
				}
				prev = current;
			}
			return adjustmentRules;
		}

#if LIBC || MONODROID
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
				| ((i << 24)));
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

			if (time_types.Count == 1 && ((TimeType)time_types[0]).IsDst)
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
					if (dstDelta.TotalSeconds != ttype.Offset - baseUtcOffset.TotalSeconds)
						dstDelta = new TimeSpan(0, 0, ttype.Offset) - baseUtcOffset;

					dst_start = ttime;
					dst_observed = true;
				}
			}

			TimeZoneInfo tz;
			if (adjustmentRules.Count == 0 && !storeTransition) {
				TimeType t = (TimeType)time_types [0];
				if (standardDisplayName == null) {
					standardDisplayName = t.Name;
					baseUtcOffset = new TimeSpan (0, 0, t.Offset);
				}
				tz = CreateCustomTimeZone (id, baseUtcOffset, id, standardDisplayName);
			} else {
				tz = CreateCustomTimeZone (id, baseUtcOffset, id, standardDisplayName, daylightDisplayName, ValidateRules (adjustmentRules).ToArray ());
			}

			if (storeTransition)
				tz.transitions = transitions;

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
					for (int j = 1; j < sb.Length; j++)
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

        // used by GetUtcOffsetFromUtc (DateTime.Now, DateTime.ToLocalTime) for max/min whole-day range checks
        private static DateTime s_maxDateOnly = new DateTime(9999, 12, 31);
        private static DateTime s_minDateOnly = new DateTime(1, 1, 2);

        static internal TimeSpan GetUtcOffsetFromUtc (DateTime time, TimeZoneInfo zone, out Boolean isDaylightSavings, out Boolean isAmbiguousLocalDst)
        {
            isDaylightSavings = false;
            isAmbiguousLocalDst = false;
            TimeSpan baseOffset = zone.BaseUtcOffset;
            Int32 year;
            AdjustmentRule rule;

            if (time > s_maxDateOnly) {
                rule = zone.GetAdjustmentRuleForTime(DateTime.MaxValue);
                year = 9999;
            }
            else if (time < s_minDateOnly) {
                rule = zone.GetAdjustmentRuleForTime(DateTime.MinValue);
                year = 1;
            }
            else {
                DateTime targetTime = time + baseOffset;
                year = time.Year;
                rule = zone.GetAdjustmentRuleForTime(targetTime);
            }

            if (rule != null) {
                isDaylightSavings = GetIsDaylightSavingsFromUtc(time, year, zone.baseUtcOffset, rule, out isAmbiguousLocalDst);
                baseOffset += (isDaylightSavings ? rule.DaylightDelta : TimeSpan.Zero /* */);
            }

            return baseOffset;
        }

        // assumes dateTime is in the current time zone's time
        private AdjustmentRule GetAdjustmentRuleForTime(DateTime dateTime) {
            if (adjustmentRules == null || adjustmentRules.Length == 0) {
                return null;
            }

#if WINXP_AND_WIN2K3_SUPPORT
            // On pre-Vista versions of Windows if you run "cmd /c date" or "cmd /c time" to update the system time
            // the operating system doesn't pick up the correct time zone adjustment rule (it stays on the currently loaded rule).
            // We need to use the OS API data in this scenario instead of the loaded adjustment rules from the registry for
            // consistency.  Otherwise DateTime.Now might not match the time displayed in the system tray.              
            if (!Environment.IsWindowsVistaOrAbove && s_cachedData.GetCorrespondingKind(this) == DateTimeKind.Local) {
                return s_cachedData.GetOneYearLocalFromLocal(dateTime.Year).rule;
            }
#endif
            // Only check the whole-date portion of the dateTime -
            // This is because the AdjustmentRule DateStart & DateEnd are stored as
            // Date-only values {4/2/2006 - 10/28/2006} but actually represent the
            // time span {4/2/2006@00:00:00.00000 - 10/28/2006@23:59:59.99999}
            DateTime date = dateTime.Date;

            for (int i = 0; i < adjustmentRules.Length; i++) {
                if (adjustmentRules[i].DateStart <= date && adjustmentRules[i].DateEnd >= date) {
                    return adjustmentRules[i];
                }
            }

            return null;
        }

        //
        // GetIsDaylightSavingsFromUtc -
        //
        // Helper function that checks if a given dateTime is in Daylight Saving Time (DST)
        // This function assumes the dateTime is in UTC and AdjustmentRule is in a different time zone
        //
        static private Boolean GetIsDaylightSavingsFromUtc(DateTime time, Int32 Year, TimeSpan utc, AdjustmentRule rule, out Boolean isAmbiguousLocalDst) {
            isAmbiguousLocalDst = false;

            if (rule == null) {
                return false;
            }

            // Get the daylight changes for the year of the specified time.
            TimeSpan offset = utc; /* */
            DaylightTime daylightTime = GetDaylightTime(Year, rule);

            // The start and end times represent the range of universal times that are in DST for that year.                
            // Within that there is an ambiguous hour, usually right at the end, but at the beginning in
            // the unusual case of a negative daylight savings delta.
            DateTime startTime = daylightTime.Start - offset;
            DateTime endTime = daylightTime.End - offset - rule.DaylightDelta; /* */
            DateTime ambiguousStart;
            DateTime ambiguousEnd;
            if (daylightTime.Delta.Ticks > 0) {
                ambiguousStart = endTime - daylightTime.Delta;
                ambiguousEnd = endTime;
            } else {
                ambiguousStart = startTime;
                ambiguousEnd = startTime - daylightTime.Delta;
            }

            Boolean isDst = CheckIsDst(startTime, time, endTime);

            // See if the resulting local time becomes ambiguous. This must be captured here or the
            // DateTime will not be able to round-trip back to UTC accurately.
            if (isDst) {
                isAmbiguousLocalDst = (time >= ambiguousStart && time < ambiguousEnd);

                if (!isAmbiguousLocalDst && ambiguousStart.Year != ambiguousEnd.Year) {
                    // there exists an extreme corner case where the start or end period is on a year boundary and
                    // because of this the comparison above might have been performed for a year-early or a year-later
                    // than it should have been.
                    DateTime ambiguousStartModified;
                    DateTime ambiguousEndModified;
                    try {
                        ambiguousStartModified = ambiguousStart.AddYears(1);
                        ambiguousEndModified   = ambiguousEnd.AddYears(1);
                        isAmbiguousLocalDst = (time >= ambiguousStart && time < ambiguousEnd); 
                    }
                    catch (ArgumentOutOfRangeException) {}

                    if (!isAmbiguousLocalDst) {
                        try {
                            ambiguousStartModified = ambiguousStart.AddYears(-1);
                            ambiguousEndModified   = ambiguousEnd.AddYears(-1);
                            isAmbiguousLocalDst = (time >= ambiguousStart && time < ambiguousEnd);
                        }
                        catch (ArgumentOutOfRangeException) {}
                    }

                }
            }

            return isDst;
        }


        static private Boolean CheckIsDst(DateTime startTime, DateTime time, DateTime endTime) {
            Boolean isDst;

            int startTimeYear = startTime.Year;
            int endTimeYear = endTime.Year;

            if (startTimeYear != endTimeYear) {
                endTime = endTime.AddYears(startTimeYear - endTimeYear);
            }

            int timeYear = time.Year;

            if (startTimeYear != timeYear) {
                time = time.AddYears(startTimeYear - timeYear);
            }

            if (startTime > endTime) {
                // In southern hemisphere, the daylight saving time starts later in the year, and ends in the beginning of next year.
                // Note, the summer in the southern hemisphere begins late in the year.
                isDst = (time < endTime || time >= startTime);
            }
            else {
                // In northern hemisphere, the daylight saving time starts in the middle of the year.
                isDst = (time >= startTime && time < endTime);
            }
            return isDst;
        }

        //
        // GetDaylightTime -
        //
        // Helper function that returns a DaylightTime from a year and AdjustmentRule
        //
        static private DaylightTime GetDaylightTime(Int32 year, AdjustmentRule rule) {
            TimeSpan delta = rule.DaylightDelta;
            DateTime startTime = TransitionTimeToDateTime(year, rule.DaylightTransitionStart);
            DateTime endTime = TransitionTimeToDateTime(year, rule.DaylightTransitionEnd);
            return new DaylightTime(startTime, endTime, delta);
        }

        //
        // TransitionTimeToDateTime -
        //
        // Helper function that converts a year and TransitionTime into a DateTime
        //
        static private DateTime TransitionTimeToDateTime(Int32 year, TransitionTime transitionTime) {
            DateTime value;
            DateTime timeOfDay = transitionTime.TimeOfDay;

            if (transitionTime.IsFixedDateRule) {
                // create a DateTime from the passed in year and the properties on the transitionTime

                // if the day is out of range for the month then use the last day of the month
                Int32 day = DateTime.DaysInMonth(year, transitionTime.Month);

                value = new DateTime(year, transitionTime.Month, (day < transitionTime.Day) ? day : transitionTime.Day, 
                            timeOfDay.Hour, timeOfDay.Minute, timeOfDay.Second, timeOfDay.Millisecond);
            }
            else {
                if (transitionTime.Week <= 4) {
                    //
                    // Get the (transitionTime.Week)th Sunday.
                    //
                    value = new DateTime(year, transitionTime.Month, 1,
                            timeOfDay.Hour, timeOfDay.Minute, timeOfDay.Second, timeOfDay.Millisecond);

                    int dayOfWeek = (int)value.DayOfWeek;
                    int delta = (int)transitionTime.DayOfWeek - dayOfWeek;
                    if (delta < 0) {
                        delta += 7;
                    }
                    delta += 7 * (transitionTime.Week - 1);

                    if (delta > 0) {
                        value = value.AddDays(delta);
                    }
                }
                else {
                    //
                    // If TransitionWeek is greater than 4, we will get the last week.
                    //
                    Int32 daysInMonth = DateTime.DaysInMonth(year, transitionTime.Month);
                    value = new DateTime(year, transitionTime.Month, daysInMonth,
                            timeOfDay.Hour, timeOfDay.Minute, timeOfDay.Second, timeOfDay.Millisecond);

                    // This is the day of week for the last day of the month.
                    int dayOfWeek = (int)value.DayOfWeek;
                    int delta = dayOfWeek - (int)transitionTime.DayOfWeek;
                    if (delta < 0) {
                        delta += 7;
                    }

                    if (delta > 0) {
                        value = value.AddDays(-delta);
                    }
                }
            }
            return value;
        }

        //
        // IsInvalidTime -
        //
        // returns true when dateTime falls into a "hole in time".
        //
        public Boolean IsInvalidTime(DateTime dateTime) {
            Boolean isInvalid = false;
          
            if ( (dateTime.Kind == DateTimeKind.Unspecified)
            ||   (dateTime.Kind == DateTimeKind.Local && this == Local) ) {

                // only check Unspecified and (Local when this TimeZoneInfo instance is Local)
                AdjustmentRule rule = GetAdjustmentRuleForTime(dateTime);


                if (rule != null) {
                    DaylightTime daylightTime = GetDaylightTime(dateTime.Year, rule);
                    isInvalid = GetIsInvalidTime(dateTime, rule, daylightTime);
                }
                else {
                    isInvalid = false;
                }
            }

            return isInvalid;
        }

        //
        // GetIsInvalidTime -
        //
        // Helper function that checks if a given DateTime is in an invalid time ("time hole")
        // A "time hole" occurs at a DST transition point when time jumps forward;
        // For example, in Pacific Standard Time on Sunday, April 2, 2006 time jumps from
        // 1:59:59.9999999 to 3AM.  The time range 2AM to 2:59:59.9999999AM is the "time hole".
        // A "time hole" is not limited to only occurring at the start of DST, and may occur at
        // the end of DST as well.
        //
        static private Boolean GetIsInvalidTime(DateTime time, AdjustmentRule rule, DaylightTime daylightTime) {
            Boolean isInvalid = false;
            if (rule == null || rule.DaylightDelta == TimeSpan.Zero) {
                return isInvalid;
            }

            DateTime startInvalidTime;
            DateTime endInvalidTime;

            // if at DST start we transition forward in time then there is an ambiguous time range at the DST end
            if (rule.DaylightDelta < TimeSpan.Zero) {
                startInvalidTime = daylightTime.End;
                endInvalidTime = daylightTime.End - rule.DaylightDelta; /* */
            }
            else {
                startInvalidTime = daylightTime.Start;
                endInvalidTime = daylightTime.Start + rule.DaylightDelta; /* */
            }

            isInvalid = (time >= startInvalidTime && time < endInvalidTime);

            if (!isInvalid && startInvalidTime.Year != endInvalidTime.Year) {
                // there exists an extreme corner case where the start or end period is on a year boundary and
                // because of this the comparison above might have been performed for a year-early or a year-later
                // than it should have been.
                DateTime startModifiedInvalidTime;
                DateTime endModifiedInvalidTime;
                try {
                    startModifiedInvalidTime = startInvalidTime.AddYears(1);
                    endModifiedInvalidTime   = endInvalidTime.AddYears(1);
                    isInvalid = (time >= startModifiedInvalidTime && time < endModifiedInvalidTime);
                }
                catch (ArgumentOutOfRangeException) {}

                if (!isInvalid) {
                    try {
                        startModifiedInvalidTime = startInvalidTime.AddYears(-1);
                        endModifiedInvalidTime  = endInvalidTime.AddYears(-1);
                        isInvalid = (time >= startModifiedInvalidTime && time < endModifiedInvalidTime);
                    }
                    catch (ArgumentOutOfRangeException) {}
                }
            }
            return isInvalid;
        } 
#endregion
	}

	struct TimeType {
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

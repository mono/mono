/*
 * System.TimeZoneInfo.Serialization
 *
 * Author(s)
 * 	Sasha Kotlyar <sasha@arktronic.com>
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

using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;

namespace System
{
	public
	partial class TimeZoneInfo
	{
		public static TimeZoneInfo FromSerializedString (string source)
		{
			var input = new StringBuilder (source);
			var tzId = DeserializeString (ref input);
			var offset = DeserializeInt (ref input);
			var displayName = DeserializeString (ref input);
			var standardName = DeserializeString (ref input);
			var daylightName = DeserializeString (ref input);
			List<TimeZoneInfo.AdjustmentRule> rules = null;
			while (input [0] != ';') {
				if (rules == null)
					rules = new List<TimeZoneInfo.AdjustmentRule> ();
				rules.Add (DeserializeAdjustmentRule (ref input));
			}
			var offsetSpan = TimeSpan.FromMinutes (offset);
			return TimeZoneInfo.CreateCustomTimeZone (tzId, offsetSpan, displayName, standardName, daylightName, rules?.ToArray ());
		}

		public string ToSerializedString ()
		{
			var stb = new StringBuilder ();
			var daylightName = (string.IsNullOrEmpty(this.DaylightName) ? this.StandardName : this.DaylightName);
			stb.AppendFormat ("{0};{1};{2};{3};{4};", EscapeForSerialization (this.Id), (int)this.BaseUtcOffset.TotalMinutes,
				EscapeForSerialization (this.DisplayName), EscapeForSerialization (this.StandardName), EscapeForSerialization (daylightName));

			if (this.SupportsDaylightSavingTime) {
				foreach (var rule in this.GetAdjustmentRules()) {
					var start = rule.DateStart.ToString ("MM:dd:yyyy", CultureInfo.InvariantCulture);
					var end = rule.DateEnd.ToString ("MM:dd:yyyy", CultureInfo.InvariantCulture);
					var delta = (int)rule.DaylightDelta.TotalMinutes;
					var transitionStart = SerializeTransitionTime (rule.DaylightTransitionStart);
					var transitionEnd = SerializeTransitionTime (rule.DaylightTransitionEnd);
					stb.AppendFormat ("[{0};{1};{2};{3};{4};]", start, end, delta,
						transitionStart, transitionEnd);
				}
			}

			stb.Append (";");
			return stb.ToString ();
		}

		private static TimeZoneInfo.AdjustmentRule DeserializeAdjustmentRule (ref StringBuilder input)
		{
			// Similar to: [01:01:0001;12:31:9999;60;[0;01:00:00;3;5;0;];[0;02:00:00;10;5;0;];]
			if (input [0] != '[')
				throw new SerializationException ();
			input.Remove (0, 1); // [
			var dateStart = DeserializeDate (ref input);
			var dateEnd = DeserializeDate (ref input);
			var delta = DeserializeInt (ref input);
			var transitionStart = DeserializeTransitionTime (ref input);
			var transitionEnd = DeserializeTransitionTime (ref input);
			input.Remove (0, 1); // ]
			var deltaSpan = TimeSpan.FromMinutes (delta);
			return TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (dateStart, dateEnd, deltaSpan,
				transitionStart, transitionEnd);
		}

		private static TimeZoneInfo.TransitionTime DeserializeTransitionTime (ref StringBuilder input)
		{
			if (input [0] != '[' || (input [1] != '0' && input [1] != '1') || input [2] != ';')
				throw new SerializationException ();
			var rule = input [1];
			input.Remove (0, 3); // [#;
			var timeOfDay = DeserializeTime (ref input);
			var month = DeserializeInt (ref input);
			if (rule == '0') {
				// Floating rule such as: [0;01:00:00;3;5;0;];
				var week = DeserializeInt (ref input);
				var dayOfWeek = DeserializeInt (ref input);
				input.Remove (0, 2); // ];
				return TimeZoneInfo.TransitionTime.CreateFloatingDateRule (timeOfDay, month, week, (DayOfWeek)dayOfWeek);
			}

			// Fixed rule such as: [1;02:15:59.999;6;2;];
			var day = DeserializeInt (ref input);
			input.Remove (0, 2); // ];
			return TimeZoneInfo.TransitionTime.CreateFixedDateRule (timeOfDay, month, day);
		}

		private static string DeserializeString (ref StringBuilder input)
		{
			var stb = new StringBuilder ();
			var isEscaped = false;
			int charCount;
			for (charCount = 0; charCount < input.Length; charCount++) {
				var inChar = input [charCount];
				if (isEscaped) {
					isEscaped = false;
					stb.Append (inChar);
				} else if (inChar == '\\') {
					isEscaped = true;
					continue;
				} else if (inChar == ';') {
					break;
				} else {
					stb.Append (inChar);
				}
			}
			input.Remove (0, charCount + 1);
			return stb.ToString ();
		}

		private static int DeserializeInt(ref StringBuilder input)
		{
			int charCount = 0;
			while(charCount++ < input.Length)
			{
				if (input[charCount] == ';')
					break;
			}
			int result;
			if(!int.TryParse(input.ToString(0, charCount), NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
				throw new SerializationException();
			input.Remove(0, charCount + 1);
			return result;
		}

		private static DateTime DeserializeDate (ref StringBuilder input)
		{
			var inChars = new char[11];
			input.CopyTo (0, inChars, 0, inChars.Length);
			DateTime result;
			if (!DateTime.TryParseExact (new string (inChars), "MM:dd:yyyy;", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
				throw new SerializationException ();
			input.Remove (0, inChars.Length);
			return result;
		}

		private static DateTime DeserializeTime (ref StringBuilder input)
		{
			if (input [8] == ';') {
				// Without milliseconds
				var inChars = new char[9];
				input.CopyTo (0, inChars, 0, inChars.Length);
				DateTime result;
				if (!DateTime.TryParseExact (new string (inChars), "HH:mm:ss;", CultureInfo.InvariantCulture, DateTimeStyles.NoCurrentDateDefault, out result))
					throw new SerializationException ();
				input.Remove (0, inChars.Length);
				return result;
			} else if (input [12] == ';') {
				// With milliseconds
				char[] inChars = new char[13];
				input.CopyTo (0, inChars, 0, inChars.Length);
				var inString = new string (inChars);
				DateTime result;
				if (!DateTime.TryParseExact (inString, "HH:mm:ss.fff;", CultureInfo.InvariantCulture, DateTimeStyles.NoCurrentDateDefault, out result))
					throw new SerializationException ();
				input.Remove (0, inChars.Length);
				return result;
			}
			throw new SerializationException ();
		}

		private static string EscapeForSerialization (string unescaped)
		{
			return unescaped.Replace (@"\", @"\\").Replace (";", "\\;");
		}

		private static string SerializeTransitionTime (TimeZoneInfo.TransitionTime transition)
		{
			string timeOfDay;
			if (transition.TimeOfDay.Millisecond > 0)
				timeOfDay = transition.TimeOfDay.ToString ("HH:mm:ss.fff");
			else
				timeOfDay = transition.TimeOfDay.ToString ("HH:mm:ss");

			if (transition.IsFixedDateRule) {
				return string.Format ("[1;{0};{1};{2};]", timeOfDay, transition.Month, transition.Day);
			}

			return string.Format ("[0;{0};{1};{2};{3};]", timeOfDay, transition.Month,
				transition.Week, (int)transition.DayOfWeek);
		}
	}
}

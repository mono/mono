//
// System.Diagnostics.TraceSwtich.cs
//
// Comments from John R. Hicks <angryjohn69@nc.rr.com> original implementation 
// can be found at: /mcs/docs/apidocs/xml/en/System.Diagnostics
//
// Author:
//	John R. Hicks (angryjohn69@nc.rr.com)
//	Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2001-2002
//

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

namespace System.Diagnostics
{
	[SwitchLevel (typeof (TraceLevel))]
	public class TraceSwitch : Switch
	{
		public TraceSwitch(string displayName, string description)
			: base(displayName, description)
		{
		}

		public TraceSwitch(string displayName, string description, string defaultSwitchValue)
			: base(displayName, description)
		{
			Value = defaultSwitchValue;
		}

		public TraceLevel Level {
			get {return (TraceLevel) SwitchSetting;}
			set {
				if (!Enum.IsDefined (typeof(TraceLevel), value))
					throw new ArgumentException ("value");
				SwitchSetting = (int) value;
			}
		}

		public bool TraceError {
			get {return SwitchSetting >= (int) TraceLevel.Error;}
		}

		public bool TraceWarning {
			get {return SwitchSetting >= (int) TraceLevel.Warning;}
		}

		public bool TraceInfo {
			get {return SwitchSetting >= (int) TraceLevel.Info;}
		}

		public bool TraceVerbose {
			get {return SwitchSetting >= (int) TraceLevel.Verbose;}
		}

		// .NET accepts values over 4; they're equivalent to TraceLevel.Verbose
		// For -1, .NET crashes.  (Oops!)  Other negative numbers work with an
		// equivalent to setting SwitchSetting to TraceLevel.Off.
		// The logic for the accessors will cope with values >= 4, so we'll just
		// check for negative numbers.
		protected override void OnSwitchSettingChanged()
		{
			if (SwitchSetting < 0)
				SwitchSetting = (int) TraceLevel.Off;
			else if (SwitchSetting > 4)
				SwitchSetting = (int) TraceLevel.Verbose;
		}

		protected override void OnValueChanged ()
		{
			SwitchSetting = (int) Enum.Parse (typeof (TraceLevel),
				Value, true);
		}
	}
}


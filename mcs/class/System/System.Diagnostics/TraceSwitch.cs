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

namespace System.Diagnostics
{
	public class TraceSwitch : Switch
	{
		public TraceSwitch(string displayName, string description)
			: base(displayName, description)
		{
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
		}
	}
}


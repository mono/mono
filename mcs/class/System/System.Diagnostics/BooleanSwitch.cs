//
// System.Diagnostics.BooleanSwitch.cs
//
// Author:
//      John R. Hicks (angryjohn69@nc.rr.com)
//      Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2001-2002
//

namespace System.Diagnostics
{
	/// <summary>
	/// Provides a simple on/off switch that controls debugging
	/// and tracing output
	/// </summary>
	public class BooleanSwitch : Switch
	{
		/// <summary>
		/// Initializes a new instance
		/// </summary>
		public BooleanSwitch(string displayName, string description)
			: base(displayName, description)
		{
		}

		/// <summary>
		/// Specifies whether the switch is enabled or disabled
		/// </summary>
		public bool Enabled {
			// On .NET, any non-zero value is true.  Only 0 is false.
			get {return SwitchSetting != 0;}
			set {
				SwitchSetting = Convert.ToInt32(value);
			}
		}
	}
}


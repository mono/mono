//
// System.Diagnostics.Switch.cs
//
// Comments from John R. Hicks <angryjohn69@nc.rr.com> original implementation 
// can be found at: /mcs/docs/apidocs/xml/en/System.Diagnostics
//
// Author:
//      John R. Hicks  (angryjohn69@nc.rr.com)
//      Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2001-2002
//

using System.Collections;

namespace System.Diagnostics
{
	public abstract class Switch
	{
		private string name = "";
		private string description = "";
		private int switchSetting = 0;

		// MS Behavior is that (quoting from MSDN for OnSwitchSettingChanged()):
		// 		"...It is invoked the first time a switch reads its value from the
		// 		configuration file..."
		// The docs + testing implies two things:
		// 	1. The value of the switch is not read in from the constructor
		// 	2. The value is instead read in on the first time get_SwitchSetting is
		// 		invoked
		// Assuming that OnSwitchSettingChanged() is invoked on a .config file
		// read and on all changes
		//
		// Thus, we need to keep track of whether or not switchSetting has been
		// initialized.  Using `switchSetting=-1' seems logical, but if someone
		// actually wants to use -1 as a switch value that would cause problems.
		private bool initialized = false;

		protected Switch(string displayName, string description)
		{
			this.name = displayName;
			this.description = description;
		}

		public string Description {
			get {return description;}
		}

		public string DisplayName {
			get {return name;}
		}

		protected int SwitchSetting {
			get {
				if (!initialized) {
					initialized = true;
					GetConfigFileSetting ();
					OnSwitchSettingChanged ();
				}
				return switchSetting;
			}
			set {
				if(switchSetting != value) {
					switchSetting = value;
					OnSwitchSettingChanged();
				}
				initialized = true;
			}
		}

		private void GetConfigFileSetting ()
		{
			// Load up the specified switch
			IDictionary d = 
				(IDictionary) DiagnosticsConfiguration.Settings ["switches"];
			if (d != null) {
				object o = d [name];
        try {
          switchSetting = int.Parse (o.ToString());
        }
        catch {
          switchSetting = 0;
        }
			}
		}

		protected virtual void OnSwitchSettingChanged()
		{
			// Do nothing.  This is merely provided for derived classes to know when
			// the value of SwitchSetting has changed.
		}
	}
}


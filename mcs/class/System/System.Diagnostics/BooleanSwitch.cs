//
// System.Diagnostics.BooleanSwitch.cs
//
// Author:
//      John R. Hicks (angryjohn69@nc.rr.com)
//
// (C) 2001
//

namespace System.Diagnostics
{
        /// <summary>
        /// Provides a simple on/off switch that controls debuggina
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
			SwitchSetting = (int)BooleanSwitchSetting.False;
                }

                // =================== Properties ===================

                /// <summary>
                /// Specifies whether the switch is enabled or disabled
                /// </summary>
                public bool Enabled
                {
                        get
                        {
                                if((int)BooleanSwitchSetting.False == SwitchSetting) {
					return false;
				}
				else {
					return true;
				}
                        }
                        set
                        {
                                if(value) {
					SwitchSetting = (int)BooleanSwitchSetting.True;
				}
				else {
					SwitchSetting = (int)BooleanSwitchSetting.False;
				}
                        }
                }

		private enum BooleanSwitchSetting : int {
			True = 1, False = 0
		}
        }

}

//
// System.Diagnostics.Switch.cs
//
// Author:
//      John R. Hicks  (angryjohn69@nc.rr.com)
//
// (C) 2001
//

namespace System.Diagnostics
{
        /// <summary>
        /// Abstract base class to create new debugging and tracing switches
        /// </summary>
        public abstract class Switch
        {
                private string desc = "";
                private string display_name = "";
                private int iSwitch;

                // ================= Constructors ===================
                /// <summary>
                /// Initialize a new instance
                /// </summary>
                protected Switch(string displayName, string description)
                {
                        display_name = displayName;
                        desc = description;
                }

                /// <summary>
                /// Allows an Object to attempt to free resources and
                /// perform cleanup before the Object is reclaimed
                /// by the Garbage Collector
                /// </summary>
                ~Switch()
                {
                }

                // ================ Instance Methods ================

                // ==================== Properties ==================

                /// <summary>
                /// Returns a description of the switch
                /// </summary>
                public string Description
                {
                        get
                        {
                                return desc;
                        }
                }

                /// <summary>
                /// Returns a name used to identify the switch
                /// </summary>
                public string DisplayName
                {
                        get
                        {
                                return display_name;
                        }
                }

                /// <summary>
                /// Gets or sets the current setting for this switch
                /// </summary>
                protected int SwitchSetting
                {
                        get
                        {
                                return iSwitch;
                        }
                        set
                        {
				if(iSwitch != value) 
				{
					
				}
                                iSwitch = value;
                        }
                }

                /// <summary>
                /// Raises the SwitchSettingChanged event
                /// </summary>
		protected virtual void OnSwitchSettingChanged()
		{
			// TODO: implement me
		}
        }
}

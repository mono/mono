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
                private bool enabled = false;

                /// <summary>
                /// Initializes a new instance
                /// </summary>
                public BooleanSwitch(string displayName, string description)
                        : base(displayName, description)
                {
                }

                // =================== Properties ===================

                /// <summary>
                /// Specifies whether the switch is enabled or disabled
                /// </summary>
                public bool Enabled
                {
                        get
                        {
                                return enabled;
                        }
                        set
                        {
                                enabled = value;
                        }
                }

                // ================= Event Handlers =================

        }
}

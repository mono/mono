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

using System.Collections;
using System.Collections.Specialized;
#if CONFIGURATION_DEP
using System.Configuration;
#endif

namespace System.Diagnostics
{
	public abstract class Switch
	{
		private string name;
		private string description;
		private int switchSetting;
		private string value;
		private string defaultSwitchValue;
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
		private bool initialized;

		protected Switch(string displayName, string description)
		{
			this.name = displayName;
			this.description = description;
		}

		protected Switch(string displayName, string description, string defaultSwitchValue)
			: this (displayName, description)
		{
			this.defaultSwitchValue = defaultSwitchValue;
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

		StringDictionary attributes = new StringDictionary ();

#if XML_DEP
		[System.Xml.Serialization.XmlIgnore]
#endif
		public StringDictionary Attributes {
			get { return attributes; }
		}

		protected string Value {
			get { return value; }
			set {
				this.value = value;
#if CONFIGURATION_DEP
				try {
					OnValueChanged ();
				} catch (Exception ex) {
					string msg = string.Format ("The config "
						+ "value for Switch '{0}' was "
						+ "invalid.", DisplayName);

					throw new ConfigurationErrorsException (
						msg, ex);
				}
#else
				OnValueChanged ();
#endif
			}
		}

		protected internal virtual string [] GetSupportedAttributes ()
		{
			return null;
		}

		protected virtual void OnValueChanged ()
		{
		}

		private void GetConfigFileSetting ()
		{
#if !MOBILE
			IDictionary d = (IDictionary) DiagnosticsConfiguration.Settings ["switches"];
			
			// Load up the specified switch
			if (d != null) {
				if (d.Contains (name)) {
#if CONFIGURATION_DEP
					Value = d [name] as string;
#else
					switchSetting = (int) d [name];
#endif
					return;
				}
			}
#endif  // !MOBILE

			if (defaultSwitchValue != null) {
				value = defaultSwitchValue;
				OnValueChanged ();
			}
		}

		protected virtual void OnSwitchSettingChanged()
		{
			// Do nothing.  This is merely provided for derived classes to know when
			// the value of SwitchSetting has changed.
		}
	}
}

//
// System.Diagnostics.Switch.cs
//
// Comments from John R. Hicks <angryjohn69@nc.rr.com> original implementation 
// can be found at: /mcs/docs/apidocs/xml/en/System.Diagnostics
//
// Author:
//      John R. Hicks  (angryjohn69@nc.rr.com)
//
// (C) 2001
//

namespace System.Diagnostics
{
	public abstract class Switch
	{
		private string desc = "";
		private string display_name = "";
		private int iSwitch;

		// ================= Constructors ===================
		protected Switch(string displayName, string description)
		{
			display_name = displayName;
			desc = description;
		}

		~Switch()
		{
		}

		// ================ Instance Methods ================

		// ==================== Properties ==================

		public string Description {
			get {return desc;}
		}

		public string DisplayName {
			get {return display_name;}
		}

		protected int SwitchSetting {
			get {return iSwitch;}
			set {
				if(iSwitch != value) {
					iSwitch = value;
					OnSwitchSettingChanged();
				}
			}
		}

		[MonoTODO]
		protected virtual void OnSwitchSettingChanged()
		{
			// TODO: implement me
		}
	}
}


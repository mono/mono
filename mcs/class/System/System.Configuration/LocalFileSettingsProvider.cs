//
// System.Configuration.LocalFileSettingsProvider.cs
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// (C) 2005, 2007 Novell, Inc (http://www.novell.com)
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

#if CONFIGURATION_DEP

using System;
using System.Collections.Specialized;

namespace System.Configuration
{
	public class LocalFileSettingsProvider : SettingsProvider, IApplicationSettingsProvider
	{
		CustomizableFileSettingsProvider impl;

		public LocalFileSettingsProvider ()
		{
			impl = new CustomizableFileSettingsProvider ();
		}

		[MonoTODO]
		public SettingsPropertyValue GetPreviousVersion (SettingsContext context,
								 SettingsProperty property)
		{
			return impl.GetPreviousVersion (context, property);
		}


		[MonoTODO]
		public override SettingsPropertyValueCollection GetPropertyValues (SettingsContext context,
										   SettingsPropertyCollection properties)
		{
			return impl.GetPropertyValues (context, properties);
		}

#if CONFIGURATION_DEP
		public override void Initialize (string name,
						 NameValueCollection values)
		{
			if (name == null)
				name = "LocalFileSettingsProvider";
			if (values != null)
				impl.ApplicationName = values ["applicationName"];

			base.Initialize (name, values);
		}
#endif

		[MonoTODO]
		public void Reset (SettingsContext context)
		{
			impl.Reset (context);
		}

		[MonoTODO]
		public override void SetPropertyValues (SettingsContext context,
							SettingsPropertyValueCollection values)
		{
			impl.SetPropertyValues (context, values);
		}

		[MonoTODO]
		public void Upgrade (SettingsContext context,
				     SettingsPropertyCollection properties)
		{
			impl.Upgrade (context, properties);
		}

		public override string ApplicationName {
			get { return impl.ApplicationName; }
			set { impl.ApplicationName = value; }
		}

/*
		bool IsUserSetting (SettingsProperty prop)
		{
#if CONFIGURATION_DEP
			if (prop.Attributes.ContainsKey (typeof (UserScopedSettingAttribute)))
				return true;
			else if (prop.Attributes.ContainsKey (typeof (ApplicationScopedSettingAttribute)))
				return false;
			else
				throw new ConfigurationErrorsException (
							String.Format ("The setting '{0}' does not have either an ApplicationScopedSettingAttribute or UserScopedSettingAttribute.",
								       prop.Name));
#else
			return false;
#endif
		}
*/		
	}

}

#endif

//
// System.Web.UI.WebControls.SettingsBase.cs
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections;
using System.ComponentModel;

namespace System.Configuration
{

	public abstract class SettingsBase
	{
		protected SettingsBase ()
		{
		}

		public void Initialize (SettingsContext context,
					SettingsPropertyCollection properties,
					SettingsProviderCollection providers)
		{
			this.context = context;
			this.properties = properties;
			this.providers = providers;
			// values do not seem to be reset here!! (otherwise one of the SettingsBaseTest will fail)
		}

		public virtual void Save ()
		{
			if (sync)
				lock (this)
					SaveCore ();
			else
				SaveCore ();
		}

		void SaveCore ()
		{
			//
			// Copied from ApplicationSettingsBase
			//
#if (CONFIGURATION_DEP)
			/* ew.. this needs to be more efficient */
			foreach (SettingsProvider provider in Providers) {
				SettingsPropertyValueCollection cache = new SettingsPropertyValueCollection ();

				foreach (SettingsPropertyValue val in PropertyValues) {
					if (val.Property.Provider == provider)
						cache.Add (val);
				}

				if (cache.Count > 0)
					provider.SetPropertyValues (Context, cache);
			}
#endif
		}

		public static SettingsBase Synchronized (SettingsBase settingsBase)
		{
			settingsBase.sync = true;
			return settingsBase;
		}

		public virtual SettingsContext Context {
			get { return context; }
		}

		[Browsable (false)]
		public bool IsSynchronized {
			get { return sync; }
		}

		public virtual object this [ string propertyName ] {
			get
			{
				if (sync)
					lock (this) {
						return GetPropertyValue (propertyName);
					}
				else
					return GetPropertyValue (propertyName);
			}
			set
			{
				if (sync)
					lock (this) {
						SetPropertyValue (propertyName, value);
					}
				else
					SetPropertyValue (propertyName, value);
			}
		}

		public virtual SettingsPropertyCollection Properties {
			get {
				// It seems that Properties.IsSynchronized is
				// nothing to do with this.IsSynchronized.
				return properties;
			}
		}

		public virtual SettingsPropertyValueCollection PropertyValues {
			get {
				if (sync) {
					lock (this) {
						return values;
					}
				} else {
					return values;
				}
			}
		}

		public virtual SettingsProviderCollection Providers {
			get {
				return providers;
			}
		}

		object GetPropertyValue (string propertyName)
		{
			SettingsProperty prop = null;
			if (Properties == null || (prop = Properties [propertyName]) == null)
				throw new SettingsPropertyNotFoundException (
					string.Format ("The settings property '{0}' was not found", propertyName));

			if (values [propertyName] == null)
				foreach (SettingsPropertyValue v in prop.Provider.GetPropertyValues (Context, Properties))
					values.Add (v);

			return PropertyValues [propertyName].PropertyValue;
		}

		void SetPropertyValue (string propertyName, object value)
		{
			SettingsProperty prop = null;
			if (Properties == null || (prop = Properties [propertyName]) == null)
				throw new SettingsPropertyNotFoundException (
					string.Format ("The settings property '{0}' was not found", propertyName));

			if (prop.IsReadOnly)
				throw new SettingsPropertyIsReadOnlyException (
					string.Format ("The settings property '{0}' is read only", propertyName));

			if (prop.PropertyType != value.GetType ())
				throw new SettingsPropertyWrongTypeException (
					string.Format ("The value supplied is of a type incompatible with the settings property '{0}'", propertyName));

			PropertyValues [propertyName].PropertyValue = value;
		}

		bool sync;
		SettingsContext context;
		SettingsPropertyCollection properties;
		SettingsProviderCollection providers;
		SettingsPropertyValueCollection values = new SettingsPropertyValueCollection ();
	}
}


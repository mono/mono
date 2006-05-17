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

#if NET_2_0
using System;
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
		}

		public virtual void Save ()
		{
			throw new NotImplementedException ();
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
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
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
						return GetPropertyValues ();
					}
				}
				else
					return GetPropertyValues ();
			}
		}

		SettingsPropertyValueCollection GetPropertyValues ()
		{
			SettingsPropertyValueCollection col = new SettingsPropertyValueCollection ();

			foreach (SettingsProperty prop in properties)
			{
				col.Add (new SettingsPropertyValue (prop));
			}

			return col;
		}

		public virtual SettingsProviderCollection Providers {
			get {
				return providers;
			}
		}

		bool sync;
		SettingsContext context;
		SettingsPropertyCollection properties;
		SettingsProviderCollection providers;
	}
}

#endif

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

		[MonoTODO]
		public static SettingsBase Synchronized (SettingsBase settingsBase)
		{
			return new SyncSettingsBase (settingsBase);
		}

		public virtual SettingsContext Context {
			get { return context; }
		}

		[Browsable (false)]
		public bool IsSynchronized {
			get { return false; }
		}

		public virtual object this [ string propertyName ] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public virtual SettingsPropertyCollection Properties {
			get { return properties; }
		}

		public virtual SettingsPropertyValueCollection PropertyValues {
			get {
				SettingsPropertyValueCollection col = new SettingsPropertyValueCollection ();

				foreach (SettingsProperty prop in properties)
				{
					col.Add (new SettingsPropertyValue (prop));
				}

				return col;
			}
		}

		public virtual SettingsProviderCollection Providers {
			get {
				return providers;
			}
		}

		SettingsContext context;
		SettingsPropertyCollection properties;
		SettingsProviderCollection providers;

		private class SyncSettingsBase : SettingsBase
		{
			SettingsBase host;
			object syncRoot;

			public SyncSettingsBase (SettingsBase host)
			{
				this.host = host;
				syncRoot = host;
			}

			public override void Save ()
			{
				lock (syncRoot) {
					host.Save ();
				}
			}

			public override object this [ string propertyName ] {
				get { return host[propertyName]; }
				set {
					lock (syncRoot) {
						host[propertyName] = value;
					}
				}
			}

			public override SettingsPropertyCollection Properties {
				get {
					SettingsPropertyCollection props;

					lock (syncRoot) {
						props = host.Properties;
					}

					return props;
				}
			}

			public virtual SettingsPropertyValueCollection PropertyValues {
				get {
					SettingsPropertyValueCollection vals;

					lock (syncRoot) {
						vals = host.PropertyValues;
					}

					return vals;
				}
			}

			public virtual SettingsProviderCollection Providers {
				get {
					SettingsProviderCollection prov;

					lock (syncRoot) {
						prov = host.Providers;
					}

					return prov;
				}
			}
		}
	}
}

#endif

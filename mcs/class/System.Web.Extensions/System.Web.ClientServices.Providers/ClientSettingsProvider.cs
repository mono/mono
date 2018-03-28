//
// System.Web.ClientServices.Providers.ClientSettingsProvider
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2008 Novell, Inc
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

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Net;
using System.Security;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace System.Web.ClientServices.Providers
{
	public class ClientSettingsProvider : SettingsProvider, IApplicationSettingsProvider
	{
#pragma warning disable 67
		public event EventHandler <SettingsSavedEventArgs> SettingsSaved;
#pragma warning restore 67
		
		public static string ServiceUri {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		public override string ApplicationName {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		public ClientSettingsProvider ()
		{
			throw new NotImplementedException ();
		}
		
		public static SettingsPropertyCollection GetPropertyMetadata (string serviceUri)
		{
			throw new NotImplementedException ();
		}

		public SettingsPropertyValue GetPreviousVersion (SettingsContext context, SettingsProperty property)
		{
			throw new NotImplementedException ();
		}
		
		public override SettingsPropertyValueCollection GetPropertyValues (SettingsContext context, SettingsPropertyCollection propertyCollection)
		{
			throw new NotImplementedException ();
		}
		
		public override void Initialize (string name, NameValueCollection config)
		{
			throw new NotImplementedException ();
		}
		
		public void Reset (SettingsContext context)
		{
			throw new NotImplementedException ();
		}
		
		public override void SetPropertyValues (SettingsContext context, SettingsPropertyValueCollection propertyValueCollection)
		{
			throw new NotImplementedException ();
		}

		public void Upgrade (SettingsContext context, SettingsPropertyCollection properties)
		{
			throw new NotImplementedException ();
		}
	}
}

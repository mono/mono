//
// System.Web.UI.WebControls.SettingsProviderCollection.cs
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
#if (CONFIGURATION_DEP)
using System.Configuration.Provider;
#endif

namespace System.Configuration
{
	public class SettingsProviderCollection
#if (CONFIGURATION_DEP)
		: ProviderCollection
#endif
	{
		public SettingsProviderCollection ()
		{
		}

#if (CONFIGURATION_DEP)
		public override void Add (ProviderBase provider)
		{
			if (!(provider is SettingsProvider))
				throw new ArgumentException ("SettingsProvider is expected");
			if (String.IsNullOrEmpty (provider.Name))
				throw new ArgumentException ("Provider name cannot be null or empty");
			base.Add (provider);
		}

		public new SettingsProvider this [ string name ] { 
			get {
				return (SettingsProvider) base [ name ];
			}
		}
#endif
	}
}


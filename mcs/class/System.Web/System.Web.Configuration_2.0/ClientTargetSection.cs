//
// System.Web.Configuration.ClientTargetSection
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Configuration;

#if NET_2_0

namespace System.Web.Configuration {

	public sealed class ClientTargetSection : ConfigurationSection
	{
		static ConfigurationProperty clientTargetsProp;

		static ConfigurationPropertyCollection properties;

		static ClientTargetSection ()
		{
			clientTargetsProp = new ConfigurationProperty (null, typeof (ClientTargetCollection), null,
								       ConfigurationPropertyOptions.IsDefaultCollection |
								       ConfigurationPropertyOptions.IsRequired);

			properties = new ConfigurationPropertyCollection ();
			properties.Add (clientTargetsProp);
		}
		
		[ConfigurationProperty ("", Options = ConfigurationPropertyOptions.IsDefaultCollection | ConfigurationPropertyOptions.IsRequired)]
		public ClientTargetCollection ClientTargets {
			get { return (ClientTargetCollection) base [clientTargetsProp];}
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}

#endif

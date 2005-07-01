//
// System.Web.Configuration.MembershipSection.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.Configuration;
using System.Web.Security;
using System.ComponentModel;

namespace System.Web.Configuration
{
	public sealed class MembershipSection: InternalSection
	{
		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("defaultProvider", DefaultValue = "AspNetSqlMembershipProvider")]
		public string DefaultProvider {
			get { return (string) base ["defaultProvider"]; }
			set { base ["defaultProvider"] = value; }
		}
		
		[ConfigurationProperty ("hashAlgorithmType", DefaultValue = "")]
		public string HashAlgorithmType {
			get { return (string) base ["hashAlgorithmType"]; }
			set { base ["hashAlgorithmType"] = value; }
		}
		
		[ConfigurationProperty ("providers")]
		public ProviderSettingsCollection Providers {
			get { return (ProviderSettingsCollection) base ["providers"]; }
		}
		
		[TypeConverter (typeof(TimeSpanMinutesConverter))]
		[ConfigurationProperty ("userIsOnlineTimeWindow", DefaultValue = "00:15:00")]
		[TimeSpanValidator (MinValueString = "00:01:00")]
		public TimeSpan UserIsOnlineTimeWindow {
			get { return (TimeSpan) base ["userIsOnlineTimeWindow"]; }
			set { base ["userIsOnlineTimeWindow"] = value; }
		}
	}
}

#endif

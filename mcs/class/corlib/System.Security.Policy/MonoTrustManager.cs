//
// System.Security.Policy.MonoTrustManager internal class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Security;
using System.Security.Permissions;

namespace System.Security.Policy {

	// this is temporary until we include one in SWF for 2.0

	internal class MonoTrustManager : IApplicationTrustManager {

		private const string tag = "IApplicationTrustManager";

		[SecurityPermission (SecurityAction.Demand, ControlPolicy = true)]
		public ApplicationTrust DetermineApplicationTrust (ActivationContext activationContext, TrustManagerContext context)
		{
			if (activationContext == null)
				throw new ArgumentNullException ("activationContext");
			return null;
		}

		public void FromXml (SecurityElement e)
		{
			if (e == null)
				throw new ArgumentNullException ("e");
			if (e.Tag != tag)
				throw new ArgumentException ("e", Locale.GetText ("Invalid XML tag."));
			// nothing more to do in this case
		}

		public SecurityElement ToXml ()
		{
			SecurityElement se = new SecurityElement (tag);
			se.AddAttribute ("class", typeof (MonoTrustManager).AssemblyQualifiedName);
			se.AddAttribute ("version", "1");
			return se;
		}
	}
}

#endif

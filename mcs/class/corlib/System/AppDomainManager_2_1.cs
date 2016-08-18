//
// System.AppDomainManager class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2005,2009 Novell, Inc (http://www.novell.com)
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

#if MOBILE

using System.Runtime.InteropServices;
using System.Security;

namespace System {

#if MONO_FEATURE_MULTIPLE_APPDOMAINS
	[ComVisible (true)]
	public class AppDomainManager {

		public AppDomainManager ()
		{
		}

		[MonoTODO]
		public virtual void InitializeNewDomain (AppDomainSetup appDomainInfo)
		{
		}

		[MonoTODO]
		public virtual bool CheckSecuritySettings (SecurityState state)
		{
			return (state != null);
		}
	}
#else
	[Obsolete ("AppDomainManager is not supported on the current platform.", true)]
	public class AppDomainManager {

		public AppDomainManager ()
		{
			get { throw new PlatformNotSupportedException ("AppDomainManager is not supported on the current platform."); }
		}

		public virtual void InitializeNewDomain (AppDomainSetup appDomainInfo)
		{
			get { throw new PlatformNotSupportedException ("AppDomainManager is not supported on the current platform."); }
		}

		public virtual bool CheckSecuritySettings (SecurityState state)
		{
			get { throw new PlatformNotSupportedException ("AppDomainManager is not supported on the current platform."); }
		}
	}
#endif // MONO_FEATURE_MULTIPLE_APPDOMAINS
}

#endif


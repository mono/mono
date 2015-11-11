//
// SecurityContext.cs: This is CAS disabled SecurityContext version
//                     it does nothing but has same public API
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2015 Xamarin Inc (http://www.xamarin.com)
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

#if !FEATURE_COMPRESSEDSTACK

using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;

namespace System.Security {

	public sealed class SecurityContext : IDisposable
	{
		private SecurityContext ()
		{
		}

		public SecurityContext CreateCopy ()
		{
			return this;
		}

		static public SecurityContext Capture ()
		{
			return new SecurityContext ();
		}
		
		public void Dispose ()
		{
		}

		static public bool IsFlowSuppressed ()
		{
			return false;
		} 

		static public bool IsWindowsIdentityFlowSuppressed ()
		{
			return false;
		}

		static public void RestoreFlow ()
		{
		}

		// if you got the context then you can use it
		[SecurityPermission (SecurityAction.Assert, ControlPrincipal = true)]
		[SecurityPermission (SecurityAction.LinkDemand, Infrastructure = true)]
		static public void Run (SecurityContext securityContext, ContextCallback callback, object state)
		{
			callback (state);
		}

		[SecurityPermission (SecurityAction.LinkDemand, Infrastructure = true)]
		static public AsyncFlowControl SuppressFlow ()
		{
			throw new NotSupportedException ();
		}

		static public AsyncFlowControl SuppressFlowWindowsIdentity ()
		{
			throw new NotSupportedException ();
		}
	}
}

#endif
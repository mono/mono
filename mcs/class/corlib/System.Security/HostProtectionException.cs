//
// System.Security.HostProtectionException class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0 && !DISABLE_SECURITY

using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Security {

	[Serializable]
	[ComVisible (true)]
	[MonoTODO ("Not supported in the runtime")]
	public class HostProtectionException : SystemException {

		private HostProtectionResource _protected;
		private HostProtectionResource _demanded;

		public HostProtectionException ()
		{
		}

		public HostProtectionException (string message)
			: base (message)
		{
		}

		public HostProtectionException (string message, Exception e)
			: base (message, e)
		{
		}

		public HostProtectionException (string message, HostProtectionResource protectedResources, HostProtectionResource demandedResources)
			: base (message)
		{
			this._protected = protectedResources;
			this._demanded = demandedResources;
		}

		protected HostProtectionException (SerializationInfo info, StreamingContext context)
		{
			GetObjectData (info, context);
		}

		public HostProtectionResource DemandedResources {
			get { return _demanded; }
		}

		public HostProtectionResource ProtectedResources {
			get { return _protected; }
		}

		[MonoTODO]
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");
		}

		[MonoTODO]
		public override string ToString ()
		{
			return base.ToString ();
		}
	}
}

#endif

//
// System.Threading.HostExecutionContextManager class
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

#if NET_2_0

using System.Security.Permissions;
using System.Runtime.ConstrainedExecution;

namespace System.Threading {

	public class HostExecutionContextManager {

		public HostExecutionContextManager ()
		{
		}

		[MonoTODO]
		public virtual HostExecutionContext Capture ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.MayFail)]
		public virtual void Revert (object previousState)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		[SecurityPermission (SecurityAction.LinkDemand, Infrastructure = true)]
		public virtual object SetHostExecutionContext (HostExecutionContext hostExecutionContext)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif

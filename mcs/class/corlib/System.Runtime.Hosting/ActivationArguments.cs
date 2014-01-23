//
// System.Runtime.Hosting.ActivationArguments class
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

using System.Runtime.InteropServices;
using System.Security.Policy;

namespace System.Runtime.Hosting {

	[Serializable]
	[ComVisible (true)]
	public sealed class ActivationArguments
#if NET_4_0
		: EvidenceBase
#endif
	{

		private ActivationContext _context;
		private ApplicationIdentity _identity;
		private string[] _data;

		public ActivationArguments (ActivationContext activationData)
		{
			if (activationData == null)
				throw new ArgumentNullException ("activationData");

			_context = activationData;
			_identity = activationData.Identity;
		}

		public ActivationArguments (ApplicationIdentity applicationIdentity)
		{
			if (applicationIdentity == null)
				throw new ArgumentNullException ("applicationIdentity");

			_identity = applicationIdentity;
		}

		public ActivationArguments (ActivationContext activationContext, string[] activationData)
		{
			if (activationContext == null)
				throw new ArgumentNullException ("activationContext");

			_context = activationContext;
			_identity = activationContext.Identity;
			_data = activationData;
		}

		public ActivationArguments (ApplicationIdentity applicationIdentity, string[] activationData)
		{
			if (applicationIdentity == null)
				throw new ArgumentNullException ("applicationIdentity");

			_identity = applicationIdentity;
			_data = activationData;
		}

		// properties

		public ActivationContext ActivationContext {
			get { return _context; }
		}

		public string[] ActivationData {
			get { return _data; }
		}

		public ApplicationIdentity ApplicationIdentity {
			get { return _identity; }
		}
	}
}


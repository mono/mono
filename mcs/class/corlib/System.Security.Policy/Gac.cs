//
// System.Security.Policy.Gac
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


using System.Security.Permissions;
using System.Runtime.InteropServices;

namespace System.Security.Policy {

	[Serializable]
	[ComVisible (true)]
	public sealed class GacInstalled :
#if NET_4_0
		EvidenceBase,
#endif
		IIdentityPermissionFactory, IBuiltInEvidence {

		public GacInstalled ()
		{
		}

		public object Copy ()
		{
			return (object) new GacInstalled ();
		}

		public IPermission CreateIdentityPermission (Evidence evidence)
		{
			return new GacIdentityPermission ();
		}

		public override bool Equals (object o)
		{
			if (o == null)
				return false;
			return (o is GacInstalled);
		}

		public override int GetHashCode ()
		{
			return 0; // as documented
		}

		public override string ToString ()
		{
			SecurityElement se = new SecurityElement (GetType ().FullName);
			se.AddAttribute ("version", "1");
			return se.ToString ();
		}

		// IBuiltInEvidence

		int IBuiltInEvidence.GetRequiredSize (bool verbose)
		{
			return 1;	// LAMESPEC
		}

		int IBuiltInEvidence.InitFromBuffer (char[] buffer, int position)
		{
			return position;
		}

		int IBuiltInEvidence.OutputToBuffer (char[] buffer, int position, bool verbose)
		{
			buffer [position] = '\t';
			return position + 1;
		}
	}
}

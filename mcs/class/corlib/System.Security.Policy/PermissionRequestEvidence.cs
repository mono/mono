//
// System.Security.Policy.PermissionRequestEvidence.cs
//
// Authors:
//      Nick Drochak (ndrochak@gol.com)
//
// (C) 2003 Nick Drochak
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Security;
using System.Text;

namespace System.Security.Policy
{
	[Serializable]
	public sealed class PermissionRequestEvidence : IBuiltInEvidence {
		PermissionSet requested, optional, denied;

		public PermissionRequestEvidence(PermissionSet requested, 
				PermissionSet optional, PermissionSet denied) {
			this.requested = requested;
			this.optional = optional;
			this.denied = denied;
		}

		public PermissionSet DeniedPermissions {
			get {return denied;}
		}

		public PermissionSet OptionalPermissions {
			get {return optional;}
		}

		public PermissionSet RequestedPermissions {
			get {return requested;}
		}

		public PermissionRequestEvidence Copy ()
		{
			return new PermissionRequestEvidence (requested, optional, denied);
		}

		public override string ToString () 
		{
			SecurityElement se = new SecurityElement ("System.Security.Policy.PermissionRequestEvidence");
			se.AddAttribute ("version", "1");

			if (requested != null) {
				SecurityElement requestElement = new SecurityElement ("Request");
				requestElement.AddChild (requested.ToXml ());
				se.AddChild (requestElement);
			}
			if (optional != null) {
				SecurityElement optionalElement = new SecurityElement ("Optional");
				optionalElement.AddChild (optional.ToXml ());
				se.AddChild (optionalElement);
			}
			if (denied != null) {
				SecurityElement deniedElement = new SecurityElement ("Denied");
				deniedElement.AddChild (denied.ToXml ());
				se.AddChild (deniedElement);
			}
			return se.ToString ();
		}

		// interface IBuiltInEvidence

		[MonoTODO]
		int IBuiltInEvidence.GetRequiredSize (bool verbose) 
		{
			return 0;
		}

		[MonoTODO]
		int IBuiltInEvidence.InitFromBuffer (char [] buffer, int position) 
		{
			return 0;
		}

		[MonoTODO]
		int IBuiltInEvidence.OutputToBuffer (char [] buffer, int position, bool verbose) 
		{
			return 0;
		}

	}
}

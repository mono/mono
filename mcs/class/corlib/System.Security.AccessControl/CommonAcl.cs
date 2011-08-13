//
// System.Security.AccessControl.CommonAcl implementation
//
// Authors:
//	Dick Porter  <dick@ximian.com>
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2006-2007 Novell, Inc (http://www.novell.com)
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

using System.Collections.Generic;
using System.Security.Principal;

namespace System.Security.AccessControl {
	/* NB: Note the Remarks section in the CommonAcl class docs
	 * concerning ACE management
	 */
	public abstract class CommonAcl : GenericAcl
	{
		const int default_capacity = 10; // FIXME: not verified

		internal CommonAcl (bool isContainer, bool isDS, byte revision)
			: this (isContainer, isDS, revision, default_capacity)
		{
		}

		internal CommonAcl (bool isContainer, bool isDS, byte revision, int capacity)
		{
			this.is_container = isContainer;
			this.is_ds = isDS;
			this.rev = revision;
			raw_acl = new RawAcl(revision, capacity);
		}

		bool is_container, is_ds;
		byte rev;
		RawAcl raw_acl;

		public override sealed int BinaryLength
		{
			get {
				return raw_acl.BinaryLength;
			}
		}

		public override sealed int Count {
			get { return raw_acl.Count; }
		}

		[MonoTODO]
		public bool IsCanonical
		{
			get {
				throw new NotImplementedException ();
			}
		}

		public bool IsContainer {
			get { return is_container; }
		}
		
		public bool IsDS {
			get { return is_ds; }
		}

		public override sealed GenericAce this[int index]
		{
			get { return raw_acl [index]; }
			set { raw_acl [index] = value; }
		}
		
		public override sealed byte Revision {
			get { return rev; }
		}
		
		public override sealed void GetBinaryForm (byte[] binaryForm,
							   int offset)
		{
			raw_acl.GetBinaryForm(binaryForm, offset);
		}
		
		[MonoTODO]
		public void Purge (SecurityIdentifier sid)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveInheritedAces ()
		{
			throw new NotImplementedException ();
		}

		internal RawAcl RawAcl
		{
			get { return raw_acl; }
		}
		
		internal override string GetSddlForm(ControlFlags sdFlags, bool isDacl)
		{
			return raw_acl.GetSddlForm(sdFlags, isDacl);
		}
	}
}


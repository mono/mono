//
// System.Security.AccessControl.CustomAce implementation
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

using System.Collections;

namespace System.Security.AccessControl {
	public sealed class CustomAce : GenericAce {
		private byte[] opaque;

		public CustomAce (AceType type, AceFlags flags, byte[] opaque)
			: base(type, flags)
		{
			/* FIXME: check length of opaque >
			 * MaxOpaqueLength or !multiple of 4
			 */			
			SetOpaque (opaque);
		}

		[MonoTODO]
		public static readonly int MaxOpaqueLength;

		[MonoTODO]
		public override int BinaryLength {
			get {
				throw new NotImplementedException ();
			}
		}

		public int OpaqueLength {
			get { return opaque.Length; }
		}

		[MonoTODO]
		public override void GetBinaryForm (byte[] binaryForm,
		                                    int offset)
		{
			throw new NotImplementedException ();
		}

		public byte[] GetOpaque ()
		{
			return (byte[])opaque.Clone ();
		}

		public void SetOpaque (byte[] opaque)
		{
			if (opaque == null)
				this.opaque = null;
			else
				this.opaque = (byte[])opaque.Clone ();
		}

		internal override string GetSddlForm ()
		{
			throw new NotSupportedException ();
		}
	}
}
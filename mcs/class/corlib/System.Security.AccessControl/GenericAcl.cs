//
// System.Security.AccessControl.GenericAcl implementation
//
// Author:
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
	public abstract class GenericAcl : ICollection, IEnumerable
	{
		public static readonly byte AclRevision;
		public static readonly byte AclRevisionDS;
		public static readonly int MaxBinaryLength;
		
		static GenericAcl ()
		{
			// FIXME: they are likely platform dependent (on windows)
			AclRevision = 2;
			AclRevisionDS = 4;
			MaxBinaryLength = 0x10000;
		}

		protected GenericAcl ()
		{
		}
		
		public abstract int BinaryLength { get; }
		
		public abstract int Count { get; }
		
		public bool IsSynchronized {
			get { return false; }
		}
		
		public abstract GenericAce this [int index] {
			get;
			set;
		}
		
		public abstract byte Revision { get; }
		
		public object SyncRoot {
			get { return this; }
		}
		
		public void CopyTo (GenericAce [] array, int index)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (index < 0 || array.Length - index < Count)
				throw new ArgumentOutOfRangeException ("index", "Index must be non-negative integer and must not exceed array length - count");
			for (int i = 0; i < Count; i++)
				array [i + index] = this [i];
		}

		void ICollection.CopyTo (Array array, int index)
		{
			CopyTo ((GenericAce []) array, index);
		}
		
		public abstract void GetBinaryForm (byte[] binaryForm, int offset);
		
		public AceEnumerator GetEnumerator ()
		{
			return new AceEnumerator (this);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
		
		internal abstract string GetSddlForm(ControlFlags sdFlags,
		                                     bool isDacl);
	}
}


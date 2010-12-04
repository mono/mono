//
// System.Security.AccessControl.RawAcl implementation
//
// Authors:
//	Dick Porter  <dick@ximian.com>
//	Atsushi Enomoto  <atsushi@ximian.com>
//	Kenneth Bell
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
using System.Text;

namespace System.Security.AccessControl {
	public sealed class RawAcl : GenericAcl
	{
		private byte revision;
		private List<GenericAce> list;

		public RawAcl (byte revision, int capacity)
		{
			this.revision = revision;
			list = new List<GenericAce> (capacity);
		}
		
		public RawAcl (byte [] binaryForm, int offset)
		{
			if (binaryForm == null)
				throw new ArgumentNullException("binaryForm");
			
			if (offset < 0 || offset > binaryForm.Length - 8)
				throw new ArgumentOutOfRangeException("offset", offset, "Offset out of range");
			
			revision = binaryForm[offset];
			if (revision != AclRevision && revision != AclRevisionDS)
				throw new ArgumentException("Invalid ACL - unknown revision", "binaryForm");
			
			int binaryLength = ReadUShort(binaryForm, offset + 2);
			if (offset > binaryForm.Length - binaryLength)
				throw new ArgumentException("Invalid ACL - truncated", "binaryForm");
			
			int pos = offset + 8;
			int numAces = ReadUShort(binaryForm, offset + 4);
			list = new List<GenericAce>(numAces);
			for (int i = 0; i < numAces; ++i) {
				GenericAce newAce = GenericAce.CreateFromBinaryForm(binaryForm, pos);
				list.Add(newAce);
				pos += newAce.BinaryLength;
			}
		}
		
		internal RawAcl(byte revision, List<GenericAce> aces)
		{
			this.revision = revision;
			this.list = aces;
		}
		
		public override int BinaryLength
		{
			get {
				int len = 8;
				foreach(var ace in list)
				{
					len += ace.BinaryLength;
				}
				return len;
			}
		}

		public override int Count {
			get { return list.Count; }
		}

		public override GenericAce this [int index]
		{
			get { return list [index]; }
			set { list [index] = value; }
		}
		
		public override byte Revision {
			get { return revision; }
		}

		public override void GetBinaryForm (byte[] binaryForm,
		                                    int offset)
		{
			if(binaryForm == null)
				throw new ArgumentNullException("binaryForm");
			
			if(offset < 0
			   || offset > binaryForm.Length - BinaryLength)
				throw new ArgumentException("Offset out of range", "offset");
			
			binaryForm[offset] = Revision;
			binaryForm[offset + 1] = 0;
			WriteUShort((ushort)BinaryLength, binaryForm,
			            offset + 2);
			WriteUShort((ushort)list.Count, binaryForm,
			            offset + 4);
			WriteUShort(0, binaryForm, offset + 6);
			
			int pos = offset + 8;
			foreach(var ace in list)
			{
				ace.GetBinaryForm(binaryForm, pos);
				pos += ace.BinaryLength;
			}
		}

		public void InsertAce (int index, GenericAce ace)
		{
			if (ace == null)
				throw new ArgumentNullException ("ace");
			list.Insert (index, ace);
		}
		
		public void RemoveAce (int index)
		{
			list.RemoveAt (index);
		}
		
		internal override string GetSddlForm(ControlFlags sdFlags,
		                                     bool isDacl)
		{
			StringBuilder result = new StringBuilder();
			
			if(isDacl) {
				if((sdFlags & ControlFlags.DiscretionaryAclProtected) != 0)
					result.Append("P");
				if((sdFlags & ControlFlags.DiscretionaryAclAutoInheritRequired) != 0)
					result.Append("AR");
				if((sdFlags & ControlFlags.DiscretionaryAclAutoInherited) != 0)
					result.Append("AI");
			} else {
				if((sdFlags & ControlFlags.SystemAclProtected) != 0)
					result.Append("P");
				if((sdFlags & ControlFlags.SystemAclAutoInheritRequired) != 0)
					result.Append("AR");
				if((sdFlags & ControlFlags.SystemAclAutoInherited) != 0)
					result.Append("AI");
			}
			
			foreach(var ace in list)
			{
				result.Append(ace.GetSddlForm());
			}
			
			return result.ToString();
		}

		internal static RawAcl ParseSddlForm(string sddlForm,
		                                     bool isDacl,
		                                     ref ControlFlags sdFlags,
		                                     ref int pos)
		{
			ParseFlags(sddlForm, isDacl, ref sdFlags, ref pos);
			
			byte revision = GenericAcl.AclRevision;
			List<GenericAce> aces = new List<GenericAce>();
			while(pos < sddlForm.Length && sddlForm[pos] == '(') {
				GenericAce ace = GenericAce.CreateFromSddlForm(
							sddlForm, ref pos);
				if ((ace as ObjectAce) != null)
					revision = GenericAcl.AclRevisionDS;
				aces.Add(ace);
			}
			
			
			return new RawAcl(revision, aces);
		}
		
		private static void ParseFlags(string sddlForm,
		                               bool isDacl,
		                               ref ControlFlags sdFlags,
		                               ref int pos)
		{
			char ch = Char.ToUpperInvariant(sddlForm[pos]);
			while(ch == 'P' || ch == 'A') {
				if(ch == 'P') {
					if (isDacl)
						sdFlags |= ControlFlags.DiscretionaryAclProtected;
					else
						sdFlags |= ControlFlags.SystemAclProtected;
					pos++;
				} else if(sddlForm.Length > pos + 1) {
					ch = Char.ToUpperInvariant(sddlForm[pos + 1]);
					if(ch == 'R') {
						if (isDacl)
							sdFlags |= ControlFlags.DiscretionaryAclAutoInheritRequired;
						else
							sdFlags |= ControlFlags.SystemAclAutoInheritRequired;
						pos += 2;
					} else if (ch == 'I') {
						if (isDacl)
							sdFlags |= ControlFlags.DiscretionaryAclAutoInherited;
						else
							sdFlags |= ControlFlags.SystemAclAutoInherited;
						pos += 2;
					} else {
						throw new ArgumentException("Invalid SDDL string.", "sddlForm");
					}
				} else {
					throw new ArgumentException("Invalid SDDL string.", "sddlForm");
				}
			}
			
		}
		
		private void WriteUShort (ushort val, byte[] buffer, int offset)
		{
			buffer[offset] = (byte)val;
			buffer[offset + 1] = (byte)(val >> 8);
		}
		
		private ushort ReadUShort (byte[] buffer, int offset)
		{
			return (ushort)((((int)buffer[offset + 0]) << 0)
			                | (((int)buffer[offset + 1]) << 8));
		}
	}
}


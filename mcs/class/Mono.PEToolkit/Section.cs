
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
/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace Mono.PEToolkit {

	public class Section {

		// IMAGE_SECTION_HEADER
		protected class Header {
			internal uint  phAddr_virtSize;
			internal RVA   virtAddr;
			internal uint  rawSize;
			internal RVA   rawDataPtr;
			internal RVA   relocPtr;
			internal RVA   lineNumPtr;
			internal short relocNum;
			internal short linenumNum;
			internal SectionCharacteristics flags;
			
			public Header (BinaryReader reader)
			{
				Read (reader);
			}

			public void Read (BinaryReader reader)
			{
				phAddr_virtSize = reader.ReadUInt32 ();
				virtAddr = new RVA (reader.ReadUInt32 ());
				rawSize = reader.ReadUInt32 ();
				rawDataPtr = new RVA (reader.ReadUInt32 ());
				relocPtr = new RVA (reader.ReadUInt32 ());
				lineNumPtr = new RVA (reader.ReadUInt32 ());
				relocNum = reader.ReadInt16 ();
				linenumNum = reader.ReadInt16 ();
				flags = (SectionCharacteristics) reader.ReadUInt32 ();
			}
			
			public void Write (BinaryWriter writer)
			{
				writer.Write (phAddr_virtSize);
				virtAddr.Write (writer);
				writer.Write (rawSize);
				rawDataPtr.Write (writer);
				relocPtr.Write (writer);
				lineNumPtr.Write (writer);
				writer.Write (relocNum);
				writer.Write (linenumNum);
				writer.Write ((uint) flags);
			}
		}

		private string name;
		private Header hdr;

		public readonly static Section Invalid;

		static Section()
		{
			Invalid = new Section();
		}

		public Section()
		{
		}


		public string Name {
			get {
				return name;
			}
			set {
				name = value;
			}
		}


		public uint PhysicalAddress {
			get {
				return hdr.phAddr_virtSize;
			}
			set {
				hdr.phAddr_virtSize = value;
			}
		}

		public uint VirtualSize {
			get {
				return hdr.phAddr_virtSize;
			}
			set {
				hdr.phAddr_virtSize = value;
			}
		}

		public RVA VirtualAddress {
			get {
				return hdr.virtAddr;
			}
			set {
				hdr.virtAddr = value;
			}
		}

		public uint SizeOfRawData {
			get {
				return hdr.rawSize;
			}
			set {
				hdr.rawSize = value;
			}
		}

		public RVA PointerToRawData {
			get {
				return hdr.rawDataPtr;
			}
			set {
				hdr.rawDataPtr = value;
			}
		}
		
		public RVA PointerToRelocations {
			get {
				return hdr.relocPtr;
			}
			set {
				hdr.relocPtr = value;
			}
		}
		
		public RVA PointerToLinenumbers {
			get {
				return hdr.lineNumPtr;
			}
			set {
				hdr.lineNumPtr = value;
			}
		}
		
		public short NumberOfRelocations {
			get {
				return hdr.relocNum;
			}
			set {
				hdr.relocNum = value;
			}
		}
		
		public short NumberOfLinenumbers {
			get {
				return hdr.linenumNum;
			}
			set {
				hdr.linenumNum = value;
			}
		}
		
		public SectionCharacteristics Characteristics {
			get {
				return hdr.flags;
			}
			set {
				hdr.flags = value;
			}
		}

		/// <summary>
		/// </summary>
		public void Read(BinaryReader reader)
		{
			char[] pName = new char[8];
			int len = 0;

			for (len = 0; len<8; len++) {
				sbyte c = reader.ReadSByte();
				if (c == 0) 
					break;
				pName[len] = (char) c;
			}

			if (len == 0)
				name = String.Empty;
			else
				name = new String (pName);			

			reader.BaseStream.Position += 8 - len - 1;

			hdr = new Header (reader);
		}

		public void Write (BinaryWriter writer)
		{
			sbyte[] name_bytes =  new sbyte[8];
	
			for (int i=0; i<name.Length; i++)
				writer.Write ((sbyte) name[i]);
			
			for (int i=name.Length; i<8; i++)
				writer.Write ((sbyte) 0);

			hdr.Write (writer);
		}

	}

}


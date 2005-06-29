
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
using System.Runtime.InteropServices;

namespace Mono.PEToolkit {

	/// <summary>
	/// IMAGE_DATA_DIRECTORY.
	/// </summary>
	public class DataDir {

		public static readonly DataDir Null;

		public RVA virtAddr;
		public uint size;

		static DataDir ()
		{
			Null = new DataDir ();
			Null.virtAddr = 0;
			Null.size = 0;
		}

		public DataDir () {

		}

		public DataDir (BinaryReader reader)
		{
			Read (reader);
		}

		public void Read (BinaryReader reader)
		{
			virtAddr = new RVA (reader.ReadUInt32 ());
			size = reader.ReadUInt32 ();
		}

		public void Write (BinaryWriter writer)
		{
			virtAddr.Write (writer);
			writer.Write (size);
		}

		public RVA VirtualAddress {
			get {
				return virtAddr;
			}
			set {
				virtAddr = value;
			}
		}

		public uint Size {
			get {
				return size;
			}
			set {
				size = value;
			}
		}

		public bool IsNull {
			get {
				return (this == Null);
			}
		}

		public override int GetHashCode()
		{
			return (virtAddr.GetHashCode() ^ (int)(size << 1));
		}

		public override bool Equals(object obj)
		{
			bool res = (obj is DataDir);
			if (res) {
				DataDir that = (DataDir) obj;
				res = (this.virtAddr == that.virtAddr) &&
				      (this.size == that.size);
			}
			return res;
		}

		public static bool operator == (DataDir d1, DataDir d2)
		{
			return d1.Equals(d2);
		}

		public static bool operator != (DataDir d1, DataDir d2)
		{
			return !d1.Equals(d2);
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			if (this.IsNull) return "NULL";
			return String.Format("RVA = {0}, size = 0x{1}", virtAddr, size.ToString("X"));
		}

	}

}


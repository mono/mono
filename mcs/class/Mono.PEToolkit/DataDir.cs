/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

using System;
using System.Runtime.InteropServices;

namespace Mono.PEToolkit {

	/// <summary>
	/// IMAGE_DATA_DIRECTORY.
	/// </summary>
	[StructLayoutAttribute(LayoutKind.Sequential)]
	public struct DataDir {

		public static readonly DataDir Null;

		public RVA virtAddr;
		public uint size;

		static DataDir ()
		{
			Null.virtAddr = 0;
			Null.size = 0;
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


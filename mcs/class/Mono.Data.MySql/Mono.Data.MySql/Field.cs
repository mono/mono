// 
// Field.cs
//
// Provide definition for the Field class
// Part of the C# bindings to MySQL library libMySQL.dll
//
// Author:
//    Brad Merrill <zbrad@cybercom.net>
//    Daniel Morgan <danmorg@sc.rr.com>
//
// (C)Copyright 2002 Brad Merril
// (C)Copyright 2002 Daniel Morgan
//
// http://www.cybercom.net/~zbrad/DotNet/MySql/
//
// Mono has gotten permission from Brad Merrill to include in 
// the Mono Class Library
// his C# bindings to MySQL under the X11 License
//
// Mono can be found at http://www.go-mono.com/
// The X11/MIT License can be found 
// at http://www.opensource.org/licenses/mit-license.html
//


namespace Mono.Data.MySql {
	using System;
	using System.Runtime.InteropServices;

	[Flags]
	internal enum MySqlFieldFlags : uint {
		NOT_NULL_FLAG = 1,
		PRI_KEY_FLAG = 2,
		UNIQUE_KEY_FLAG = 4,
		MULTIPLE_KEY_FLAG = 8,
		BLOB_FLAG = 16,
		UNSIGNED_FLAG = 32,
		ZEROFILL_FLAG = 64,
		BINARY_FLAG = 128,
		ENUM_FLAG = 256,
		AUTO_INCREMENT_FLAG = 512,
		TIMESTAMP_FLAG = 1024,
		SET_FLAG = 2048,
		NUM_FLAG = 32768,
		PART_KEY_FLAG = 16384,
		GROUP_FLAG = 32768,
		UNIQUE_FLAG = 65536
	}

	///<remarks>
	///<para>
	/// MySql P/Invoke implementation test program
	/// Brad Merrill
	/// 3-Mar-2002
	///</para>
	///<para>
	/// This structure contains information about a field, such as the
	/// field's name, type, and size. Its members are described in more
	/// detail below.  You may obtain the <see cref="Field"/> structures for
	/// each field by calling
	/// <see cref="MySql.FetchField"/>
	/// repeatedly.
	/// Field values are not part of this structure;
	/// they are contained in a Row structure.
	///</para>
	///</remarks>
	[StructLayout(LayoutKind.Sequential)]
	public class MySqlMarshalledField {

		///<value>name of column</value>
		[MarshalAs(UnmanagedType.LPStr)]
		public string Name;
		///<value>table of column</value>
		[MarshalAs(UnmanagedType.LPStr)]
		public string Table;
		///<value>default value</value>
		[MarshalAs(UnmanagedType.LPStr)]
		public string Def;
		///<value>type of field</value>
		public int FieldType;
		///<value>width of column</value>
		public uint Length;
		///<value>max width of selected set</value>
		public uint MaxLength;
		///<value>div flags</value>
		public uint Flags;
		///<value>number of decimals in field</value>
		public uint Decimals;	
	}

	internal sealed class MySqlFieldHelper {	
		
		public static bool IsPrimaryKey(uint fieldFlags) {
			//  if ((SomeEnum)U & SomeEnum.EnumFlagValue) != 0) {...}
			if(! (((MySqlFieldFlags) fieldFlags) & MySqlFieldFlags.PRI_KEY_FLAG).Equals(0))	
				return true;
			else
				return false;
		}

		public static bool IsNotNull(uint fieldFlags) {
			
			if(! (((MySqlFieldFlags) fieldFlags) & MySqlFieldFlags.NOT_NULL_FLAG).Equals(0))	
				return true;
			else
				return false;
		}

		public static bool IsBlob(uint fieldFlags) {
			
			if(! (((MySqlFieldFlags) fieldFlags) & MySqlFieldFlags.BLOB_FLAG).Equals(0))	
				return true;
			else
				return false;
		}
	}
}

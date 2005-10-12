/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2002, 2005 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

using System;
using System.Runtime.InteropServices;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Embedded
{
	#region Structures

	[StructLayout(LayoutKind.Sequential)]
	internal struct ArrayDescMarshal
	{
		public byte DataType;
		public byte Scale;
		public short Length;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string FieldName;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string RelationName;
		public short Dimensions;
		public short Flags;

		#region Static Methods

		public static int ComputeLength(int n)
		{
			return (Marshal.SizeOf(typeof(ArrayDescMarshal)) + n * Marshal.SizeOf(typeof(ArrayBoundMarshal)));
		}

		#endregion
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct ArrayBoundMarshal
	{
		public short LowerBound;
		public short UpperBound;
	}

	#endregion
}

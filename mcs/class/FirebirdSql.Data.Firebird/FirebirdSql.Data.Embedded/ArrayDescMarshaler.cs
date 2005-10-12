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
	internal class ArrayDescMarshaler
	{
		#region Static Fields

		private static ArrayDescMarshaler instance;

		#endregion

		#region Constructors

		private ArrayDescMarshaler()
		{
		}

		#endregion

		#region Methods

		public static ArrayDescMarshaler GetInstance()
		{
			if (ArrayDescMarshaler.instance == null)
			{
				ArrayDescMarshaler.instance = new ArrayDescMarshaler();
			}

			return ArrayDescMarshaler.instance;
		}

		public void CleanUpNativeData(ref IntPtr pNativeData)
		{
			if (pNativeData != IntPtr.Zero)
			{
				// Destroy ArrayDescMarshal	structure
				Marshal.DestroyStructure(pNativeData, typeof(ArrayDescMarshal));

				// Destroy ArrayBound structures
				for (int i = 0; i < 16; i++)
				{
					Marshal.DestroyStructure(
						(IntPtr)(pNativeData.ToInt32() + ArrayDescMarshal.ComputeLength(i)),
						typeof(ArrayBoundMarshal));
				}

				// Free	pointer	memory
				Marshal.FreeHGlobal(pNativeData);

				pNativeData = IntPtr.Zero;
			}
		}

		public IntPtr MarshalManagedToNative(ArrayDesc descriptor)
		{
			ArrayDescMarshal arrayDesc = new ArrayDescMarshal();

			arrayDesc.DataType		= descriptor.DataType;
			arrayDesc.Scale			= (byte)descriptor.Scale;
			arrayDesc.Length		= descriptor.Length;
			arrayDesc.FieldName		= descriptor.FieldName;
			arrayDesc.RelationName	= descriptor.RelationName;
			arrayDesc.Dimensions	= descriptor.Dimensions;
			arrayDesc.Flags			= descriptor.Flags;

			ArrayBoundMarshal[] arrayBounds = new ArrayBoundMarshal[descriptor.Bounds.Length];

			for (int i = 0; i < descriptor.Dimensions; i++)
			{
				arrayBounds[i].LowerBound = (short)descriptor.Bounds[i].LowerBound;
				arrayBounds[i].UpperBound = (short)descriptor.Bounds[i].UpperBound;
			}

			return this.MarshalManagedToNative(arrayDesc, arrayBounds);
		}

		public IntPtr MarshalManagedToNative(
			ArrayDescMarshal arrayDesc,
			ArrayBoundMarshal[] arrayBounds)
		{
			int		size = ArrayDescMarshal.ComputeLength(arrayBounds.Length);
			IntPtr	ptr	 = Marshal.AllocHGlobal(size);

			Marshal.StructureToPtr(arrayDesc, ptr, true);
			for (int i = 0; i < arrayBounds.Length; i++)
			{
				Marshal.StructureToPtr(
					arrayBounds[i],
					(IntPtr)(ptr.ToInt32() + ArrayDescMarshal.ComputeLength(i)),
					true);
			}

			return ptr;
		}

		#endregion
	}
}
// 
// OciLobLocator.cs 
//  
// Part of managed C#/.NET library System.Data.OracleClient.dll
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient.Oci
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient.Oci
// 
// Author: 
//     Tim Coleman <tim@timcoleman.com>
//         
// Copyright (C) Tim Coleman, 2003
// 

using System;
using System.Data.OracleClient;
using System.Runtime.InteropServices;

namespace System.Data.OracleClient.Oci {
	internal sealed class OciParameterDescriptor : OciDescriptorHandle
	{
		#region Fields

		OciErrorHandle errorHandle;
		OciServiceHandle service;
		OciDataType type;

		#endregion // Fields

		#region Constructors

		public OciParameterDescriptor (OciHandle parent, IntPtr handle)
			: base (OciHandleType.Parameter, parent, handle)
		{
		}

		#endregion // Constructors

		#region Properties 

		public OciErrorHandle ErrorHandle {
			get { return errorHandle; }
			set { errorHandle = value; }
		}

		#endregion // Properties

		#region Methods

		public string GetName ()
		{
			return GetAttributeString (OciAttributeType.Name, ErrorHandle);
		}

		public int GetDataSize ()
		{
			return (int) GetAttributeUInt16 (OciAttributeType.DataSize, ErrorHandle);
		}

		public OciDataType GetDataType ()
		{
			return (OciDataType) GetAttributeInt32 (OciAttributeType.DataType, ErrorHandle);
		}

		public short GetPrecision ()
		{
			return (short) GetAttributeByte (OciAttributeType.Precision, ErrorHandle);
		}

		public short GetScale ()
		{
			return (short) GetAttributeSByte (OciAttributeType.Scale, ErrorHandle);
		}

		public bool GetIsNull ()
		{
			return GetAttributeBool (OciAttributeType.IsNull, ErrorHandle);
		}

		#endregion // Methods
	}
}

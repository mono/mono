// 
// OciHandle.cs 
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
using System.Runtime.InteropServices;

namespace System.Data.OracleClient.Oci {
	internal abstract class OciHandle : IDisposable
	{
		#region Fields

		bool disposed = false;
		IntPtr handle;
		OciHandle parent;
		OciHandleType type;

		#endregion // Fields

		#region Constructors

		public OciHandle (OciHandleType type, OciHandle parent, IntPtr newHandle)
		{
			this.type = type;
			this.parent = parent;
			this.handle = newHandle;
		}

		~OciHandle ()
		{
			Dispose (false);
		}

		#endregion // Constructors

		#region Properties

		public OciHandle Parent {
			get { return parent; }
		}

		public IntPtr Handle { 
			get { return handle; }
		}

		public OciHandleType HandleType { 
			get { return type; }
		}

		#endregion // Properties

		#region Methods

		public OciHandle Allocate (OciHandleType type)
		{
			int status = 0;
			IntPtr newHandle = IntPtr.Zero;

			if (type < OciHandleType.LobLocator)
				status = OciCalls.OCIHandleAlloc (this,
					out newHandle,
					type,
					0,
					IntPtr.Zero);
			else
				status = OciCalls.OCIDescriptorAlloc (this,
					out newHandle,
					type,
					0,
					IntPtr.Zero);
		
			if (status != 0 && status != 1)
				throw new Exception (String.Format ("Could not allocate new OCI Handle of type {0}", type));

			switch (type) {
			case OciHandleType.Service:
				return new OciServiceHandle (this, newHandle);
			case OciHandleType.Error:
				return new OciErrorHandle (this, newHandle);
			case OciHandleType.Server:
				return new OciServerHandle (this, newHandle);
			case OciHandleType.Session:
				return new OciSessionHandle (this, newHandle);
			case OciHandleType.Statement:
				return new OciStatementHandle (this, newHandle);
			case OciHandleType.Transaction:
				return new OciTransactionHandle (this, newHandle);
			case OciHandleType.LobLocator:
				return new OciLobLocator (this, newHandle);
			case OciHandleType.RowId:
				return new OciRowIdDescriptor (this, newHandle);
			}
			return null;
		}

		protected virtual void Dispose (bool disposing)
		{
			if (!disposed) {
				FreeHandle ();
				if (disposing) {
					parent = null;
				}
				disposed = true;
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void FreeHandle ()
		{
			switch (type) {
				case OciHandleType.Bind:
				case OciHandleType.Define:
					// Bind and Define handles are freed when Statement handle is disposed
					break;
				case OciHandleType.Environment:
					if (handle != IntPtr.Zero) {
						OciCalls.OCIHandleFree (handle, type);
					}
					break;
				default:
					if ( handle != IntPtr.Zero &&
						 parent != null && 
						 parent.Handle != IntPtr.Zero )	{

							OciCalls.OCIHandleFree (handle, type);
					}
					break;
			}
			handle = IntPtr.Zero;
		}

		public bool GetAttributeBool (OciAttributeType attrType, OciErrorHandle errorHandle)
		{
			return (GetAttributeInt32 (attrType, errorHandle) != 0);
		}

		public sbyte GetAttributeSByte (OciAttributeType attrType, OciErrorHandle errorHandle)
		{
			int status = 0;
			sbyte output;

			status = OciCalls.OCIAttrGetSByte (Handle,
						HandleType,
						out output,
						IntPtr.Zero,
						attrType,
						errorHandle);

			if (status != 0) {
				OciErrorInfo info = errorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			return output;
		}

		public byte GetAttributeByte (OciAttributeType attrType, OciErrorHandle errorHandle)
		{
			int status = 0;
			byte output;

			status = OciCalls.OCIAttrGetByte (Handle,
						HandleType,
						out output,
						IntPtr.Zero,
						attrType,
						errorHandle);

			if (status != 0) {
				OciErrorInfo info = errorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			return output;
		}

		public ushort GetAttributeUInt16 (OciAttributeType attrType, OciErrorHandle errorHandle)
		{
			int status = 0;
			ushort output;

			status = OciCalls.OCIAttrGetUInt16 (Handle,
						HandleType,
						out output,
						IntPtr.Zero,
						attrType,
						errorHandle);

			if (status != 0) {
				OciErrorInfo info = errorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			return output;
		}

		public int GetAttributeInt32 (OciAttributeType attrType, OciErrorHandle errorHandle)
		{
			int status = 0;
			int output;

			status = OciCalls.OCIAttrGetInt32 (Handle,
						HandleType,
						out output,
						IntPtr.Zero,
						attrType,
						errorHandle);

			if (status != 0) {
				OciErrorInfo info = errorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			return output;
		}

		public IntPtr GetAttributeIntPtr (OciAttributeType attrType, OciErrorHandle errorHandle)
		{
			int status = 0;
			IntPtr output = IntPtr.Zero;
			status = OciCalls.OCIAttrGetIntPtr (Handle,
						HandleType,
						out output,
						IntPtr.Zero,
						attrType,
						errorHandle);

			if (status != 0) {
				OciErrorInfo info = errorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			return output;
		}

		public string GetAttributeString (OciAttributeType attrType, OciErrorHandle errorHandle)
		{
			string output = String.Empty;
			IntPtr outputPtr = IntPtr.Zero;
			int outSize;
			int status = 0;

			status = OciCalls.OCIAttrGet (Handle,
					HandleType,
					out outputPtr,
					out outSize,
					attrType,
					errorHandle);

			if (status != 0) {
				OciErrorInfo info = errorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			if (outputPtr != IntPtr.Zero && outSize > 0) {
				object str = Marshal.PtrToStringAnsi (outputPtr, outSize);
				if (str != null) 
					output = String.Copy ((string) str);
			}

			return output;
		}

		public void SetHandle (IntPtr h)
		{
			handle = h;
		}

		#endregion // Methods

		#region Operators and Type Conversions

		public static implicit operator IntPtr (OciHandle h)
		{
			return h.Handle;
		}

		#endregion // Operators and Type Conversions
	}
}

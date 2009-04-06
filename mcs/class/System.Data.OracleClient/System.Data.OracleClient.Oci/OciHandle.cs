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
//     Daniel Morgan <danielmorgan@verizon.net>
//         
// Copyright (C) Tim Coleman, 2003
// Copyright (C) Daniel Morgan, 2005
// 

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Security.Cryptography;
using System.Globalization;

namespace System.Data.OracleClient.Oci {
	internal abstract class OciHandle : IDisposable {
		#region Fields

		bool disposed = false;

		protected internal IntPtr handle = IntPtr.Zero;
		OciHandle parent = null;
		OciHandleType type;

		#endregion // Fields

		#region Constructors

		internal OciHandle (OciHandleType type, OciHandle parent, IntPtr newHandle) {
			this.type = type;
			this.parent = parent;
			this.handle = newHandle;
		}

		~OciHandle () {
			Dispose (false);
		}

		#endregion // Constructors

		#region Properties

		internal OciHandle Parent {
			get { return parent; }
		}

		internal IntPtr Handle { 
			get { return handle; }
		}

		internal OciHandleType HandleType { 
			get { return type; }
		}

		#endregion // Properties

		#region Methods

		internal OciHandle Allocate (OciHandleType type) {
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
			case OciHandleType.TimeStamp:
				return new OciDateTimeDescriptor (this, newHandle);
			case OciHandleType.IntervalDayToSecond:
			case OciHandleType.IntervalYearToMonth:
				return new OciIntervalDescriptor (this, type, newHandle);
			}
			return null;
		}

		protected virtual void Dispose (bool disposing) {
			if (!disposed) {
				FreeHandle ();
				if (disposing) {
					parent = null;
				}
				disposed = true;
			}
		}

		public void Dispose () {
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void FreeHandle () 
		{
			if (type < OciHandleType.LobLocator) {
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
		}

		internal bool GetAttributeBool (OciAttributeType attrType, OciErrorHandle errorHandle) {
			return (GetAttributeInt32 (attrType, errorHandle) != 0);
		}

		internal sbyte GetAttributeSByte (OciAttributeType attrType, OciErrorHandle errorHandle) {
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

		internal byte GetAttributeByte (OciAttributeType attrType, OciErrorHandle errorHandle) {
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

		internal ushort GetAttributeUInt16 (OciAttributeType attrType, OciErrorHandle errorHandle) {
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

		internal int GetAttributeInt32 (OciAttributeType attrType, OciErrorHandle errorHandle) {
			int status = 0;
			int output;

			status = OciCalls.OCIAttrGetInt32 (Handle,
				HandleType,
				out output,
				IntPtr.Zero,
				attrType,
				errorHandle);

			if (status != 0) {
				OciErrorInfo info = OciErrorHandle.HandleError (errorHandle, status);
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			return output;
		}

		[DllImport ("oci", EntryPoint = "OCIAttrGet")]
		internal static extern int OCIAttrGetRowIdDesc (IntPtr trgthndlp,
			[MarshalAs (UnmanagedType.U4)] OciHandleType trghndltyp,
			IntPtr attributep,
			ref uint sizep,
			[MarshalAs (UnmanagedType.U4)] OciAttributeType attrtype,
			IntPtr errhp);
		internal OciRowIdDescriptor GetAttributeRowIdDescriptor (OciErrorHandle errorHandle, OciHandle env)
		{
			OciRowIdDescriptor descriptor = null;				
			IntPtr outputPtr = IntPtr.Zero;
			int outSize = 16;
			int status = 0;
			OciAttributeType attrType = OciAttributeType.RowId; 

			outputPtr = OciCalls.AllocateClear (outSize);

			uint siz = (uint) outSize;
			status = OCIAttrGetRowIdDesc (Handle,
				HandleType,
				outputPtr,
				ref siz,
				attrType,
				errorHandle);

			if (status != 0) {
				OciErrorInfo info = errorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			if (outputPtr != IntPtr.Zero && siz > 0) {
				descriptor = (OciRowIdDescriptor) env.Allocate(OciHandleType.RowId);
				descriptor.SetHandle (outputPtr);
			}

			return descriptor;
		}

		internal IntPtr GetAttributeIntPtr (OciAttributeType attrType, OciErrorHandle errorHandle) {
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

		internal string GetAttributeString (OciAttributeType attrType, OciErrorHandle errorHandle) {
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

		internal void SetAttributeString (string attribute, OciAttributeType attrType, OciErrorHandle errorHandle) 
		{
			int status = 0;
			
			status = OciCalls.OCIAttrSetString (Handle,
				HandleType,
				attribute,
				(uint) attribute.Length,
				attrType,
				errorHandle);

			if (status != 0) {
				OciErrorInfo info = errorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
		}

		internal void SetHandle (IntPtr h)
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

// 
// OciBindHandle.cs 
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
using System.Text;

namespace System.Data.OracleClient.Oci {
	internal sealed class OciBindHandle : IOciHandle, IDisposable
	{
		#region Fields

		IntPtr handle;
		string name;
		IntPtr value;
		OciStatementHandle statement;
		OciDataType type;
		int size;
		int indicator;
	
		#endregion // Fields

		#region Constructors

		public OciBindHandle (string name)
		{
			this.name = name;
			this.value = IntPtr.Zero;
			this.indicator = 0;
		}

		#endregion // Constructors

		#region Properties

		public IntPtr Handle {
			get { return handle; }
			set { handle = value; }
		}

		public OciHandleType HandleType {
			get { return OciHandleType.Bind; }
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public int Size {
			get { return size; }
			set { size = value; }
		}

		public OciDataType Type {
			get { return type; }
			set { type = value; }
		}

		public IntPtr Value {
			get { return value; }
		}

		#endregion

		#region Methods

		[DllImport ("oci", EntryPoint = "OCIBindByName")]
		public static extern int OCIBindByName (IntPtr stmtp,
							out IntPtr bindpp,
							IntPtr errhp,
							string placeholder,
							int placeh_len,
							IntPtr valuep,
							int value_sz,
							[MarshalAs (UnmanagedType.U2)] OciDataType dty,
							ref int indp,
							IntPtr alenp,
							ushort rcodep,
							uint maxarr_len,
							IntPtr curelp,
							uint mode);

		public void Bind (OciStatementHandle statement, object val)
		{
			Console.WriteLine ("IN BIND");
			handle = IntPtr.Zero;

			this.statement = statement;

			int indp = 0;
			ushort alenp = 0;
			IntPtr rcodep = IntPtr.Zero;
			int status = 0;
			OciDataType bindType = Type;
			int definedSize = 0;

			string stringValue = val.ToString ();
			if (val == DBNull.Value) 
				indicator = -1;
			else {
				switch (Type) {
				case OciDataType.Number:
				case OciDataType.Integer:
				case OciDataType.Float:
				case OciDataType.VarNum:
					bindType = OciDataType.Char;
					definedSize = stringValue.Length;
					value = Marshal.StringToHGlobalAnsi (stringValue);
					break;
				case OciDataType.Date:
					break;
				default:
					bindType = OciDataType.Char;
					definedSize = stringValue.Length;
					value = Marshal.StringToHGlobalAnsi (stringValue);
					break;
				}
			}

				
			status = OCIBindByName (statement.Handle,
						out handle,
						statement.ErrorHandle.Handle,
						name,
						name.Length,
						value,
						definedSize,
						bindType,
						ref indicator,
						IntPtr.Zero,
						0,
						0,
						IntPtr.Zero, 
						0);

			if (status != 0) {
				OciErrorInfo info = statement.ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
		}

		public void Dispose ()
		{
			Marshal.FreeHGlobal (value);
		}

		#endregion // Methods
	}
}

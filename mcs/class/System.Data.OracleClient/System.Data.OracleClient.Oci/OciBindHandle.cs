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
	internal sealed class OciBindHandle : OciHandle, IDisposable
	{
		#region Fields

		bool disposed = false;
		IntPtr value;
	
		#endregion // Fields

		#region Constructors

		public OciBindHandle (OciHandle parent)
			: base (OciHandleType.Bind, parent, IntPtr.Zero)
		{
		}

		#endregion // Constructors

		#region Properties

		public IntPtr Value {
			get { return value; }
			set { this.value = value; }
		}

		#endregion // Properties

		#region Methods

		protected override void Dispose (bool disposing)
		{
			if (!disposed) {
				try {
					Marshal.FreeHGlobal (value);
					disposed = true;
				} finally {
					base.Dispose (disposing);
				}
			}
		}

		#endregion // Methods
	}
}

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

namespace System.Data.OracleClient.Oci {
	internal abstract class OciHandle : IOciHandle
	{
		#region Fields

		IntPtr handle;
		OciEnvironmentHandle environment;
		OciHandleType type;

		#endregion // Fields

		#region Constructors

		public OciHandle (OciHandleType type, OciEnvironmentHandle environment, IntPtr newHandle)
		{
			this.type = type;
			this.environment = environment;
			this.handle = newHandle;
		}

		#endregion // Constructors

		#region Properties

		public OciEnvironmentHandle Environment {
			get { return environment; }
		}

		public IntPtr Handle { 
			get { return handle; }
			set { handle = value; }
		}

		public OciHandleType HandleType { 
			get { return type; }
		}

		#endregion // Properties
	}
}

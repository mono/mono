//
// System.Data.SqlClient.SqlParameter.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc. 2002
//
using System;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;

namespace System.Data.SqlClient
{
	/// <summary>
	/// Represents a parameter to a Command object, and optionally, 
	/// its mapping to DataSet columns; and is implemented by .NET 
	/// data providers that access data sources.
	/// </summary>
	//public sealed class SqlParameter : MarshalByRefObject,
	//	IDbDataParameter, IDataParameter, ICloneable
	public sealed class SqlParameter : IDbDataParameter, IDataParameter
	{
		[MonoTODO]
		public DbType DbType {
			get { 
				throw new NotImplementedException (); 
			}
			set { 
				throw new NotImplementedException (); 
			}
		}

		[MonoTODO]
		public ParameterDirection Direction {
			get { 
				throw new NotImplementedException (); 
			}
			set { 
				throw new NotImplementedException (); 
			}
		}

		[MonoTODO]
		public bool IsNullable	{
			get { 
				throw new NotImplementedException (); 
			}
		}

		[MonoTODO]
		public string ParameterName {
			get { 
				throw new NotImplementedException (); 
			}

			set { 
				throw new NotImplementedException (); 
			}
		}

		[MonoTODO]
		public string SourceColumn {
			get { 
				throw new NotImplementedException (); 
			}

			set { 
				throw new NotImplementedException (); 
			}
		}

		[MonoTODO]
		public DataRowVersion SourceVersion {
			get { 
				throw new NotImplementedException (); 
			}

			set { 
				throw new NotImplementedException (); 
			}
		}

		[MonoTODO]
		public object Value {
			get { 
				throw new NotImplementedException (); 
			}

			set { 
				throw new NotImplementedException (); 
			}
		}

		[MonoTODO]
		public byte Precision {
			get { 
				throw new NotImplementedException (); 
			}

			set { 
				throw new NotImplementedException (); 
			}
		}

		[MonoTODO]
                public byte Scale {
			get { 
				throw new NotImplementedException (); 
			}

			set { 
				throw new NotImplementedException (); 
			}
		}

		[MonoTODO]
                public int Size
		{
			get { 
				throw new NotImplementedException (); 
			}

			set { 
				throw new NotImplementedException (); 
			}
		}

	}
}

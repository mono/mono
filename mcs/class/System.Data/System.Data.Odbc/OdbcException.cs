//
// System.Data.Odbc.OdbcDataReader
//
// Author:
//   Brian Ritchie (brianlritchie@hotmail.com) 
//
// Copyright (C) Brian Ritchie, 2002
//

using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Data.Odbc
{
	public sealed class OdbcException : SystemException 
	{
		OdbcErrorCollection col;

		internal OdbcException(OdbcError Error) : base (Error.Message)
		{
			col=new OdbcErrorCollection();
			col.Add(Error);
		}


		public int ErrorCode 
		{	
			get 
			{
				return col[0].NativeError;
			}
		}

		public OdbcErrorCollection Errors 
		{
			get 
			{
				return col;
			}
		}

		public override string Source 
		{	
			get
			{
				return col[0].Source;
			}
		}

		public override string Message 	{
			get { 
				
				return  col[0].Message;
			     }
		}	
		
		#region Methods

		public override void GetObjectData (SerializationInfo si, StreamingContext context)
		{
			if (si == null)
				throw new ArgumentNullException ("si");

			si.AddValue ("col", col);
			base.GetObjectData (si, context);
		}

		#endregion // Methods
	}
}

using System;
using System.Data;

namespace System.Data.SqlClient
{
	/// <summary>
	/// Summary description for SqlInfoMessageEventArgs.
	/// </summary>
	public sealed class SqlInfoMessageEventArgs : EventArgs
	{
		#region Fields

		SqlErrorCollection errors ;

		#endregion // Fields

		#region Constructors
	
		internal SqlInfoMessageEventArgs (SqlErrorCollection errors)
		{
			this.errors = errors;
		}

		#endregion // Constructors

		#region Properties

		public SqlErrorCollection Errors 
		{
			get { return errors; }
		}	

		public string Message 
		{
			get { return errors[0].Message; }
		}	

		public string Source 
		{
			get { return errors[0].Source; }
		}

		#endregion // Properties

		#region Methods

		public override string ToString() 
		{
			return Message;
		}

		#endregion // Methods
	}
}

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace IBM.Data.DB2
{
	public sealed class DB2Error
	{
		string message;
		string state;
		int nativeerror;

		internal DB2Error(string message, string state, int nativeerror)
		{
			this.message = message;
			this.state = state;
			this.nativeerror = nativeerror;
		}

		public override string ToString()
		{
			return Message;
		}
		public string Message 
		{
			get { return message; }
		}
		public int NativeError  
		{
			get { return nativeerror; }
		}
		public string Source
		{
			get { return "IBM.Data.DB2"; }
		}
		public string SQLState 
		{
			get { return state; }
		}					 
	}
}

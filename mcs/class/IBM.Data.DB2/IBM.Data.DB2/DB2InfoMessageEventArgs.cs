using System;
using System.Data;
using System.Runtime.InteropServices;

namespace IBM.Data.DB2
{
	public delegate void DB2InfoMessageEventHandler(object sender, DB2InfoMessageEventArgs e);

	public sealed class DB2InfoMessageEventArgs : EventArgs
	{
		private DB2ErrorCollection errors;

		public DB2InfoMessageEventArgs(DB2ErrorCollection errors)
		{
			this.errors = errors;
		}

		public DB2ErrorCollection Errors
		{
			get
			{
				return errors;
			}
		}
		public string Message
		{
			get
			{
				if(errors.Count > 0)
				{
					string result = "";
					for(int i = 0; i < errors.Count; i++)
					{
						if(i > 0)
						{
							result += " ";
						}
						result += "INFO [" + errors[i].SQLState + "] " + errors[i].Message;
					}
					return result;
				}
				return "No information";
			}
		}

	}
}


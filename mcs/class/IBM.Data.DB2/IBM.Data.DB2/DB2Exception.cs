using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Runtime.Serialization;

namespace IBM.Data.DB2
{


	[Serializable]
	public sealed class DB2Exception : Exception
	{
		private DB2ErrorCollection errors;
		private string message;


		public DB2ErrorCollection Errors
		{
			get
			{
				return errors;
			}
		}
		public override string Message
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
						result += "ERROR [" + errors[i].SQLState + "] " + errors[i].Message;
					}
					if(message != null)
					{
						result += " " + message;
					}
					return result;
				}
				return "No error information";
			}
		}

		private DB2Exception(SerializationInfo si, StreamingContext sc)
		{
			throw new NotImplementedException();
		}
		public override void GetObjectData(SerializationInfo si, StreamingContext context)
		{
			throw new NotImplementedException();
		}

		internal DB2Exception(short sqlHandleType, IntPtr sqlHandle, string message)
		{	
			this.message = message;
			errors = new DB2ErrorCollection(sqlHandleType, sqlHandle);


		}
	}
}

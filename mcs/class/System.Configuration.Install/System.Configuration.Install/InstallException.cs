// System.Configuration.Install.InstallException.cs
//
// Author:
// 	Alejandro Sánchez Acosta  <raciel@es.gnu.org>
//
// (C) Alejandro Sánchez Acosta
//

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Configuration.Install
{
	[Serializable]
	public class InstallException : SystemException
	{
		private Exception innerException;
		
		public InstallException ()
		{
		}

		public InstallException (string message) : base (message) 
		{
		} 

		protected InstallException (SerializationInfo info, StreamingContext context) : base (info, context)
		{
		}

		public InstallException (string message, Exception innerException) : base (message)
		{
			this.innerException = innerException;
		}		
	}
}

// Transport Security Layer (TLS)
// Copyright (c) 2003-2004 Carlos Guzman Alvarez

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Text;
using System.Runtime.Serialization;

namespace Mono.Security.Protocol.Tls
{
	[Serializable]
	internal sealed class TlsException : Exception
	{	
		#region Fields

		private Alert alert;

		#endregion

		#region Properties

		public Alert Alert
		{
			get { return this.alert; }
		}

		#endregion

		#region Constructors
		
		internal TlsException(string message) : base(message)
		{
		}

		internal TlsException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
		
		internal TlsException(string message, Exception ex) : base(message, ex)
		{
		}

		internal TlsException(
			AlertLevel			level,
			AlertDescription	description) 
			: this (level, description, Alert.GetAlertMessage(description))
		{
		}

		internal TlsException(
			AlertLevel			level,
			AlertDescription	description,
			string				message) : base (message)
		{
			this.alert = new Alert(level, description);
		}

		internal TlsException(
			AlertDescription description) 
			: this (description, Alert.GetAlertMessage(description))
		{
		}

		internal TlsException(
			AlertDescription	description,
			string				message) : base (message)
		{
			this.alert = new Alert(description);
		}

		#endregion
	}
}

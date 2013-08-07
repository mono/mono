/******************************************************************************
* The MIT License
*
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to  permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.DirectoryServices
{
	[Serializable]
	public class DirectoryServicesCOMException : COMException, ISerializable
	{
		public int ExtendedError {
			get {
				throw new NotImplementedException ();
			}
		}

		public string ExtendedErrorMessage {
			get {
				throw new NotImplementedException ();
			}
		}

		public DirectoryServicesCOMException ()
		{
		}

		public DirectoryServicesCOMException (string message) : base(message)
		{
		}

		public DirectoryServicesCOMException (string message, Exception inner) : base(message, inner)
		{
		}

		protected DirectoryServicesCOMException (SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			throw new NotImplementedException ();
		}
	}
}

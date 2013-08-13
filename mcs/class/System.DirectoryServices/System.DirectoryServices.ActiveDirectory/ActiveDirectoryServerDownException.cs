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
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.DirectoryServices.ActiveDirectory
{
	[Serializable]
	public class ActiveDirectoryServerDownException : Exception, ISerializable
	{
		public int ErrorCode {
			get {
				throw new NotImplementedException ();
			}
		}

		public string Name {
			get {
				throw new NotImplementedException ();
			}
		}

		public override string Message {
			get {
				throw new NotImplementedException ();
			}
		}

		public ActiveDirectoryServerDownException (string message, Exception inner, int errorCode, string name) : base(message, inner)
		{
		}

		public ActiveDirectoryServerDownException (string message, int errorCode, string name) : base(message)
		{

		}

		public ActiveDirectoryServerDownException (string message, Exception inner) : base(message, inner)
		{
		}

		public ActiveDirectoryServerDownException (string message) : base(message)
		{
		}

		public ActiveDirectoryServerDownException ()
		{
		}

		protected ActiveDirectoryServerDownException (SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			throw new NotImplementedException ();
		}
	}
}

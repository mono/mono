//
// System.Data.SqlTypes.SqlTypeException
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
//	Boris Kirzner <borisk@mainsoft.com>
//	
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
//

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
using System.Runtime.Serialization;

namespace System.Data.SqlTypes
{
	[Serializable]
    public class SqlTypeException : SystemException
    {
		protected string _message;

		public SqlTypeException() : this("System error.")
        {
        }

        public SqlTypeException(String message) : base(message)
        {
			_message = message;
        }

		public SqlTypeException (SerializationInfo info, StreamingContext context) {
			_message = (string) info.GetString ("SqlTypeExceptionMessage");
		}

		public SqlTypeException(String message, Exception innerException) : base(message, innerException)
        {
			_message = message;
        }

		public override string Message {
			get { return _message; }
		}

		public override void GetObjectData (SerializationInfo si, StreamingContext context)
        {                                             
            si.AddValue ("SqlTypeExceptionMessage", Message);                
        }
    }
}
// System.Data.SqlTypes.SqlNullValueException
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
using System.Runtime.Serialization;

namespace System.Data.SqlTypes
{
	[Serializable]
    public class SqlNullValueException : System.Data.SqlTypes.SqlTypeException
    {
        public SqlNullValueException() : base("Data is Null. This method or property cannot be called on Null values.")
        {
        }

        public SqlNullValueException(String message): base(message)
        {
			_message = message;
        }

		public SqlNullValueException (SerializationInfo info, StreamingContext context) 
			: this () {
			_message = (string) info.GetString ("SqlNullValueExceptionMessage");
		}

		public SqlNullValueException(String message, Exception innerException): base(message, innerException)
        {
			_message = message;
        }

		public override void GetObjectData (SerializationInfo si, StreamingContext context)
        {                                             
            si.AddValue ("SqlNullValueExceptionMessage", Message);                
        }
    }
}
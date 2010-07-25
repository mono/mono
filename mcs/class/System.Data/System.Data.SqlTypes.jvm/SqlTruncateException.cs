// System.Data.SqlTypes.SqlTruncateException
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

namespace System.Data.SqlType
{

    /**
     * The exception that is thrown for errors in an arithmetic, casting, or conversion operation
     */

    /*
    * CURRENT LIMITATIONS
    * 1. Constructor(SerializationInfo info, StreamingContext context) is not supported
    * 2. Method "void GetObjectData(...,...)" is not supported from ISerializable
    */

	[Serializable]
    public class SqlTruncateException : System.Data.SqlTypes.SqlTypeException
    {
        /**
         * Initializes a new instance of the <code>SqlTruncateException</code> class.
         */
        public SqlTruncateException() : base("Numeric arithmetic causes truncation.")
        {
        }

        /**
         * Initializes a new instance of the <code>SqlTruncateException</code> class
         * with a specified error message.
         *
         * @param message The message that describes the error.
         */
        public SqlTruncateException(String message) : base(message)
        {
        }
    }

}
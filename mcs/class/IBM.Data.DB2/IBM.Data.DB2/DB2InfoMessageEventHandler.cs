
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


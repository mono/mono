//
// System.Data.OleDb.OleDbInfoMessageEventArgs
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Rodrigo Moya, 2002
// Copyright (C) Tim Coleman, 2002	
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


using System.Data;
using System.Data.Common;

namespace System.Data.OleDb
{
	public sealed class OleDbInfoMessageEventArgs : EventArgs
	{
		private OleDbErrorCollection errors;
		internal OleDbInfoMessageEventArgs(OleDbErrorCollection errors)
		{
			this.errors = errors;
		}
		#region Properties

		public int ErrorCode {
			[MonoTODO]
			get { return errors[0].NativeError; }
		}

		public OleDbErrorCollection Errors {
			[MonoTODO]
			get { return errors; }
		}

		public string Message {
			[MonoTODO]
			get { return errors[0].Message; }
		}

		public string Source {
			[MonoTODO]
			get { return errors[0].Source;}
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override string ToString ()
		{
			return errors[0].Message;
		}

		#endregion // Methods
	}
}

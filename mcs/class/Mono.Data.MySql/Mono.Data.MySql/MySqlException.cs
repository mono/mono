//
// Mono.Data.MySql.Exception
//
// Author:
//   Daniel Morgan <danmorg@sc.rr.com>
//
// (c)Copyright 2002 Daniel Morgan
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
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Mono.Data.MySql {
	public class MySqlException : SystemException {
		MySqlErrorCollection errors;

		internal MySqlException (MySqlError error) : base (error.Message) {
			errors = new MySqlErrorCollection ();
			errors.Add (error);
		}

		internal MySqlException (string message) : base (message) {
			MySqlError error = new MySqlError (message);
			errors = new MySqlErrorCollection ();
			errors.Add (error);
		}

		#region Properties

			public int ErrorCode {	
				get {
					return errors[0].NativeError;
				}
			}

		public MySqlErrorCollection Errors {
			get {
				return errors;
			}
		}

		public override string Source {	
			get {
				return errors[0].Source;
			}
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override void GetObjectData (SerializationInfo si, StreamingContext context) {
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

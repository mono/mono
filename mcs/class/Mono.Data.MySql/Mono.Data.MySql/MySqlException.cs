//
// Mono.Data.MySql.Exception
//
// Author:
//   Daniel Morgan <danmorg@sc.rr.com>
//
// (c)Copyright 2002 Daniel Morgan
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

//
// System.Data.FillErrorEventArgs.cs
//
// Author:
//   Miguel de Icaza <miguel@ximian.com>
//
// (C) Ximian, Inc 2002
//

using System;

namespace System.Data
{
	public class FillErrorEventArgs : EventArgs {
		DataTable data_table;
		object [] values;
		Exception errors;
		bool f_continue;

		public FillErrorEventArgs (DataTable dataTable, object [] values)
		{
			this.data_table = dataTable;
			this.values = values;
		}

		public bool Continue {
			get {
				return f_continue;
			}

			set {
				f_continue = value;
			}
		}

		public DataTable DataTable {
			get {
				return data_table;
			}
		}

		public Exception Errors {
			get {
				return errors;
			}

			set {
				errors = value;
			}
		}

		public object [] Values {
			get {
				return values;
			}
		}
	}
}

//
// System.Data.Common.TdsDataColumn.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
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

using System.Collections;

namespace Mono.Data.Tds.Protocol {
	public class TdsDataColumn
	{
		int column_ordinal;
		TdsColumnType column_type;
		string column_name;

		Hashtable properties;

		public TdsDataColumn ()
		{
			SetDefaultValues ();
		}

		public TdsColumnType ColumnType {
			get {
				return column_type;
			}
			set {
				column_type = value;
			}
		}
		
		public string ColumnName {
			get {
				return column_name;
			}
			set {
				column_name = value;
			}
		}

		public int ColumnOrdinal {
			get {
				return column_ordinal;
			}
			set {
				column_ordinal = value;
			}
		}

		// This allows the storage of arbitrary properties in addition to the predefined ones
		public object this [object key] {
			get {
				if (properties == null)
					return null;
				return properties [key];
			}
			set {
				if (properties == null)
					properties = new Hashtable ();
				properties [key] = value;
			}
		}

		static object bool_true = true;
		static object bool_false = false;

		private void SetDefaultValues ()
		{
			this ["IsAutoIncrement"] = bool_false;
			this ["IsIdentity"] = bool_false;
			this ["IsRowVersion"] = bool_false;
			this ["IsUnique"] = bool_false;
			this ["IsHidden"] = bool_false;
		}
	}
}

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
#if NET_2_0
		int column_ordinal;
		TdsColumnType? column_type;
		string column_name;
		bool is_auto_increment;
		bool is_identity;
		bool is_row_version;
		bool is_unique;
		bool is_hidden;
#endif
		Hashtable properties;

#if !NET_2_0
		public TdsDataColumn ()
		{
			object bool_false = false;
			this ["IsAutoIncrement"] = bool_false;
			this ["IsIdentity"] = bool_false;
			this ["IsRowVersion"] = bool_false;
			this ["IsUnique"] = bool_false;
			this ["IsHidden"] = bool_false;
		}
#endif


#if NET_2_0
		public TdsColumnType? ColumnType {
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

		public bool IsAutoIncrement {
			get {
				return is_auto_increment;
			}
			set {
				is_auto_increment = value;
			}
		}

		public bool IsIdentity {
			get {
				return is_identity;
			}
			set {
				is_identity = value;
			}
		}

		public bool IsRowVersion {
			get {
				return is_row_version;
			}
			set {
				is_row_version = value;
			}
		}

		public bool IsUnique {
			get {
				return is_unique;
			}
			set {
				is_unique = value;
			}
		}

		public bool IsHidden {
			get {
				return is_hidden;
			}
			set {
				is_hidden = value;
			}
		}
#endif
		
		// This allows the storage of arbitrary properties in addition to the predefined ones
		public object this [string key] {
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
	}
}

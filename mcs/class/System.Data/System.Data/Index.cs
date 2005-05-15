/*
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

namespace System.Data
{
	/// <summary>
	/// Summary description for Index.
	/// </summary>
	internal class Index1 
	{

		// fields
		private DataTable _table;
		private DataColumn[] _columns;
		private bool _unique;
		private Node _root;
		private string _indexName;


		
		internal Index1 (string name, DataTable table, DataColumn[] columns,
			bool unique) 
		{

			_indexName = name;
			_table = table;
			_columns = columns;
			_unique = unique;
		}

		internal void SetUnique (bool unique)
		{
			_unique = unique;
		}

		internal Node Root
		{
			get 
			{
				return _root;
			}

			set 
			{
				_root = value;
			}
		}

		internal string Name 
		{
			get 
			{
				return _indexName;
			}

			set 
			{
		
			}
		}

		internal bool IsUnique
		{
			get 
			{
				return _unique;
			}
		}

		internal DataColumn[] Columns
		{
			get 
			{
				return _columns;    // todo: this gives back also primary key field!
			}
		}

		internal bool IsEquivalent (Index index) 
		{

			if (_unique == index._unique
				&& _columns.Length == index._columns.Length) {
				for (int j = 0; j < _columns.Length; j++) {
					if (_columns[j] != index._columns[j]) {
						return false;
					}
				}

				return true;
			}

			return false;
		}

		internal void Insert (Node i, DataRowVersion version)
		{
			DataRow  data  = i.Row;
			Node    n       = _root,
				x       = n;
			bool way     = true;
			int     compare = -1;
			bool needBalance = true;

			while (true) {
				if (n == null) {
					if (x == null) {
						_root = i;

						return;
					}

					Set(x, way, i);

					break;
				}

				DataRow nData = n.Row;

				if (data == nData) 	{
					//Set(x, way, i);
					needBalance = false;
					break;
				}

				compare = CompareRow(data, version, nData);

				if (compare == 0) {
					throw new ConstraintException("Unique key violation");
				}

				way = compare < 0;
				x   = n;
				n   = Child(x, way);
			}

			if(needBalance) {
				Balance(x, way);
			}
		}

		private void Balance(Node x, bool way)
		{

			while (true) {
            
				int sign = way ? 1
					: -1;

				switch (x.GetBalance() * sign) {

					case 1 :
						x.SetBalance(0);

						return;

					case 0 :
						x.SetBalance(-sign);
						break;

					case -1 :
						Node l = Child(x, way);

						if (l.GetBalance() == -sign) {
							Replace(x, l);
							Set(x, way, Child(l, !way));
							Set(l, !way, x);
							x.SetBalance(0);
							l.SetBalance(0);
						} 
						else {
							Node r = Child(l, !way);

							Replace(x, r);
							Set(l, !way, Child(r, way));
							Set(r, way, l);
							Set(x, way, Child(r, !way));
							Set(r, !way, x);

							int rb = r.GetBalance();

							x.SetBalance((rb == -sign) ? sign
								: 0);
							l.SetBalance((rb == sign) ? -sign
								: 0);
							r.SetBalance(0);
						}

						return;
				}

				if (x.Equals(_root)) {
					return;
				}

				way = x.From();
				x   = x.Parent;
			}
		}
		
		internal void Delete (DataRow row)
		{
			Node x = Search(row,DataRowVersion.Current);
			Delete (x);
		}

		internal void Delete(Node x)
		{
			if (x == null) {
				return;
			}

			Node n;

			if (x.Left == null) {
				n = x.Right;
			} 
			else if (x.Right == null) {
				n = x.Left;
			} 
			else {
				Node d = x;

				x = x.Left;

				// todo: this can be improved
				while (x.Right != null) {
					x = x.Right;
				}

				// x will be replaced with n later
				n = x.Left;

				// swap d and x
				int b = x.GetBalance();

				x.SetBalance(d.GetBalance());
				d.SetBalance(b);

				// set x.parent
				Node xp = x.Parent;
				Node dp = d.Parent;

				if (d == _root) {
					_root = x;
				}

				x.Parent = dp;

				if (dp != null) {
					if (dp.Right.Equals(d)) {
						dp.Right = x;
					} 
					else {
						dp.Left = x;
					}
				}

				// for in-memory tables we could use: d.rData=x.rData;
				// but not for cached tables
				// relink d.parent, x.left, x.right
				if (xp == d) {
					d.Parent = x;

					if (d.Left.Equals(x)) {
						x.Left = d;
						x.Right = d.Right;
					} 
					else {
						x.Right = d;
						x.Left = d.Left;
					}
				} 
				else {
					d.Parent = xp;
					xp.Right = d;
					x.Right = d.Right;
					x.Left = d.Left;
				}

				x.Right.Parent = x;
				x.Left.Parent = x;

				// set d.left, d.right
				d.Left = n;

				if (n != null) {
					n.Parent = d;
				}

				d.Right = null;

				x = d;
			}

			bool way = x.From();

			Replace(x, n);

			n = x.Parent;
			x.Delete();

			while (n != null) {
				x = n;

				int sign = way ? 1
					: -1;

				switch (x.GetBalance() * sign) 	{

					case -1 :
						x.SetBalance(0);
						break;

					case 0 :
						x.SetBalance(sign);

						return;

					case 1 :
						Node r = Child(x, !way);
						int  b = r.GetBalance();

						if (b * sign >= 0) {
							Replace(x, r);
							Set(x, !way, Child(r, way));
							Set(r, way, x);

							if (b == 0) {
								x.SetBalance(sign);
								r.SetBalance(-sign);

								return;
							}

							x.SetBalance(0);
							r.SetBalance(0);

							x = r;
						} 
						else {
							Node l = Child(r, way);

							Replace(x, l);

							b = l.GetBalance();

							Set(r, way, Child(l, !way));
							Set(l, !way, r);
							Set(x, !way, Child(l, way));
							Set(l, way, x);
							x.SetBalance((b == sign) ? -sign
								: 0);
							r.SetBalance((b == -sign) ? sign
								: 0);
							l.SetBalance(0);

							x = l;
						}
						break;
				}

				way = x.From();
				n   = x.Parent;
			}
		}

		internal Node[] FindAllSimple(DataColumn[] relatedColumns, int index)
		{
			if (_columns.Length == 0) {
				return new Node[0];
			}

			int tmpRecord = _columns[0].Table.RecordCache.NewRecord();

			try {
				for (int i = 0; i < relatedColumns.Length && i < _columns.Length; i++) {
					// according to MSDN: the DataType value for both columns must be identical.
					_columns[i].DataContainer.CopyValue(relatedColumns[i].DataContainer, index, tmpRecord);
				}

				return FindAllSimple(tmpRecord, relatedColumns.Length);
			}
			finally {
				_columns[0].Table.RecordCache.DisposeRecord(tmpRecord);
			}
		}
		
		internal Node[] FindAllSimple(int index, int length)
		{
			ArrayList nodes = new ArrayList();
			Node n = FindSimple (index, length, true);
			if (n == null)
				return new Node[0];
			while (n != null && ComparePartialRowNonUnique(index, n.Row.IndexFromVersion(DataRowVersion.Current), length) == 0) {
				nodes.Add (n);
				n = Next (n);
			}
			
			return (Node[])nodes.ToArray (typeof (Node));
		}

		internal Node FindSimple(int index, int length, bool first)
		{
			Node x      = _root, n;
			Node result = null;

			if (_columns.Length > 0 && _columns[0].DataContainer.IsNull(index)) {
				return null;
			}

			while (x != null) {
           
				int i = this.ComparePartialRowNonUnique(index, x.Row.IndexFromVersion(DataRowVersion.Current), length);

				if (i == 0) {
					if (first == false) {
						result = x;
						break;
					} 
					else if (result == x) {
						break;
					}
					result = x;
					n      = x.Left;
				} 
				else if (i > 0) {
					n = x.Right;
				} 
				else {
					n = x.Left;
				}

				if (n == null) 
				{
					break;
				}

				x = n;
			}

			return result;
		}

		internal Node Find(DataRow data, DataRowVersion version)
		{

			Node x = _root, n;

			while (x != null) {
				int i = CompareRow(data, version, x.Row);

				if (i == 0) {
					return x;
				} 
				else if (i > 0) {
					n = x.Right;
				} 
				else {
					n = x.Left;
				}

				if (n == null) {
					return null;
				}

				x = n;
			}

			return null;
		}

		//		internal Node FindFirst(Object value, int compare)
		//		{
		//			Node x     = _root;
		//			int  iTest = 1;
		//
		//			//        if (compare == Expression.BIGGER) {
		//			//            iTest = 0;
		//			//        }
		//
		//			while (x != null) {
		//				bool t = CompareValue(value, x.GetData()[0]) >= iTest;
		//
		//				if (t) {
		//					Node r = x.Right;
		//
		//					if (r == null) 
		//					{
		//						break;
		//					}
		//
		//					x = r;
		//				} 
		//				else {
		//					Node l = x.Left;
		//
		//					if (l == null) {
		//						break;
		//					}
		//
		//					x = l;
		//				}
		//			}
		//
		//			while (x != null && CompareValue(value, x.GetData()[0]) >= iTest) {
		//				x = Next(x);
		//			}
		//
		//			return x;
		//		}

		//		internal Node First()
		//		{
		//
		//			Node x = _root,
		//			l = x;
		//
		//			while (l != null) {
		//            
		//				x = l;
		//				l = x.Left;
		//			}
		//
		//			return x;
		//		}

		internal Node Next(Node x)
		{

			if (x == null) {
				return null;
			}

			Node r = x.Right;

			if (r != null) {
				x = r;

				Node l = x.Left;

				while (l != null) {
					x = l;
					l = x.Left;
				}

				return x;
			}

			Node ch = x;

			x = x.Parent;

			while (x != null && ch.Equals(x.Right)) {
            
				ch = x;
				x  = x.Parent;
			}

			return x;
		}

		private Node Child(Node x, bool w)
		{
			return w ? x.Left
				: x.Right;
		}

		private void Replace(Node x, Node n)
		{

			if (x.Equals(_root)) {
				_root = n;

				if (n != null) {
					n.Parent = null;
				}
			} 
			else {
				Set(x.Parent, x.From(), n);
			}
		}

		private void Set(Node x, bool w, Node n)
		{
			if (w) {
				x.Left = n;
			} 
			else {
				x.Right = n;
			}

			if (n != null) {
				n.Parent = x;
			}
		}
		
		private Node Search(DataRow r,DataRowVersion version)
		{

			Node x = _root;

			while (x != null) {
				int c = CompareRow(r, version,  x.Row);

				if (c == 0) {
					return x;
				} 
				else if (c < 0) {
					x = x.Left;
				} 
				else {
					x = x.Right;
				}
			}

			return null;
		}

		internal int ComparePartialRowNonUnique(int index1, int index2, int length)
		{
			int i = _columns[0].CompareValues(index1, index2);

			if (i != 0) {
				return i;
			}

			int fieldcount = _columns.Length;

			for (int j = 1; j < length && j < fieldcount; j++) {
				DataColumn column = _columns[j];

				if (column.DataContainer.IsNull(index1)) {
					continue;
				}

				i = column.CompareValues(index1, index2);

				if (i != 0) {
					return i;
				}
			}

			return 0;
		}

		//		private int CompareRowNonUnique(DataRow a, DataRow b)
		//		{
		//			if (a == b)
		//				return 0;
		//
		//			int i = DataColumn.CompareValues(a[0], GetNodeValue(b,0), _columns[0].DataType, !_columns[0].Table.CaseSensitive);
		//
		//			if (i != 0) {
		//				return i;
		//			}
		//
		//			int fieldcount = _columns.Length;
		//
		//			for (int j = 1; j < fieldcount; j++) {
		//				i = DataColumn.CompareValues(a[_columns[j].Ordinal], b[_columns[j].Ordinal], _columns[j].DataType, !_columns[j].Table.CaseSensitive);
		//
		//				if (i != 0) {
		//					return i;
		//				}
		//			}
		//
		//			return 0;
		//		}

		private int CompareRow(DataRow a, DataRowVersion version, DataRow b)
		{
			//			if (a == b)
			//				return 0;

			int index1 = a.IndexFromVersion(version);
			int index2 = b.IndexFromVersion(DataRowVersion.Current);
			for (int j = 0; j < _columns.Length; j++) {
				int i = _columns[j].DataContainer.CompareValues(index1, index2);

				if (i != 0) {
					return i;
				}
			}

			return 0;
		}

		//		private int CompareValue(Object a, Object b)
		//		{
		//			if (a == DBNull.Value) {
		//				if (b == DBNull.Value)
		//					return 0;
		//				return -1;
		//			}
		//
		//			if (b == DBNull.Value)
		//				return 1;
		//
		//			return System.Data.Common.DBComparerFactory.GetComparer(b.GetType(), false).Compare(a, b);
		//		}

		//		/// <summary>
		//		/// When we are inspectiong node (row) value in the index
		//		/// we are alway referencing its Current value
		//		/// </summary>
		//		private object GetNodeValue(DataRow row,DataColumn column)
		//		{
		//			return row[column,DataRowVersion.Current];
		//		}
		//
		//		/// <summary>
		//		/// When we are inspectiong node (row) value in the index
		//		/// we are alway referencing its Current value
		//		/// </summary>
		//		private object GetNodeValue(DataRow row,index idx)
		//		{
		//			return row[idx,DataRowVersion.Current];
		//		}
		
//		internal String GetString()
//		{
//			System.Text.StringBuilder sb = new System.Text.StringBuilder();
//			for(int i =0; i < this._columns.Length; i++) {
//				sb.Append("[ " + _columns[i].ColumnName + " ]  ");
//			}
//			sb.Append("\n");
//			if(this.Root != null) {
//				this.Root.CollectString(sb,0);
//			}
//			return sb.ToString();
//		}
	}
} */


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

namespace System.Data
{
	/// <summary>
	/// Summary description for Node.
	/// </summary>
	internal class Node
	{
		protected int _iBalance;    // currently, -2 means 'deleted'
		internal Node _nNext;       // node of next index (nNext==null || nNext.iId=iId+1)
		protected Node _nLeft;
		protected Node _nRight;
		protected Node _nParent;

		protected DataRow _row;
		
		public Node(DataRow row)
		{
			_row = row;
		}

		internal int GetBalance()
		{
			if (_iBalance == -2)
				throw new SystemException ("Node is deleted.");

			return _iBalance;
		}

		internal void Delete()
		{
			_iBalance = -2;
			_nLeft = null;
			_nRight = null;
			_nParent = null;
		}


		internal DataRow Row
		{
			get {
				return _row;
			}
		}

		internal Node Left
		{
			get {
				if (_iBalance == -2)
					throw new SystemException ("Node is deleted.");

				return _nLeft;
			}

			set {
				if (_iBalance == -2)
					throw new SystemException ("Node is deleted.");

				_nLeft = value;
			}
		}

		internal Node Right
		{
			get {
				if (_iBalance == -2)
					throw new SystemException ("Node is deleted.");
				return _nRight;
			}

			set {
				if (_iBalance == -2)
					throw new SystemException ("Node is deleted.");

				_nRight = value;
			}
		}


		internal Node Parent
		{
			get {
				if (_iBalance == -2)
					throw new SystemException ("Node is deleted.");

				return _nParent;
			}

			set {
				if (_iBalance == -2)
					throw new SystemException ("Node is deleted.");
				_nParent = value;
			}
		}

		internal bool IsRoot()
		{
			return _nParent == null;
		}


		internal void SetBalance(int b)
		{

			if (_iBalance == -2)
				throw new SystemException ("Node is deleted.");

			_iBalance = b;
		}

		internal bool From()
		{

			if (this.IsRoot()){
				return true;
			}

			if (_iBalance == -2)
				throw new SystemException ("Node is deleted.");
			Node parent = Parent;

			return Equals(parent.Left);
		}

		internal Object[] GetData()
		{

			if (_iBalance == -2)
				throw new SystemException ("Node is deleted.");
			return _row.ItemArray;
		}

		internal bool Equals(Node n)
		{

			if (_iBalance == -2)
				throw new SystemException ("Node is deleted.");

			return n == this;
		}
	}
}

//
// (C) 2006 Mainsoft Corporation (http://www.mainsoft.com)
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
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
using System.Collections.Generic;
using System.Text;

namespace System.ComponentModel
{
	[AttributeUsageAttribute (AttributeTargets.Class)]
	public sealed class DataObjectAttribute : Attribute
	{
		#region Fields

		public static readonly DataObjectAttribute DataObject;
		public static readonly DataObjectAttribute Default;
		public static readonly DataObjectAttribute NonDataObject;

		readonly bool _isDataObject;

		#endregion

		#region Constructors

		static DataObjectAttribute () {
			DataObject = new DataObjectAttribute (true);
			NonDataObject = new DataObjectAttribute (false);
			Default = NonDataObject;
		}

		public DataObjectAttribute () : this (true) { }
		public DataObjectAttribute (bool isDataObject) {
			_isDataObject = isDataObject;
		}

		#endregion

		#region Properties

		public bool IsDataObject { get { return _isDataObject; } }

		#endregion

		#region Methods

		public override bool Equals (object obj) {
			if (!(obj is DataObjectAttribute))
				return false;

			return ((DataObjectAttribute) obj).IsDataObject == IsDataObject;
		}
		public override int GetHashCode () {
			return IsDataObject.GetHashCode ();
		}
		public override bool IsDefaultAttribute () {
			return Default.Equals (this);
		}

		#endregion
	}
}

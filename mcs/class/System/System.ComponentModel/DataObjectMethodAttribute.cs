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
	[AttributeUsageAttribute (AttributeTargets.Method)]
	public sealed class DataObjectMethodAttribute : Attribute
	{
		#region Fields

		readonly DataObjectMethodType _methodType;
		readonly bool _isDefault;

		#endregion

		#region Constructors

		public DataObjectMethodAttribute (DataObjectMethodType methodType) : this (methodType, false) { }
		public DataObjectMethodAttribute (DataObjectMethodType methodType, bool isDefault) {
			_methodType = methodType;
			_isDefault = isDefault;
		}

		#endregion

		#region Properties

		public DataObjectMethodType MethodType { get { return _methodType; } }
		public bool IsDefault { get { return _isDefault; } }

		#endregion

		#region Methods

		public override bool Match (object obj) {
			if (!(obj is DataObjectMethodAttribute))
				return false;
			return ((DataObjectMethodAttribute) obj).MethodType == MethodType;
		}

		public override bool Equals (object obj) {
			return Match (obj) ?
				((DataObjectMethodAttribute) obj).IsDefault == IsDefault
				: false;
		}

		public override int GetHashCode () {
			return MethodType.GetHashCode () ^ IsDefault.GetHashCode ();
		}

		#endregion
	}
}

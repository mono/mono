//
// System.Drawing.BindingMemberInfo.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
//
// (C) 2002/3 Ximian, Inc.  http://www.ximian.com
//
//TODO:
// 1) Add real values in constructor.
// 2) Verify nocheck needed in GetHashCode.
// 3) Verify GetHashCode returns decent and valid hash.

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
using System.Windows.Forms;
namespace System.Windows.Forms {
	
	public struct BindingMemberInfo { 

		private string bindingfield;
		private string bindingpath;
		private string bindingmember;

		// -----------------------
		// Public Constructor
		// -----------------------

		/// <summary>
		/// 
		/// </summary>
		///
		/// <remarks>
		///
		/// </remarks>
		
		public BindingMemberInfo (string dataMember)
		{
			//TODO: Initilize with real values.
			bindingmember =  ("");
			bindingfield =  ("");
			bindingpath =  ("");
		}

		// -----------------------
		// Public Shared Members
		// -----------------------

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two BindingMemberInfo objects. The return value is
		///	based on the equivalence of the BindingMember, BindingPath,
		///	and BindingMember  properties of the two objects.
		/// </remarks>

		public static bool operator == (BindingMemberInfo bmi_a, 
			BindingMemberInfo bmi_b) {

			return ((bmi_a.bindingfield == bmi_b.bindingfield) &&
				(bmi_a.bindingpath == bmi_b.bindingpath)&&
				(bmi_a.bindingmember == bmi_b.bindingmember));
		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two BindingMemberInfo objects. The return value is
		///	based on the equivalence of the BindingMember, BindingPath,
		///	and BindingMember  properties of the two objects.
		/// </remarks>
		public static bool operator != (BindingMemberInfo bmi_a, 
			BindingMemberInfo bmi_b) {
			return ((bmi_a.bindingfield != bmi_b.bindingfield) ||
				(bmi_a.bindingpath != bmi_b.bindingpath)||
				(bmi_a.bindingmember != bmi_b.bindingmember));
		}
		
		// -----------------------
		// Public Instance Members
		// -----------------------


		public string BindingField {
			get{
				return bindingfield;
			}
		}

		public string BindingPath {
			get{
				return bindingpath;
			}
		}

		public string BindingMember {
			get{
				return bindingmember;
			}
		}
		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this BindingMemberInfo and another object.
		/// </remarks>
		
		public override bool Equals (object otherObject)
		{
			if (!(otherObject is BindingMemberInfo))
				return false;

			return (this == (BindingMemberInfo) otherObject);
		}

		/// <summary>
		///	GetHashCode Method
		/// </summary>
		///
		/// <remarks>
		///	Calculates a hashing value.
		/// </remarks>
		
		public override int GetHashCode ()
		{
			unchecked{
				// MONOTODO: This should not be checked, remove unchecked, if redundant.
				return (int)( bindingfield.GetHashCode() ^ bindingmember.GetHashCode() ^ bindingpath.GetHashCode());
			}
		}

	}
}

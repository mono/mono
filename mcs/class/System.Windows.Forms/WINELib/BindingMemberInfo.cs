//
// System.Drawing.BindingMemberInfo.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//
//TODO:
// 1) Add real values in constructor.
// 2) Verify nocheck needed in GetHashCode.
// 3) Verify GetHashCode returns decent and valid hash.


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
		
		public override bool Equals (object obj)
		{
			if (!(obj is BindingMemberInfo))
				return false;

			return (this == (BindingMemberInfo) obj);
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
			unchecked{// MONOTODO: This should not be checked, remove unchecked, if redundant.
				return (int)( bindingfield.GetHashCode() ^ bindingmember.GetHashCode() ^ bindingpath.GetHashCode());
			}
		}

		/// <summary>
		///	ToString Method
		/// </summary>
		///
		/// <remarks>
		///	Formats the BindingMemberInfo as a string.
		/// </remarks>
		
		public override string ToString ()
		{
			return String.Format ("[{0},{1},{2}]", bindingpath, bindingfield, bindingmember);
		}
	}
}

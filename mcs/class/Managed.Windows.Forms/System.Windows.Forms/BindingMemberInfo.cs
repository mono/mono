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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//


// COMPLETE

namespace System.Windows.Forms {
	public struct BindingMemberInfo {
		private string		data_member;
		private string		data_field;
		private string		data_path;

		#region Public Constructors
		public BindingMemberInfo(string dataMember) {
			int	i;

			if (dataMember!=null) {
				this.data_member=dataMember;
			} else {
				this.data_member=String.Empty;
			}
	
			// Break out our components
			i=data_member.LastIndexOf('.');
			if (i!=-1) {
				data_field=data_member.Substring(i+1);
				data_path=data_member.Substring(0, i);
			} else {
				data_field=data_member;
				data_path=String.Empty;
			}
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public string BindingField {
			get {
				return this.data_field;
			}
		}

		public string BindingMember {
			get {
				return this.data_member;
			}
		}

		public string BindingPath {
			get {
				return this.data_path;
			}
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		public override bool Equals(object otherObject) {
			if (otherObject is BindingMemberInfo) {
				return ((this.data_field == ((BindingMemberInfo)otherObject).data_field) &&
					(this.data_path == ((BindingMemberInfo)otherObject).data_path) &&
					(this.data_member == ((BindingMemberInfo)otherObject).data_member));
			} else {
				return false;
			}
		}

		public override int GetHashCode() {
			return this.data_member.GetHashCode();
		}
		#endregion	// Public Instance Methods

		#region Public Static Methods
		public static bool operator == (BindingMemberInfo a, BindingMemberInfo b)
		{
			return (a.Equals (b));
		}

		public static bool operator != (BindingMemberInfo a, BindingMemberInfo b)
		{
			return !(a.Equals (b));
		}
		#endregion	// Public Static Methods

	}
}

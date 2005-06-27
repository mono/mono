//
// System.ComponentModel.NotifyParentPropertyAttribute.cs
//
// Author:
//  Tim Coleman (tim@timcoleman.com)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// Copyright (C) Tim Coleman, 2002
// (C) 2003 Andreas Nahr
//
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

namespace System.ComponentModel {
	[AttributeUsage (AttributeTargets.Property)]
	public sealed class NotifyParentPropertyAttribute : Attribute {

		#region Fields

		private bool notifyParent;

		#endregion // Fields
		
		public static readonly NotifyParentPropertyAttribute Default = new NotifyParentPropertyAttribute (false);
		public static readonly NotifyParentPropertyAttribute No = new NotifyParentPropertyAttribute (false);
		public static readonly NotifyParentPropertyAttribute Yes = new NotifyParentPropertyAttribute (true);

		#region Constructors

		public NotifyParentPropertyAttribute (bool notifyParent)
		{
			this.notifyParent = notifyParent;
		}

		#endregion // Constructors

		#region Properties

		public bool NotifyParent {
			get { return notifyParent; }
		}

		#endregion // Properties

		#region Methods

		public override bool Equals (object obj)
		{
			if (!(obj is NotifyParentPropertyAttribute))
				return false;
			if (obj == this)
				return true;
			return ((NotifyParentPropertyAttribute) obj).NotifyParent == notifyParent;
		}

		public override int GetHashCode ()
		{
			return notifyParent.GetHashCode ();
		}

		public override bool IsDefaultAttribute ()
		{
			return notifyParent == NotifyParentPropertyAttribute.Default.NotifyParent;
		}

		#endregion // Methods
	}
}

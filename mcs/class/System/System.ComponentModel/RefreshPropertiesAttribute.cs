//
// System.ComponentModel.RefreshPropertiesAttribute.cs
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
	[AttributeUsage (AttributeTargets.All)]
	public sealed class RefreshPropertiesAttribute : Attribute {

		#region Fields

		RefreshProperties refresh;

		#endregion // Fields
		
		public static readonly RefreshPropertiesAttribute All = new RefreshPropertiesAttribute (RefreshProperties.All);
		public static readonly RefreshPropertiesAttribute Default = new RefreshPropertiesAttribute (RefreshProperties.None);
		public static readonly RefreshPropertiesAttribute Repaint = new RefreshPropertiesAttribute (RefreshProperties.Repaint);

		#region Constructors

		public RefreshPropertiesAttribute (RefreshProperties refresh)
		{
			this.refresh = refresh;
		}

		#endregion // Constructors

		#region Properties

		public RefreshProperties RefreshProperties {
			get { return refresh; }
		}

		#endregion // Properties

		#region Methods

		public override bool Equals (object obj)
		{
			if (!(obj is RefreshPropertiesAttribute))
				return false;
			if (obj == this)
				return true;
			return ((RefreshPropertiesAttribute) obj).RefreshProperties == refresh;
		}

		public override int GetHashCode ()
		{
			return refresh.GetHashCode ();
		}

		public override bool IsDefaultAttribute ()
		{
			return (this == RefreshPropertiesAttribute.Default);
		}

		#endregion // Methods
	}
}

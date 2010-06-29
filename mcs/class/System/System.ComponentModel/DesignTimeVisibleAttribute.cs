//
// System.ComponentModel.DesignTimeVisibleAttribute.cs
//
// Author:
//  Tim Coleman (tim@timcoleman.com)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// Copyright (C) Tim Coleman, 2002
// (C) 2003 Andreas Nahr
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
	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Interface)]
	public sealed class DesignTimeVisibleAttribute : Attribute 
	{
		#region Fields

		private bool visible;
		
		public static readonly DesignTimeVisibleAttribute Default = new DesignTimeVisibleAttribute (true);
		public static readonly DesignTimeVisibleAttribute No = new DesignTimeVisibleAttribute (false);
		public static readonly DesignTimeVisibleAttribute Yes = new DesignTimeVisibleAttribute (true);

		#endregion // Fields

		#region Constructors

		public DesignTimeVisibleAttribute ()
			: this (false)
		{
		}

		public DesignTimeVisibleAttribute (bool visible)
		{
			this.visible = visible; 
		}

		#endregion // Constructors

		#region Properties

		public bool Visible {
			get { return visible; }
		}

		#endregion // Properties

		#region Methods


		public override bool Equals (object obj)
		{
			if (!(obj is DesignTimeVisibleAttribute))
				return false;
			if (obj == this)
				return true;
			return ((DesignTimeVisibleAttribute) obj).Visible == visible;
		}

		public override int GetHashCode ()
		{
			return visible.GetHashCode ();
		}

		public override bool IsDefaultAttribute ()
		{
			return visible == DesignTimeVisibleAttribute.Default.Visible;
		}

		#endregion // Methods
	}
}

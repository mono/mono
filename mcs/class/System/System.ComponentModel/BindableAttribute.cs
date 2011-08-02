//
// System.ComponentModel.BindableAttribute.cs
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
	public sealed class BindableAttribute : Attribute {

		#region Fields

		//BindableSupport flags;
		private bool bindable;

		private BindingDirection direction = BindingDirection.OneWay;
		#endregion // Fields
		
		public static readonly BindableAttribute No = new BindableAttribute (BindableSupport.No);
		public static readonly BindableAttribute Yes = new BindableAttribute (BindableSupport.Yes);
		public static readonly BindableAttribute Default = new BindableAttribute (BindableSupport.Default);

		#region Constructors

		public BindableAttribute (BindableSupport flags)
		{
			//this.flags = flags;
			if (flags == BindableSupport.No)
				this.bindable = false;
				
			if (flags == BindableSupport.Yes || flags == BindableSupport.Default)
				this.bindable = true;
		}

		public BindableAttribute (bool bindable)
		{
			this.bindable = bindable;
		}

		public BindableAttribute (bool bindable, BindingDirection direction)
		{
			this.bindable = bindable;
			this.direction = direction;
		}

		public BindableAttribute (BindableSupport flags, BindingDirection direction): this (flags)
		{
			this.direction = direction;
		}
		
		public BindingDirection Direction {
			get { return direction; }
		}

		#endregion // Constructors

		#region Properties

		public bool Bindable {
			get { return bindable; }
		}

		#endregion // Properties

		#region Methods

		public override bool Equals (object obj)
		{
			if (!(obj is BindableAttribute))
				return false;

			if (obj == this)
				return true;

			return ((BindableAttribute) obj).Bindable == bindable;
		}

		public override int GetHashCode ()
		{
			return bindable.GetHashCode ();
		}

		public override bool IsDefaultAttribute ()
		{
			return bindable == BindableAttribute.Default.Bindable;
		}

		#endregion // Methods
	}
}


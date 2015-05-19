//
// ToolStripItemDesignerAvailabilityAttribute.cs
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
// Copyright (c) Jonathan Pobst
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

using System;
using System.Collections.Generic;
using System.Text;

namespace System.Windows.Forms.Design
{
	[AttributeUsage (AttributeTargets.Class)]
	public sealed class ToolStripItemDesignerAvailabilityAttribute : Attribute
	{
		private ToolStripItemDesignerAvailability visibility;
		public static readonly ToolStripItemDesignerAvailabilityAttribute Default = new ToolStripItemDesignerAvailabilityAttribute ();

		#region Public Constructors
		public ToolStripItemDesignerAvailabilityAttribute ()
			: base ()
		{
			this.visibility = ToolStripItemDesignerAvailability.None;
		}

		public ToolStripItemDesignerAvailabilityAttribute (ToolStripItemDesignerAvailability visibility)
			: base ()
		{
			this.visibility = visibility;
		}
		#endregion

		#region Public Properties
		public ToolStripItemDesignerAvailability ItemAdditionVisibility {
			get { return this.visibility; }
		}
		#endregion

		#region Public Methods
		public override bool Equals (object obj)
		{
			if (!(obj is ToolStripItemDesignerAvailabilityAttribute))
				return false;

			return this.ItemAdditionVisibility == (obj as ToolStripItemDesignerAvailabilityAttribute).ItemAdditionVisibility;
		}

		public override int GetHashCode ()
		{
			return (int)this.visibility;
		}

		public override bool IsDefaultAttribute ()
		{
			return this.visibility == ToolStripItemDesignerAvailability.None;
		}
		#endregion
	}
}

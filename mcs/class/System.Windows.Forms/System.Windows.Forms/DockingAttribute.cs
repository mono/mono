//
//  DockingAttribute.cs
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
// Copyright (c) 2006 Daniel Nauck
//
// Author:
//      Daniel Nauck    (dna(at)mono-project(dot)de)


using System;
using System.Windows.Forms;

namespace System.Windows.Forms
{
	[AttributeUsageAttribute(AttributeTargets.Class)] 
	public sealed class DockingAttribute : Attribute
	{
		private DockingBehavior dockingBehavior;

		public DockingAttribute()
		{
			dockingBehavior = DockingBehavior.Never;
		}

		public DockingAttribute(DockingBehavior dockingBehavior)
		{
			this.dockingBehavior = dockingBehavior;
		}

		public static readonly DockingAttribute Default = new DockingAttribute();

		public DockingBehavior DockingBehavior
		{
			get { return dockingBehavior; }
		}

		public override bool Equals(object obj)
		{
			if (obj is DockingAttribute)
				return (dockingBehavior == ((DockingAttribute)obj).DockingBehavior);
			else
				return false;
		}

		public override int GetHashCode()
		{
			return dockingBehavior.GetHashCode();
		}

		public override bool IsDefaultAttribute()
		{
			return Default.Equals(this);
		}
	}
}

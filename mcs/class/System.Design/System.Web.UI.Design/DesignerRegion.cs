//
// DesignerRegion.cs
//
// Author:
//   Marek Habersack <mhabersack@novell.com>
//
// (C) 2007 Novell, Inc.
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

#if NET_2_0
using System;
using System.Drawing;

namespace System.Web.UI.Design
{
	public class DesignerRegion : DesignerObject
	{
		public static readonly string DesignerRegionAttributeName;

		[MonoNotSupported ("")]
		public DesignerRegion (ControlDesigner designer, string name)
			: this (designer, name, false)
		{
			throw new NotImplementedException ();
		}
		
		[MonoNotSupported ("")]
		public DesignerRegion (ControlDesigner designer, string name, bool selectable)
			: base (designer, name)
		{
			throw new NotImplementedException ();
		}
		
		[MonoNotSupported ("")]
		public Rectangle GetBounds()
		{
			throw new NotImplementedException ();
		}
		
		[MonoNotSupported ("")]
		public virtual string Description {
			get {
				throw new NotImplementedException ();
			}
			
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoNotSupported ("")]
		public virtual string DisplayName {
			get {
				throw new NotImplementedException ();
			}
			
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoNotSupported ("")]
		public bool EnsureSize {
			get {
				throw new NotImplementedException ();
			}
			
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoNotSupported ("")]
		public virtual bool Highlight {
			get {
				throw new NotImplementedException ();
			}
			
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoNotSupported ("")]
		public virtual bool Selectable {
			get {
				throw new NotImplementedException ();
			}
			
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoNotSupported ("")]
		public virtual bool Selected {
			get {
				throw new NotImplementedException ();
			}
			
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoNotSupported ("")]
		public object UserData {
			get {
				throw new NotImplementedException ();
			}
			
			set {
				throw new NotImplementedException ();
			}
		}
	}
}
#endif

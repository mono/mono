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
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//    Miguel de Icaza (miguel@novell.com)
//

using System.Drawing;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace System.Windows.Forms.Design {
	
	public class ControlDesigner : ComponentDesigner {
		Control designed_control;
		
		public ControlDesigner () : base ()
		{
		}
		
		public override void Initialize (IComponent component)
		{
			if (component == null)
				throw new ArgumentNullException ("component");

			designed_control = component as Control;
			
			if (designed_control == null)
				throw new ArgumentException ("component", "Must derive from Control class");
		}

		public virtual Control Control {
			get {
				return designed_control;
			}
		}
	}
}

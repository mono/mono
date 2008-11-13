//Permission is hereby granted, free of charge, to any person obtaining
//a copy of this software and associated documentation files (the
//"Software"), to deal in the Software without restriction, including
//without limitation the rights to use, copy, modify, merge, publish,
//distribute, sublicense, and/or sell copies of the Software, and to
//permit persons to whom the Software is furnished to do so, subject to
//the following conditions:
//
//The above copyright notice and this permission notice shall be
//included in all copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//Copyright (c) 2008 Novell, Inc.
//
//Authors:
//	Andreia Gaita (avidigal@novell.com)
//

using System;

namespace Mono.WebBrowser.DOM
{
	public class NodeEventArgs : EventArgs
	{
		private DOM.INode node;

		#region Public Constructors
		public NodeEventArgs (DOM.INode node)
			: base ()
		{
			this.node = node;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public INode Node
		{
			get { return this.node; }
		}

		public IElement Element {
			get {
				if (this.node is IElement)
					return (IElement) node;
				return null;
			}
		}

		public IDocument Document {
			get {
				if (this.node is IDocument)
					return (IDocument) node;
				return null;
			}
		}
		#endregion	// Public Instance Properties
	}
	
	public class WindowEventArgs : EventArgs
	{
		private DOM.IWindow window;

		#region Public Constructors
		public WindowEventArgs (DOM.IWindow window)
			: base ()
		{
			this.window = window;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public IWindow Window
		{
			get { return this.window; }
		}
		#endregion	// Public Instance Properties
	}
	
}

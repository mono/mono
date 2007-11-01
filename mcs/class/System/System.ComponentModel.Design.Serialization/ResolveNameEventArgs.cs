//
// System.ComponentModel.Design.Serialization.ResolveNameEventArgs.cs
//
// Author:
//   Alejandro Sánchez Acosta (raciel@gnome.org)
//   Ivan N. Zlatev (contact@i-nz.net)
//
// (C) Alejandro Sánchez Acosta
// (C) 2007 Ivan N. Zlatev
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

namespace System.ComponentModel.Design.Serialization
{
	public class ResolveNameEventArgs : EventArgs
	{

		private string name;
		private object value;
	
		public ResolveNameEventArgs (string name) {
			this.name = name;
		}

		public string Name {
			get {
				return this.name;
			}
		}

		public object Value {
			get {
				return this.value;
			}

			set {
				this.value = value;
			}
		}			
	}
}

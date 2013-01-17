//
// System.Runtime.Serialization.StreamingContext.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Runtime.InteropServices;

namespace System.Runtime.Serialization {

	[Serializable]
	[System.Runtime.InteropServices.ComVisibleAttribute (true)]
	[StructLayout (LayoutKind.Sequential)]
	public struct StreamingContext {
		StreamingContextStates state;
		object additional;
		
		public StreamingContext (StreamingContextStates state)
		{
			this.state = state;
			additional = null;
		}

		public StreamingContext (StreamingContextStates state, object additional)
		{
			this.state = state;
			this.additional = additional;
		}

		public object Context {
			get {
				return additional;
			}
		}

		public StreamingContextStates State {
			get {
				return state;
			}
		}

		override public bool Equals (Object obj)
		{
			StreamingContext other;
			
			if (!(obj is StreamingContext))
				return false;

			other = (StreamingContext) obj;

			return (other.state == this.state) && (other.additional == this.additional);
		}

		override public int GetHashCode ()
		{
			return (int) state;
		}
	}
}

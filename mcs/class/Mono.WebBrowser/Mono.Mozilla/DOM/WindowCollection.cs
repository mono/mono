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
// Copyright (c) 2008 Novell, Inc.
//
// Authors:
//	Andreia Gaita (avidigal@novell.com)
//

using System;
using System.Collections;
using Mono.WebBrowser;
using Mono.WebBrowser.DOM;

namespace Mono.Mozilla.DOM
{
	internal class WindowCollection : DOMObject, IWindowCollection
	{
		protected nsIDOMWindowCollection unmanagedWindows;
		protected IWindow [] windows;
		protected int windowCount;
		
		public WindowCollection (WebBrowser control, nsIDOMWindowCollection windowCol) : base (control)
		{
			if (control.platform != control.enginePlatform)
				unmanagedWindows = nsDOMWindowCollection.GetProxy (control, windowCol);
			else
				unmanagedWindows = windowCol;
		}

		public WindowCollection (WebBrowser control) : base (control)
		{
			windows = new Window[0];
		}
		
		#region IDisposable Members
		protected override  void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					Clear ();
				}
			}
			base.Dispose(disposing);
		}		
		#endregion

		#region Helpers
		protected void Clear () 
		{
			if (windows != null) {
				for (int i = 0; i < windowCount; i++) {
					windows[i] = null;
				}
				windowCount = 0;
				windows = null;
			}
		}

		internal void Load ()
		{
			Clear ();
			uint count;
			unmanagedWindows.getLength (out count);
			Window[] tmpwindows = new Window[count];
			for (int i = 0; i < count;i++) {
				nsIDOMWindow window;
				unmanagedWindows.item ((uint)i, out window);
				tmpwindows[windowCount++] = new Window (control, (nsIDOMWindow)window);
			}
			windows = new Window[windowCount];
			Array.Copy (tmpwindows, windows, windowCount);
		}
		#endregion
		
		#region IEnumerable members
		public IEnumerator GetEnumerator () 
		{
			return new WindowEnumerator (this);
		}
		#endregion
		
		#region ICollection members
		public void CopyTo (Array dest, int index) 
		{
			if (windows != null) {
				Array.Copy (windows, 0, dest, index, windowCount);
			}
		}
	
		public int Count {
			get {
				if (unmanagedWindows != null && windows == null)
					Load ();
				return windowCount; 
			}
		}
		
		object ICollection.SyncRoot {
			get { return this; }
		}
		
		bool ICollection.IsSynchronized {
			get { return false; }
		}

		#endregion
		
		#region IList members
		public bool IsReadOnly 
		{
			get { return false;}
		}

		bool IList.IsFixedSize 
		{
			get { return false;}
		}

		void IList.RemoveAt  (int index) 
		{
			RemoveAt (index);			
		}
		
		public void RemoveAt (int index)
		{
			if (index > windowCount || index < 0)
				return;			
			Array.Copy (windows, index + 1, windows, index, (windowCount - index) - 1);
			windowCount--;
			windows[windowCount] = null;
		}
		
		public void Remove (IWindow window) 
		{
			this.RemoveAt (IndexOf (window));
		}

		void IList.Remove (object window) 
		{
			Remove (window as IWindow);
		}
		
		public void Insert (int index, IWindow value) 
		{
			if (index > windowCount)
				index = windowCount;
			IWindow[] tmp = new Window[windowCount+1];
			if (index > 0)
				Array.Copy (windows, 0, tmp, 0, index);
			tmp[index] = value;
			if (index < windowCount)
				Array.Copy (windows, index, tmp, index + 1, (windowCount - index));
			windows = tmp;
			windowCount++;
		}

		void IList.Insert (int index, object value) 
		{
			this.Insert (index, value as IWindow);
		}
		
		public int IndexOf (IWindow window) 
		{
			return Array.IndexOf (windows, window);
		}

		int IList.IndexOf (object window) 
		{
			return IndexOf (window as IWindow);
		}
		
		
		public bool Contains (IWindow window)
		{
			return this.IndexOf (window) != -1;
		}
		
		bool IList.Contains (object window)
		{
			return Contains (window as IWindow);			
		}
		
		void IList.Clear () 
		{
			this.Clear ();
		}
		
		public int Add (IWindow window) 
		{
			this.Insert (windowCount + 1, window as IWindow);
			return windowCount - 1;
		}
		
		int IList.Add (object window) 
		{
			return Add (window as IWindow);
		}
		
		object IList.this [int index] {
			get { 
				return this [index]; 
			}
			set { 
				this [index] = value as IWindow; 
			}
		}
		
		public IWindow this [int index] {
			get {
				if (index < 0 || index >= windowCount)
					throw new ArgumentOutOfRangeException ("index");
				return windows [index];								
			}
			set {
				if (index < 0 || index >= windowCount)
					throw new ArgumentOutOfRangeException ("index");
				windows [index] = value as IWindow;
			}
		}
		
		#endregion
		
		public override int GetHashCode () {
			if (this.unmanagedWindows != null)
				return this.unmanagedWindows.GetHashCode ();
			return base.GetHashCode ();
		}		

		internal class WindowEnumerator : IEnumerator {

			private WindowCollection collection;
			private int index = -1;

			public WindowEnumerator (WindowCollection collection)
			{
				this.collection = collection;
			}

			public object Current {
				get {
					if (index == -1)
						return null;
					return collection [index];
				}
			}

			public bool MoveNext ()
			{
				if (index + 1 >= collection.Count)
					return false;
				index++;
				return true;
			}

			public void Reset ()
			{
				index = -1;
			}
		}
		
	}
}

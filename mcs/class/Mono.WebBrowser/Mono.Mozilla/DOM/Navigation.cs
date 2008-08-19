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
// Copyright (c) 2007, 2008 Novell, Inc.
//
// Authors:
//	Andreia Gaita (avidigal@novell.com)
//

using System;
using System.Runtime.InteropServices;
using System.Text;
using Mono.WebBrowser;
using Mono.WebBrowser.DOM;

namespace Mono.Mozilla.DOM
{
	internal class Navigation: DOMObject, INavigation
	{

		internal nsIWebNavigation navigation;
		
		public Navigation (WebBrowser control, nsIWebNavigation webNav) : base (control)
		{
			this.navigation = webNav;
		}


		#region IDisposable Members
		protected override  void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					this.navigation = null;
				}
			}
			base.Dispose(disposing);
		}		
		#endregion	

		#region INavigation Members

		public bool CanGoBack {
			get {
				if (navigation == null)
					return false;
					
				bool canGoBack;
				navigation.getCanGoBack (out canGoBack);
				return canGoBack;
			}
		}

		public bool CanGoForward {
			get {
				if (navigation == null)
					return false;

				bool canGoForward;
				navigation.getCanGoForward (out canGoForward);
				return canGoForward;
			}
		}

		public bool Back ()
		{
			if (navigation == null)
				return false;

			control.Reset ();
			return navigation.goBack () == 0;
		}

		public bool Forward ()
		{
			if (navigation == null)
				return false;

			control.Reset ();
			return navigation.goForward () == 0;
		}

		public void Home ()
		{
			control.Reset ();
			Base.Home (control);
		}

		public void Reload ()
		{
			Reload (ReloadOption.None);
		}

		public void Reload (ReloadOption option)
		{
			if (navigation == null)
				return;

			control.Reset ();
			if (option == ReloadOption.None)
				navigation.reload ((uint)LoadFlags.None);
			else if (option == ReloadOption.Proxy)
				navigation.reload ((uint) LoadFlags.BypassLocalCache);
			else if (option == ReloadOption.Full)
				navigation.reload ((uint) LoadFlags.BypassProxy);
		}

		public void Stop ()
		{
			if (navigation == null)
				return;

			navigation.stop ((uint)StopOption.All);
		}
		
		
		/// <summary>
		/// Navigate to the page in the history, by index.
		/// </summary>
		/// <param name="index">
		/// A <see cref="System.Int32"/> representing an absolute index in the 
		/// history (that is, > -1 and < history length
		/// </param>
		public void Go (int index)
		{
			if (navigation == null || index < 0)
				return;
			
			int count;
			nsISHistory history;
			navigation.getSessionHistory (out history);
			history.getCount (out count);
			if (index > count)
				return;

			control.Reset ();
			navigation.gotoIndex (index);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index">
		/// A <see cref="System.Int32"/> representing an index in the 
		/// history, that can be relative or absolute depending on the relative argument
		/// </param>
		/// <param name="relative">
		/// A <see cref="System.Boolean"/> indicating whether the index is relative to 
		/// the current place in history or not (i.e., if relative = true, index can be
		/// positive or negative, and index=-1 means load the previous page in the history.
		/// if relative = false, index must be > -1, and index = 0 means load the first
		/// page of the history.
		/// </param>
		public void Go (int index, bool relative) {

			if (relative) {
				nsISHistory history;
				int count;
				int curIndex;

				navigation.getSessionHistory (out history);
				history.getCount (out count);
				history.getIndex (out curIndex);
				index = curIndex + index;
			}
			Go (index);			
		}
		
		public void Go (string url)
		{
			if (navigation == null)
				return;

			control.Reset ();
			navigation.loadURI (url, (uint)LoadFlags.None, null, null, null);
		}

		public void Go (string url, LoadFlags flags) 
		{
			if (navigation == null)
				return;
				
			control.Reset ();
			navigation.loadURI (url, (uint)flags, null, null, null);
		}

		public int HistoryCount {
			get {
				nsISHistory history;
				int count;
				navigation.getSessionHistory (out history);
				history.getCount (out count);
				return count;
			}
		}

		#endregion

		internal Document Document
		{
			get {
				nsIDOMDocument doc;
				this.navigation.getDocument (out doc);
				int hashcode = doc.GetHashCode ();
				if (!resources.ContainsKey (hashcode)) {
					resources.Add (hashcode, new Document (control, doc as nsIDOMHTMLDocument));
				}
				return resources [hashcode] as Document;
			}
		}
		
		public override int GetHashCode () {
			return this.navigation.GetHashCode ();
		}		
	}
}

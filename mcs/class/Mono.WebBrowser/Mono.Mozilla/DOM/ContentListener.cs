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
//	Andreia Gaita (shana@jitted.com)
//

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Mono.WebBrowser;

namespace Mono.Mozilla.DOM
{
	internal class ContentListener : nsIURIContentListener
	{
		WebBrowser owner;
		
		public ContentListener (WebBrowser instance) {
			owner = instance;
			
		}
			
		EventHandlerList events;
		public EventHandlerList Events {
			get { 
				if (events == null)
					events = new EventHandlerList ();
				return events;
			}
		}
		
		public void AddHandler (NavigationRequestedEventHandler value) {
			if (Events[WebBrowser.NavigationRequestedEvent] == null) {
				if (owner.Navigation != null) {
					nsIWebBrowser browser = (nsIWebBrowser) owner.navigation.navigation;
					browser.setParentURIContentListener (this);
				}
			}
			Events.AddHandler (WebBrowser.NavigationRequestedEvent, value);
		}
		
		public void RemoveHandler (NavigationRequestedEventHandler value) {
			Events.RemoveHandler (WebBrowser.NavigationRequestedEvent, value);
		}
		
		bool nsIURIContentListener.onStartURIOpen (nsIURI aURI)
		{
			NavigationRequestedEventHandler eh = (NavigationRequestedEventHandler) (Events[WebBrowser.NavigationRequestedEvent]);
			if (eh != null) {
				AsciiString uri = new Mono.Mozilla.AsciiString ("");
				aURI.getSpec (uri.Handle);
				NavigationRequestedEventArgs args = new NavigationRequestedEventArgs (uri.ToString ());
				eh (this, args);
				return args.Cancel;
				
			}
			return true;
		}

		bool nsIURIContentListener.doContent (string aContentType,
				 bool aIsContentPreferred,
				 nsIRequest aRequest,
				out nsIStreamListener aContentHandler)
		{
			aContentHandler = null;
			return true;
		}

		bool nsIURIContentListener.isPreferred (string aContentType,
				ref string aDesiredContentType)
		{
			return true;
		}

		bool nsIURIContentListener.canHandleContent (string aContentType,
				 bool aIsContentPreferred,
				ref string aDesiredContentType)
		{
			return true;
		}

		[return: MarshalAs (UnmanagedType.Interface)] IntPtr nsIURIContentListener.getLoadCookie ()
		{
			return IntPtr.Zero;
		}

		void nsIURIContentListener.setLoadCookie ([MarshalAs (UnmanagedType.Interface)] IntPtr value)
		{
			return;
		}

		nsIURIContentListener nsIURIContentListener.getParentContentListener ()
		{
			return null;
		}

		void nsIURIContentListener.setParentContentListener (nsIURIContentListener value)
		{
			return;
		}
	}
}

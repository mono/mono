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
using System.IO;

namespace Mono.WebBrowser.DOM
{
	public interface IElement : INode
	{
		IElementCollection 	All { get; }
		IElementCollection 	Children { get; }

		int ClientWidth		{ get; }
		int ClientHeight	{ get; }
		int ScrollHeight	{ get; }
		int ScrollWidth		{ get; }
		int ScrollLeft		{ get; set;}
		int ScrollTop		{ get; set; }
		int OffsetHeight	{ get; }
		int OffsetWidth		{ get; }
		int OffsetLeft		{ get; }
		int OffsetTop		{ get; }
		IElement OffsetParent { get; }

		string 				InnerText { get; set; }
		string 				InnerHTML { get; set; }
		string 				OuterText { get; set; }
		string 				OuterHTML { get; set; }
		string 				Style { get; set; }
		int 				TabIndex { get; set; }
		string 				TagName { get; }
		bool				Disabled { get; set; }
		Stream				ContentStream { get; }

		IElement			AppendChild (IElement child);
		void				Blur ();
		void				Focus ();
		bool 				HasAttribute (string name);
		string 				GetAttribute (string name);
		IElementCollection 	GetElementsByTagName (string id);
		int 				GetHashCode ();
		void				ScrollIntoView (bool alignWithTop);
		void 				SetAttribute (string name, string value);
		
	}
}

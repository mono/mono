//
// ChtmlTextWriter.cs: Compact HTML
//
// Author:
//	Cesar Lopez Nataren <cnataren@novell.com>
//

//
// Copyright (C) 2006-2010 Novell, Inc (http://www.novell.com)
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

using System;
using System.IO;
using System.Collections;

namespace System.Web.UI {

	public class ChtmlTextWriter : Html32TextWriter
	{
		static Hashtable global_suppressed_attrs;

		static string [] global_suppressed_attributes = {
			"onclick", "ondblclick", "onmousedown", "onmouseup",
			"onmouseover", "onmousemove", "onmouseout",
			"onkeypress", "onkeydown", "onkeyup"
		};

		static string [] recognized_attributes = {"div", "span"};

		Hashtable recognized_attrs = new Hashtable (recognized_attributes.Length);
		Hashtable suppressed_attrs = new Hashtable (recognized_attributes.Length);

		static ChtmlTextWriter ()
		{
			SetupGlobalSuppressedAttrs (global_suppressed_attributes);
		}

		static void SetupGlobalSuppressedAttrs (string [] attrs)
		{
			global_suppressed_attrs = new Hashtable ();
			PopulateHash (global_suppressed_attrs, global_suppressed_attributes);
		}

		static void PopulateHash (Hashtable hash, string [] keys)
		{
			foreach (string key in keys)
				hash.Add (key, true);
		}

		public ChtmlTextWriter (TextWriter writer)
			: this (writer, DefaultTabString)
		{
		}

		public ChtmlTextWriter (TextWriter writer, string tabString)
			: base (writer, tabString)
		{
			//
			// setup the recognized attrs
			//
			foreach (string key in recognized_attributes)
				recognized_attrs.Add (key, new Hashtable ());

			SetupSuppressedAttrs ();
		}


		void SetupSuppressedAttrs ()
		{
			//
			// we don't make these static because they are not read-only
			//
			string [] div_suppressed_attributes = {
				"accesskey", "cellspacing", "cellpadding",
				"gridlines", "rules"
			};

			string [] span_suppressed_attributes = {
				"cellspacing", "cellpadding",
				"gridlines", "rules"
			};

			Init ("div", div_suppressed_attributes, suppressed_attrs);
			Init ("span", span_suppressed_attributes, suppressed_attrs);
		}

		static void Init (string key, string [] attrs, Hashtable container)
		{
			Hashtable attrs_hash = new Hashtable (attrs.Length);
			PopulateHash (attrs_hash, attrs);
			container.Add (key, attrs_hash);
		}

		protected Hashtable GlobalSuppressedAttributes {
			get { return global_suppressed_attrs; }
		}

		protected Hashtable RecognizedAttributes {
			get { return recognized_attrs; }
		}

		protected Hashtable SuppressedAttributes {
			get { return suppressed_attrs; }
		}

		public virtual void AddRecognizedAttribute (string elementName, string attributeName)
		{
			Hashtable elem_attrs = (Hashtable) recognized_attrs [elementName];

			if (elem_attrs == null) {
				elem_attrs = new Hashtable ();
				elem_attrs.Add (attributeName, true);
				recognized_attrs.Add (elementName, elem_attrs);
			} else
				elem_attrs.Add (attributeName, true);
		}

		public virtual void RemoveRecognizedAttribute (string elementName, string attributeName)
		{
			Hashtable elem_attrs = (Hashtable) recognized_attrs [elementName];

			if (elem_attrs != null)
				elem_attrs.Remove (attributeName);
		}

		//
		// writes <br>
		//
		public override void WriteBreak ()
		{
			string br = GetTagName (HtmlTextWriterTag.Br);
			WriteBeginTag (br);
			Write (TagRightChar);
		}

		public override void WriteEncodedText (string text)
		{
			base.WriteEncodedText (text);
		}

		Hashtable attr_render = new Hashtable ();

		protected override bool OnAttributeRender (string name, string value, HtmlTextWriterAttribute key)
		{
			// FIXME:
			// I checked every possible HtmlTextWriterAttribute key
			// and always throws ArgumentNullException.
			return (bool) attr_render [null];
		}

		protected override bool OnStyleAttributeRender (string name, string value, HtmlTextWriterStyle key)
		{
			return key == HtmlTextWriterStyle.Display;
		}

		protected override bool OnTagRender (string name, HtmlTextWriterTag key)
		{
			return key != HtmlTextWriterTag.Span;
		}
	}
}

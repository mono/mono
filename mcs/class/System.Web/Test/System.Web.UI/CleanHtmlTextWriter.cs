//
// CleanHtmlTextWriter.cs
//
// An HtmlTextWriter that cleans stuff up for you a bit. Helps writing tests
// because you do not have to reproduce the attribute order, etc.
// 
// Author:
//     Ben Maurer  <bmaurer@novell.com>
// 
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

//#define TEST_THIS

using System.Web.UI;
using System;
using System.IO;
using System.Collections;

class CleanHtmlTextWriter : HtmlTextWriter {
	public CleanHtmlTextWriter (TextWriter tw) : base (tw)
	{
		tw.NewLine = "\n";
	}

	ArrayList pending_attrs = new ArrayList ();
	ArrayList pending_styles = new ArrayList ();

	class PendingStyle : IComparable {
		public string name, value;
		public HtmlTextWriterStyle s;		
		
		public PendingStyle (string name, string value, HtmlTextWriterStyle s)
		{
			this.name = name;
			this.value = value;
			this.s = s;
		}
		
		public int CompareTo (object o)
		{
			return string.CompareOrdinal (name, ((PendingStyle) o).name);
		}
	}
	
	class PendingAttribute : IComparable {
		public string name, value;
		public HtmlTextWriterAttribute a;
		public bool encode;
		public bool know_encode;
		
		public PendingAttribute (string name, string value, HtmlTextWriterAttribute a, bool encode, bool know_encode)
		{
			this.name = name;
			this.value = value;
			this.a = a;
			this.encode = encode;
			this.know_encode = know_encode;
		}

		public int CompareTo (object o)
		{
			return string.CompareOrdinal (name, ((PendingAttribute) o).name);
		}	
	}
	bool filtering = false;

	//
	// Some idiot at microsoft did not do a sanity check on this api,
	// thus forcing me to deal with some serious pain.
	//
	public override void AddAttribute (HtmlTextWriterAttribute key, string value)
	{
		if (filtering) {
			base.AddAttribute (key, value);
			return;
		}
		
		pending_attrs.Add (new PendingAttribute (GetAttributeName (key), value, key, false, false));
	}

	public override void AddAttribute (HtmlTextWriterAttribute key, string value, bool fEncode)
	{
		if (filtering) {
			base.AddAttribute (key, value, fEncode);
			return;
		}
		
		pending_attrs.Add (new PendingAttribute (GetAttributeName (key), value, key, fEncode, true));
	}
	
	public override void AddAttribute (string name, string value)
	{
		if (filtering) {
			base.AddAttribute (name, value);
			return;
		}
				
		pending_attrs.Add (new PendingAttribute (name, value, 0, false, false));
	}
	
	public override void AddAttribute (string name, string value, bool fEncode)
	{
		if (filtering) {
			base.AddAttribute (name, value, fEncode);
			return;
		}
		
		pending_attrs.Add (new PendingAttribute (name, value, 0, fEncode, true));
	}
	
	protected override void AddAttribute (string name, string value, HtmlTextWriterAttribute key)
	{
		if (filtering) {
			base.AddAttribute (name, value, key);
			return;
		}
		
		pending_attrs.Add (new PendingAttribute (name, value, key, false, false));
	}

	// TODO: use the above retardation in this stuff
	protected override void AddStyleAttribute (string name, string value, HtmlTextWriterStyle s)
	{
		pending_styles.Add (new PendingStyle (name, value, s));
	}

	protected override void FilterAttributes ()
	{
		pending_attrs.Sort ();
		pending_styles.Sort ();

		filtering = true;
		foreach (PendingAttribute a in pending_attrs) {
			if (a.a == 0) {
				if (a.know_encode)
					base.AddAttribute (a.name, a.value, a.encode);
				else
					base.AddAttribute (a.name, a.value, a.encode);
			} else {
				if (a.know_encode)
					base.AddAttribute (a.a, a.value, a.encode);
				else
					base.AddAttribute (a.a, a.value, a.encode);
			}
		}
		
		foreach (PendingStyle s in pending_styles)
			base.AddStyleAttribute (s.name, s.value, s.s);

		filtering = false;
		pending_attrs.Clear ();
		pending_styles.Clear ();
		
		base.FilterAttributes ();
	}

#if TEST_THIS
	static void Main ()
	{
		HtmlTextWriter w = new CleanHtmlTextWriter (Console.Out);
		w.AddAttribute (HtmlTextWriterAttribute.Name, "abcd");
		w.AddAttribute (HtmlTextWriterAttribute.Id, "efg");
		w.RenderBeginTag (HtmlTextWriterTag.Input);
		w.RenderEndTag ();
		Console.WriteLine ();
	}
#endif
}

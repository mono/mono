// 
// System.Web.UI.HtmlTextWriter
//
// Author:
//        Ben Maurer <bmaurer@novell.com>
//
// (c) 2005 Novell
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

using System.IO;
using System.Globalization;
using System.Collections;
using System.Text;

namespace System.Web.UI {
	
	public class HtmlTextWriter : TextWriter {

		ArrayList styles = new ArrayList ();
		ArrayList attrs = new ArrayList ();

		class AddedStyle {
			public string name;
			public HtmlTextWriterStyle key;
			public string value;
		}
		
		class AddedAttr {
			public string name;
			public HtmlTextWriterAttribute key;
			public string value;
		}

		public HtmlTextWriter (TextWriter writer) : this (writer, DefaultTabString)
		{
		}
	
		public HtmlTextWriter (TextWriter writer, string tabString)
		{
			if (writer == null)
				throw new ArgumentNullException ();

			b = writer;
			tab_string = tabString;
		}

		[MonoTODO]
		internal static string StaticGetStyleName (System.Web.UI.HtmlTextWriterStyle styleKey) 
		{
			return "";
		}
		
		[MonoTODO]
		protected static void RegisterAttribute (string name, HtmlTextWriterAttribute key)
		{
		}
		
		[MonoTODO]
		protected static void RegisterStyle (string name, HtmlTextWriterStyle key)
		{
		}
		
		[MonoTODO]
		protected static void RegisterTag (string name, HtmlTextWriterTag key)
		{
		}
		
	
		public virtual void AddAttribute (HtmlTextWriterAttribute key, string value, bool fEncode)
		{
			if (fEncode)
				value = EncodeAttributeValue (key, value);
			AddAttribute (GetAttributeName (key), value, key);
		}
		
		
		public virtual void AddAttribute (HtmlTextWriterAttribute key, string value)
		{
			AddAttribute (key, value, true);
		}
	
	
		public virtual void AddAttribute (string name, string value, bool fEncode)
		{
			if (fEncode)
				; // FIXME

			AddAttribute (name, value, GetAttributeKey (name));
		}
		
		public virtual void AddAttribute (string name, string value)
		{
			AddAttribute (name, value, true);
		}
	
		protected virtual void AddAttribute (string name, string value, HtmlTextWriterAttribute key)
		{
			AddedAttr a = new AddedAttr ();
			a.name = name;
			a.value = value;
			a.key = key;
			attrs.Add (a);
		}
		
		
		protected virtual void AddStyleAttribute (string name, string value, HtmlTextWriterStyle key)
		{
			AddedStyle a = new AddedStyle ();
			a.name = name;
			a.value = value;
			a.key = key;
			styles.Add (a);
		}
		

		public virtual void AddStyleAttribute (string name, string value)
		{
			AddStyleAttribute (name, value, GetStyleKey (name));
		}
		
		public virtual void AddStyleAttribute (HtmlTextWriterStyle key, string value)
		{
			AddStyleAttribute (GetStyleName (key), value, key);
		}
	
		public override void Close ()
		{
			b.Close ();	
		}

		[MonoTODO]
		protected virtual string EncodeAttributeValue (HtmlTextWriterAttribute attrKey, string value)
		{
			return value;
		}
		
		[MonoTODO]
		protected string EncodeAttributeValue (string value, bool fEncode)
		{
			return value;
		}
		
		[MonoTODO]
		protected string EncodeUrl (string url)
		{
			return url;
		}
		

		protected virtual void FilterAttributes ()
		{
			AddedAttr style_attr = null;
			
			foreach (AddedAttr a in attrs) {
				if (OnAttributeRender (a.name, a.value, a.key)) {
					if (a.key == HtmlTextWriterAttribute.Style) {
						style_attr = a;
						continue;
					}
					
					WriteAttribute (a.name, a.value, false);
				}
			}
			
			attrs.Clear ();

			if (styles.Count != 0 || style_attr != null) {
				Write (SpaceChar);
				Write ("style");
				Write (EqualsDoubleQuoteString);

				foreach (AddedStyle a in styles)
					if (OnStyleAttributeRender (a.name, a.value, a.key))
						WriteStyleAttribute (a.name, a.value, false);

				if (style_attr != null)
					Write (style_attr.value);
								
				Write (DoubleQuoteChar);
			}
			
			styles.Clear ();
		}
	
		public override void Flush ()
		{
			b.Flush ();
		}

		[MonoTODO]
		protected HtmlTextWriterAttribute GetAttributeKey (string attrName)
		{
			// I don't think we want to binary search
			// because there might be something added to
			// the enum later. Do we really need anything
			// faster than a linear search?
			
			foreach (HtmlAttribute t in htmlattrs) {
				if (t.name == attrName)
					return t.key;
			}

			return 0;		
		}

		[MonoTODO]
		protected string GetAttributeName (HtmlTextWriterAttribute attrKey)
		{
			if ((int) attrKey < htmlattrs.Length)
				return htmlattrs [(int) attrKey].name;

			return null;
		}
		
		[MonoTODO]
		protected HtmlTextWriterStyle GetStyleKey (string styleName)
		{
			// I don't think we want to binary search
			// because there might be something added to
			// the enum later. Do we really need anything
			// faster than a linear search?
			
			foreach (HtmlStyle t in htmlstyles) {
				if (t.name == styleName)
					return t.key;
			}

			return 0;			
		}
		
		[MonoTODO]
		protected string GetStyleName (HtmlTextWriterStyle styleKey)
		{
			if ((int) styleKey < htmlstyles.Length)
				return htmlstyles [(int) styleKey].name;

			return null;
		}
		
		[MonoTODO]
		protected virtual HtmlTextWriterTag GetTagKey (string tagName) 
		{
			// I don't think we want to binary search
			// because there might be something added to
			// the enum later. Do we really need anything
			// faster than a linear search?
			
			foreach (HtmlTag t in tags) {
				if (t.name == tagName)
					return t.key;
			}

			return HtmlTextWriterTag.Unknown;
		}

		internal static string StaticGetTagName (HtmlTextWriterTag tagKey)
		{
			if ((int) tagKey < tags.Length)
				return tags [(int) tagKey].name;

			return null;	
		}
		
		
		[MonoTODO]
		protected virtual string GetTagName (HtmlTextWriterTag tagKey)
		{
			if ((int) tagKey < tags.Length)
				return tags [(int) tagKey].name;

			return null;
		}
		
		protected bool IsAttributeDefined (HtmlTextWriterAttribute key)
		{
			string value;
			return IsAttributeDefined (key, out value);
		}
	
		protected bool IsAttributeDefined (HtmlTextWriterAttribute key, out string value)
		{
			foreach (AddedAttr a in attrs)
				if (a.key == key) {
					value = a.value;
					return true;
				}

			value = null;
			return false;
		}
		
		protected bool IsStyleAttributeDefined (HtmlTextWriterStyle key)
		{
			string value;
			return IsStyleAttributeDefined (key, out value);
		}
	
		protected bool IsStyleAttributeDefined (HtmlTextWriterStyle key, out string value)
		{

			foreach (AddedStyle a in styles)
				if (a.key == key){
					value = a.value;
					return true;
				}

			value = null;
			return false;
		}
		
		protected virtual bool OnAttributeRender (string name, string value, HtmlTextWriterAttribute key)
		{
			return true;
		}
		
		protected virtual bool OnStyleAttributeRender (string name, string value, HtmlTextWriterStyle key)
		{
			return true;
		}
		
		protected virtual bool OnTagRender (string name, HtmlTextWriterTag key)
		{
			return true;
		}
		
	
		protected virtual void OutputTabs ()
		{
			if (! newline)
				return;
			newline = false;
			
			for (int i = 0; i < Indent; i ++)
				b.Write (tab_string);
		}
	
		class TagStack {
			public string name;
			public HtmlTextWriterTag key;
			public TagStack next;
		}

		TagStack cur_tag;
			
		protected string PopEndTag ()
		{
			if (cur_tag == null)
				throw new InvalidOperationException ();
			
			string s = TagName;
			cur_tag = cur_tag.next;
			return s;
		}
		
		protected void PushEndTag (string endTag)
		{
			// TODO optimize -- too much memory!
			TagStack ts = new TagStack ();
			ts.next = cur_tag;
			cur_tag = ts;
			TagName = endTag;
		}

		void PushEndTag (HtmlTextWriterTag t)
		{
			// TODO optimize!
			TagStack ts = new TagStack ();
			ts.next = cur_tag;
			cur_tag = ts;
			TagKey = t;
		}
		

		protected virtual string RenderAfterContent ()
		{
			return null;
		}
		
		protected virtual string RenderAfterTag ()
		{
			return null;
		}
		
		protected virtual string RenderBeforeContent ()
		{
			return null;
		}
			
		protected virtual string RenderBeforeTag ()
		{
			return null;
		}

		public virtual void RenderBeginTag (string tagName)
		{
			if (! OnTagRender (tagName, GetTagKey (tagName)))
				return;

			PushEndTag (tagName);
			
			DoBeginTag ();
		}
		
		public virtual void RenderBeginTag (HtmlTextWriterTag tagKey)
		{
			if (! OnTagRender (GetTagName (tagKey), tagKey))
				return;

			PushEndTag (tagKey);

			DoBeginTag ();
		}

		void WriteIfNotNull (string s)
		{
			if (s != null)
				Write (s);
		}
		

		void DoBeginTag ()
		{
			WriteIfNotNull (RenderBeforeTag ());
			WriteBeginTag (TagName);
			FilterAttributes ();

			HtmlTextWriterTag key = (int) TagKey < tags.Length ? TagKey : HtmlTextWriterTag.Unknown;

			switch (tags [(int) key].tag_type) {
			case TagType.Inline:
				Write (TagRightChar);
				break;
			case TagType.Block:
				Write (TagRightChar);
				WriteLine ();
				Indent ++;
				break;
			case TagType.SelfClosing:
				Write (SelfClosingTagEnd);
				break;
			}
			
			// FIXME what do i do for self close here?
			WriteIfNotNull (RenderBeforeContent ());
		}
		

		public virtual void RenderEndTag ()
		{
			// FIXME what do i do for self close here?
			WriteIfNotNull (RenderAfterContent ());
			
			HtmlTextWriterTag key = (int) TagKey < tags.Length ? TagKey : HtmlTextWriterTag.Unknown;

			switch (tags [(int) key].tag_type) {
			case TagType.Inline:
				WriteEndTag (TagName);
				break;
			case TagType.Block:
				Indent --;
				WriteLineNoTabs ("");
				WriteEndTag (TagName);
				
				break;
			case TagType.SelfClosing:
				// NADA
				break;
			}
			WriteIfNotNull (RenderAfterTag ());

			PopEndTag ();
		}
		

		public virtual void WriteAttribute (string name, string value, bool fEncode)
		{
			Write (SpaceChar);
			Write (name);
			if (value != null) {
				Write (EqualsDoubleQuoteString);
				value = EncodeAttributeValue (value, fEncode);
				Write (value);
				Write (DoubleQuoteChar);
			}
		}
		
	
		public virtual void WriteBeginTag (string tagName)
		{
			Write (TagLeftChar);
			Write (tagName);
		}
		
		public virtual void WriteEndTag (string tagName)
		{
			Write (EndTagLeftChars);
			Write (tagName);
			Write (TagRightChar);	
		}
		
		public virtual void WriteFullBeginTag (string tagName)
		{
			Write (TagLeftChar);
			Write (tagName);
			Write (TagRightChar);
		}
			
		public virtual void WriteStyleAttribute (string name, string value)
		{
			WriteStyleAttribute (name, value, true);
		}

		public virtual void WriteStyleAttribute (string name, string value, bool fEncode)
		{
			Write (name);
			Write (StyleEqualsChar);
			Write (EncodeAttributeValue (value, fEncode));
			Write (SemicolonChar);
		}
		
		public override void Write (char [] buffer, int index, int count)
		{
			OutputTabs ();
			b.Write (buffer, index, count);
		}
	
		public override void Write (double value)
		{
			OutputTabs ();
			b.Write (value);
		}
	
		public override void Write (char value)
		{
			OutputTabs ();
			b.Write (value);
		}
	
		public override void Write (char [] buffer)
		{
			OutputTabs ();
			b.Write (buffer);
		}
	
		public override void Write (int value)
		{
			OutputTabs ();
			b.Write (value);
		}
	
		public override void Write (string format, object arg0)
		{
			OutputTabs ();
			b.Write (format, arg0);
		}
	
		public override void Write (string format, object arg0, object arg1)
		{
			OutputTabs ();
			b.Write (format, arg0, arg1);
		}
	
		public override void Write (string format, params object [] args)
		{
			OutputTabs ();
			b.Write (format, args);
		}

		public override void Write (string s)
		{
			OutputTabs ();
			b.Write (s);
		}
		
		public override void Write (long value)
		{
			OutputTabs ();
			b.Write (value);
		}
	
		public override void Write (object value)
		{
			OutputTabs ();
			b.Write (value);
		}
		
		public override void Write (float value)
		{
			OutputTabs ();
			b.Write (value);
		}
	
		public override void Write (bool value)
		{
			OutputTabs ();
			b.Write (value);
		}
	
		public virtual void WriteAttribute (string name, string value)
		{
			WriteAttribute (name, value, true);
		}

		public override void WriteLine (char value)
		{
			OutputTabs ();
			b.WriteLine (value);
			newline = true;
		}
	
		public override void WriteLine (long value)
		{
			OutputTabs ();
			b.WriteLine (value);
			newline = true;
		}
	
		public override void WriteLine (object value)
		{
			OutputTabs ();
			b.WriteLine (value);
			newline = true;	
		}
	
		public override void WriteLine (double value)
		{
			OutputTabs ();
			b.WriteLine (value);
			newline = true;
		}
	
		public override void WriteLine (char [] buffer, int index, int count)
		{
			OutputTabs ();
			b.WriteLine (buffer, index, count);
			newline = true;
		}
	
		public override void WriteLine (char [] buffer)
		{
			OutputTabs ();
			b.WriteLine (buffer);
			newline = true;
		}
		
		public override void WriteLine (bool value)
		{
			OutputTabs ();
			b.WriteLine (value);
			newline = true;
		}
	
		public override void WriteLine ()
		{
			OutputTabs ();
			b.WriteLine ();
			newline = true;
		}
	
		public override void WriteLine (int value)
		{
			OutputTabs ();
			b.WriteLine (value);
			newline = true;
		}
	
		public override void WriteLine (string format, object arg0, object arg1)
		{
			OutputTabs ();
			b.WriteLine (format, arg0, arg1);
			newline = true;
		}
	
		public override void WriteLine (string format, object arg0)
		{
			OutputTabs ();
			b.WriteLine (format, arg0);
			newline = true;
		}
	
		public override void WriteLine (string format, params object [] args)
		{
			OutputTabs ();
			b.WriteLine (format, args);
			newline = true;
		}
		
		[CLSCompliant (false)]
		public override void WriteLine (uint value)
		{
			OutputTabs ();
			b.WriteLine (value);
			newline = true;
		}
	
		public override void WriteLine (string s)
		{
			OutputTabs ();
			b.WriteLine (s);
			newline = true;
		}
	
		public override void WriteLine (float value)
		{
			OutputTabs ();
			b.WriteLine (value);
			newline = true;
		}
	
		public void WriteLineNoTabs (string s)
		{
			b.WriteLine (s);
			newline = true;
		}

		public override Encoding Encoding {
			get {
				return b.Encoding;	
			}
		}

		int indent;
		public int Indent {
			get {
				return indent;
			}
			set {
				indent = value;
			}
		}
	
		public System.IO.TextWriter InnerWriter {
			get {
				return b;
			}
			set {
				b = value;
			}	
		}
	
		public override string NewLine {
			get {
				return b.NewLine;
			}
			set {
				b.NewLine = value;
			}
		}
	
		protected HtmlTextWriterTag TagKey {
			get {
				if (cur_tag == null)
					throw new InvalidOperationException ();

				return cur_tag.key;
			}
			set {
				cur_tag.key = value;
				cur_tag.name = GetTagName (value);
			}
		}
	
		protected string TagName {
			get {
				if (cur_tag == null)
					throw new InvalidOperationException ();
				
				return cur_tag.name;
			}
			set {
				cur_tag.name = value;
				cur_tag.key = GetTagKey (value);
				if (cur_tag.key != HtmlTextWriterTag.Unknown)
					cur_tag.name = GetTagName (cur_tag.key);
			}
		}
		

		TextWriter b;
		string tab_string;
		bool newline;
	
		public const string DefaultTabString = "\t";
		public const char DoubleQuoteChar = '"';
		public const string EndTagLeftChars = "</";
		public const char EqualsChar = '=';
		public const string EqualsDoubleQuoteString = "=\"";
		public const string SelfClosingChars = " /";
		public const string SelfClosingTagEnd = " />";
		public const char SemicolonChar = ';';
		public const char SingleQuoteChar = '\'';
		public const char SlashChar = '/';
		public const char SpaceChar = ' ' ;
		public const char StyleEqualsChar = ':';
		public const char TagLeftChar = '<';
		public const char TagRightChar = '>';

		enum TagType {
			Block,
			Inline,
			SelfClosing,
		}
		
		
		struct HtmlTag {
			public HtmlTextWriterTag key;
			public string name;
			public TagType tag_type;

			public HtmlTag (HtmlTextWriterTag k, string n, TagType tt)
			{
				key = k;
				name = n;
				tag_type = tt;
			}
		}

		struct HtmlStyle {
			public HtmlTextWriterStyle key;
			public string name;
			
			public HtmlStyle (HtmlTextWriterStyle k, string n)
			{
				key = k;
				name = n;
			}
		}

		
		struct HtmlAttribute {
			public HtmlTextWriterAttribute key;
			public string name;

			public HtmlAttribute (HtmlTextWriterAttribute k, string n)
			{
				key = k;
				name = n;
			}
		}
		
		static HtmlTag [] tags = {
			new HtmlTag (HtmlTextWriterTag.Unknown,    "",                  TagType.Block),
			new HtmlTag (HtmlTextWriterTag.A,          "a",                 TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Acronym,    "acronym",           TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Address,    "address",           TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Area,       "area",              TagType.Block),
			new HtmlTag (HtmlTextWriterTag.B,          "b",                 TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Base,       "base",              TagType.SelfClosing),
			new HtmlTag (HtmlTextWriterTag.Basefont,   "basefont",          TagType.SelfClosing),
			new HtmlTag (HtmlTextWriterTag.Bdo,        "bdo",               TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Bgsound,    "bgsound",           TagType.SelfClosing),
			new HtmlTag (HtmlTextWriterTag.Big,        "big",               TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Blockquote, "blockquote",        TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Body,       "body",              TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Br,         "br",                TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Button,     "button",            TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Caption,    "caption",           TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Center,     "center",            TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Cite,       "cite",              TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Code,       "code",              TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Col,        "col",               TagType.SelfClosing),
			new HtmlTag (HtmlTextWriterTag.Colgroup,   "colgroup",          TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Dd,         "dd",                TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Del,        "del",               TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Dfn,        "dfn",               TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Dir,        "dir",               TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Div,        "div",               TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Dl,         "dl",                TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Dt,         "dt",                TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Em,         "em",                TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Embed,      "embed",             TagType.SelfClosing),
			new HtmlTag (HtmlTextWriterTag.Fieldset,   "fieldset",          TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Font,       "font",              TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Form,       "form",              TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Frame,      "frame",             TagType.SelfClosing),
			new HtmlTag (HtmlTextWriterTag.Frameset,   "frameset",          TagType.Block),
			new HtmlTag (HtmlTextWriterTag.H1,         "h1",                TagType.Block),
			new HtmlTag (HtmlTextWriterTag.H2,         "h2",                TagType.Block),
			new HtmlTag (HtmlTextWriterTag.H3,         "h3",                TagType.Block),
			new HtmlTag (HtmlTextWriterTag.H4,         "h4",                TagType.Block),
			new HtmlTag (HtmlTextWriterTag.H5,         "h5",                TagType.Block),
			new HtmlTag (HtmlTextWriterTag.H6,         "h6",                TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Head,       "head",              TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Hr,         "hr",                TagType.SelfClosing),
			new HtmlTag (HtmlTextWriterTag.Html,       "html",              TagType.Block),
			new HtmlTag (HtmlTextWriterTag.I,          "i",                 TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Iframe,     "iframe",            TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Img,        "img",               TagType.SelfClosing),
			new HtmlTag (HtmlTextWriterTag.Input,      "input",             TagType.SelfClosing),
			new HtmlTag (HtmlTextWriterTag.Ins,        "ins",               TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Isindex,    "isindex",           TagType.SelfClosing),
			new HtmlTag (HtmlTextWriterTag.Kbd,        "kbd",               TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Label,      "label",             TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Legend,     "legend",            TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Li,         "li",                TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Link,       "link",              TagType.SelfClosing),
			new HtmlTag (HtmlTextWriterTag.Map,        "map",               TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Marquee,    "marquee",           TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Menu,       "menu",              TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Meta,       "meta",              TagType.SelfClosing),
			new HtmlTag (HtmlTextWriterTag.Nobr,       "nobr",              TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Noframes,   "noframes",          TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Noscript,   "noscript",          TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Object,     "object",            TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Ol,         "ol",                TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Option,     "option",            TagType.Block),
			new HtmlTag (HtmlTextWriterTag.P,          "p",                 TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Param,      "param",             TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Pre,        "pre",               TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Q,          "q",                 TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Rt,         "rt",                TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Ruby,       "ruby",              TagType.Block),
			new HtmlTag (HtmlTextWriterTag.S,          "s",                 TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Samp,       "samp",              TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Script,     "script",            TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Select,     "select",            TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Small,      "small",             TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Span,       "span",              TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Strike,     "strike",            TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Strong,     "strong",            TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Style,      "style",             TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Sub,        "sub",               TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Sup,        "sup",               TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Table,      "table",             TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Tbody,      "tbody",             TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Td,         "td",                TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Textarea,   "textarea",          TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Tfoot,      "tfoot",             TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Th,         "th",                TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Thead,      "thead",             TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Title,      "title",             TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Tr,         "tr",                TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Tt,         "tt",                TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.U,          "u",                 TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Ul,         "ul",                TagType.Block),
			new HtmlTag (HtmlTextWriterTag.Var,        "var",               TagType.Inline),
			new HtmlTag (HtmlTextWriterTag.Wbr,        "wbr",               TagType.SelfClosing),
			new HtmlTag (HtmlTextWriterTag.Xml,        "xml",               TagType.Block),
		};

		HtmlAttribute [] htmlattrs = {
			new HtmlAttribute (HtmlTextWriterAttribute.Accesskey,   "accesskey"),
			new HtmlAttribute (HtmlTextWriterAttribute.Align,       "align"),
			new HtmlAttribute (HtmlTextWriterAttribute.Alt,         "alt"),
			new HtmlAttribute (HtmlTextWriterAttribute.Background,  "background"),
			new HtmlAttribute (HtmlTextWriterAttribute.Bgcolor,     "bgcolor"),
			new HtmlAttribute (HtmlTextWriterAttribute.Border,      "border"),
			new HtmlAttribute (HtmlTextWriterAttribute.Bordercolor, "bordercolor"),
			new HtmlAttribute (HtmlTextWriterAttribute.Cellpadding, "cellpadding"),
			new HtmlAttribute (HtmlTextWriterAttribute.Cellspacing, "cellspacing"),
			new HtmlAttribute (HtmlTextWriterAttribute.Checked,     "checked"),
			new HtmlAttribute (HtmlTextWriterAttribute.Class,       "class"),
			new HtmlAttribute (HtmlTextWriterAttribute.Cols,        "cols"),
			new HtmlAttribute (HtmlTextWriterAttribute.Colspan,     "colspan"),
			new HtmlAttribute (HtmlTextWriterAttribute.Disabled,    "disabled"),
			new HtmlAttribute (HtmlTextWriterAttribute.For,         "for"),
			new HtmlAttribute (HtmlTextWriterAttribute.Height,      "height"),
			new HtmlAttribute (HtmlTextWriterAttribute.Href,        "href"),
			new HtmlAttribute (HtmlTextWriterAttribute.Id,          "id"),
			new HtmlAttribute (HtmlTextWriterAttribute.Maxlength,   "maxlength"),
			new HtmlAttribute (HtmlTextWriterAttribute.Multiple,    "multiple"),
			new HtmlAttribute (HtmlTextWriterAttribute.Name,        "name"),
			new HtmlAttribute (HtmlTextWriterAttribute.Nowrap,      "nowrap"),
			new HtmlAttribute (HtmlTextWriterAttribute.Onchange,    "onchange"),
			new HtmlAttribute (HtmlTextWriterAttribute.Onclick,     "onclick"),
			new HtmlAttribute (HtmlTextWriterAttribute.ReadOnly,    "readonly"),
			new HtmlAttribute (HtmlTextWriterAttribute.Rows,        "rows"),
			new HtmlAttribute (HtmlTextWriterAttribute.Rowspan,     "rowspan"),
			new HtmlAttribute (HtmlTextWriterAttribute.Rules,       "rules"),
			new HtmlAttribute (HtmlTextWriterAttribute.Selected,    "selected"),
			new HtmlAttribute (HtmlTextWriterAttribute.Size,        "size"),
			new HtmlAttribute (HtmlTextWriterAttribute.Src,         "src"),
			new HtmlAttribute (HtmlTextWriterAttribute.Style,       "style"),
			new HtmlAttribute (HtmlTextWriterAttribute.Tabindex,    "tabindex"),
			new HtmlAttribute (HtmlTextWriterAttribute.Target,      "target"),
			new HtmlAttribute (HtmlTextWriterAttribute.Title,       "title"),
			new HtmlAttribute (HtmlTextWriterAttribute.Type,        "type"),
			new HtmlAttribute (HtmlTextWriterAttribute.Valign,      "valign"),
			new HtmlAttribute (HtmlTextWriterAttribute.Value,       "value"),
			new HtmlAttribute (HtmlTextWriterAttribute.Width,       "width"),
			new HtmlAttribute (HtmlTextWriterAttribute.Wrap,        "wrap"),
		};

		HtmlStyle [] htmlstyles = {
			new HtmlStyle (HtmlTextWriterStyle.BackgroundColor, "background-color"),
			new HtmlStyle (HtmlTextWriterStyle.BackgroundImage, "background-image"),
			new HtmlStyle (HtmlTextWriterStyle.BorderCollapse,  "border-collapse"),
			new HtmlStyle (HtmlTextWriterStyle.BorderColor,     "border-color"),
			new HtmlStyle (HtmlTextWriterStyle.BorderStyle,     "border-style"),
			new HtmlStyle (HtmlTextWriterStyle.BorderWidth,     "border-width"),
			new HtmlStyle (HtmlTextWriterStyle.Color,           "color"),
			new HtmlStyle (HtmlTextWriterStyle.FontFamily,      "font-family"),
			new HtmlStyle (HtmlTextWriterStyle.FontSize,        "font-size"),
			new HtmlStyle (HtmlTextWriterStyle.FontStyle,       "font-style"),
			new HtmlStyle (HtmlTextWriterStyle.FontWeight,      "font-weight"),
			new HtmlStyle (HtmlTextWriterStyle.Height,          "height"),
			new HtmlStyle (HtmlTextWriterStyle.TextDecoration,  "text-decoration"),
			new HtmlStyle (HtmlTextWriterStyle.Width,           "width"),
		};
	}
}

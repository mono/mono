
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
/*	System.Web.UI
*	Authors
*		Leen Toelen (toelen@hotmail.com)
*/

using System;
using System.IO;
using System.Collections;
using System.Web;
using System.Web.UI;

namespace System.Web.UI{

public class HtmlTextWriter : System.IO.TextWriter {

static HtmlTextWriter(){
	HtmlTextWriter._tagKeyLookupTable = new Hashtable(97,CaseInsensitiveHashCodeProvider.Default,
							  CaseInsensitiveComparer.Default);
	HtmlTextWriter._tagNameLookupArray = new TagInformation[97];
	HtmlTextWriter.RegisterTag("", HtmlTextWriterTag.Unknown, TagType.Other);
	HtmlTextWriter.RegisterTag("a", HtmlTextWriterTag.A, TagType.Inline);
	HtmlTextWriter.RegisterTag("acronym", HtmlTextWriterTag.Acronym, TagType.Inline);
	HtmlTextWriter.RegisterTag("address", HtmlTextWriterTag.Address, TagType.Other);
	HtmlTextWriter.RegisterTag("area", HtmlTextWriterTag.Area, TagType.Other);
	HtmlTextWriter.RegisterTag("b", HtmlTextWriterTag.B, TagType.Inline);
	HtmlTextWriter.RegisterTag("base", HtmlTextWriterTag.Base, TagType.NonClosing);
	HtmlTextWriter.RegisterTag("basefont", HtmlTextWriterTag.Basefont, TagType.NonClosing);
	HtmlTextWriter.RegisterTag("bdo", HtmlTextWriterTag.Bdo, TagType.Inline);
	HtmlTextWriter.RegisterTag("bgsound", HtmlTextWriterTag.Bgsound, TagType.NonClosing);
	HtmlTextWriter.RegisterTag("big", HtmlTextWriterTag.Big, TagType.Inline);
	HtmlTextWriter.RegisterTag("blockquote", HtmlTextWriterTag.Blockquote, TagType.Other);
	HtmlTextWriter.RegisterTag("body", HtmlTextWriterTag.Body, TagType.Other);
	HtmlTextWriter.RegisterTag("br", HtmlTextWriterTag.Br, TagType.Other);
	HtmlTextWriter.RegisterTag("button", HtmlTextWriterTag.Button, TagType.Inline);
	HtmlTextWriter.RegisterTag("caption", HtmlTextWriterTag.Caption, TagType.Other);
	HtmlTextWriter.RegisterTag("center", HtmlTextWriterTag.Center, TagType.Other);
	HtmlTextWriter.RegisterTag("cite", HtmlTextWriterTag.Cite, TagType.Inline);
	HtmlTextWriter.RegisterTag("code", HtmlTextWriterTag.Code, TagType.Inline);
	HtmlTextWriter.RegisterTag("col", HtmlTextWriterTag.Col, TagType.NonClosing);
	HtmlTextWriter.RegisterTag("colgroup", HtmlTextWriterTag.Colgroup, TagType.Other);
	HtmlTextWriter.RegisterTag("del", HtmlTextWriterTag.Del, TagType.Inline);
	HtmlTextWriter.RegisterTag("dd", HtmlTextWriterTag.Dd, TagType.Inline);
	HtmlTextWriter.RegisterTag("dfn", HtmlTextWriterTag.Dfn, TagType.Inline);
	HtmlTextWriter.RegisterTag("dir", HtmlTextWriterTag.Dir, TagType.Other);
	HtmlTextWriter.RegisterTag("div", HtmlTextWriterTag.Div, TagType.Other);
	HtmlTextWriter.RegisterTag("dl", HtmlTextWriterTag.Dl, TagType.Other);
	HtmlTextWriter.RegisterTag("dt", HtmlTextWriterTag.Dt, TagType.Inline);
	HtmlTextWriter.RegisterTag("em", HtmlTextWriterTag.Em, TagType.Inline);
	HtmlTextWriter.RegisterTag("embed", HtmlTextWriterTag.Embed, TagType.NonClosing);
	HtmlTextWriter.RegisterTag("fieldset", HtmlTextWriterTag.Fieldset, TagType.Other);
	HtmlTextWriter.RegisterTag("font", HtmlTextWriterTag.Font, TagType.Inline);
	HtmlTextWriter.RegisterTag("form", HtmlTextWriterTag.Form, TagType.Other);
	HtmlTextWriter.RegisterTag("frame", HtmlTextWriterTag.Frame, TagType.NonClosing);
	HtmlTextWriter.RegisterTag("frameset", HtmlTextWriterTag.Frameset, TagType.Other);
	HtmlTextWriter.RegisterTag("h1", HtmlTextWriterTag.H1, TagType.Other);
	HtmlTextWriter.RegisterTag("h2", HtmlTextWriterTag.H2, TagType.Other);
	HtmlTextWriter.RegisterTag("h3", HtmlTextWriterTag.H3, TagType.Other);
	HtmlTextWriter.RegisterTag("h4", HtmlTextWriterTag.H4, TagType.Other);
	HtmlTextWriter.RegisterTag("h5", HtmlTextWriterTag.H5, TagType.Other);
	HtmlTextWriter.RegisterTag("h6", HtmlTextWriterTag.H6, TagType.Other);
	HtmlTextWriter.RegisterTag("head", HtmlTextWriterTag.Head, TagType.Other);
	HtmlTextWriter.RegisterTag("hr", HtmlTextWriterTag.Hr, TagType.NonClosing);
	HtmlTextWriter.RegisterTag("html", HtmlTextWriterTag.Html, TagType.Other);
	HtmlTextWriter.RegisterTag("i", HtmlTextWriterTag.I, TagType.Inline);
	HtmlTextWriter.RegisterTag("iframe", HtmlTextWriterTag.Iframe, TagType.Other);
	HtmlTextWriter.RegisterTag("img", HtmlTextWriterTag.Img, TagType.NonClosing);
	HtmlTextWriter.RegisterTag("input", HtmlTextWriterTag.Input, TagType.NonClosing);
	HtmlTextWriter.RegisterTag("ins", HtmlTextWriterTag.Ins, TagType.Inline);
	HtmlTextWriter.RegisterTag("isindex", HtmlTextWriterTag.Isindex, TagType.NonClosing);
	HtmlTextWriter.RegisterTag("kbd", HtmlTextWriterTag.Kbd, TagType.Inline);
	HtmlTextWriter.RegisterTag("label", HtmlTextWriterTag.Label, TagType.Inline);
	HtmlTextWriter.RegisterTag("legend", HtmlTextWriterTag.Legend, TagType.Other);
	HtmlTextWriter.RegisterTag("li", HtmlTextWriterTag.Li, TagType.Inline);
	HtmlTextWriter.RegisterTag("link", HtmlTextWriterTag.Link, TagType.NonClosing);
	HtmlTextWriter.RegisterTag("map", HtmlTextWriterTag.Map, TagType.Other);
	HtmlTextWriter.RegisterTag("marquee", HtmlTextWriterTag.Marquee, TagType.Other);
	HtmlTextWriter.RegisterTag("menu", HtmlTextWriterTag.Menu, TagType.Other);
	HtmlTextWriter.RegisterTag("meta", HtmlTextWriterTag.Meta, TagType.NonClosing);
	HtmlTextWriter.RegisterTag("nobr", HtmlTextWriterTag.Nobr, TagType.Inline);
	HtmlTextWriter.RegisterTag("noframes", HtmlTextWriterTag.Noframes, TagType.Other);
	HtmlTextWriter.RegisterTag("noscript", HtmlTextWriterTag.Noscript, TagType.Other);
	HtmlTextWriter.RegisterTag("object", HtmlTextWriterTag.Object, TagType.Other);
	HtmlTextWriter.RegisterTag("ol", HtmlTextWriterTag.Ol, TagType.Other);
	HtmlTextWriter.RegisterTag("option", HtmlTextWriterTag.Option, TagType.Other);
	HtmlTextWriter.RegisterTag("p", HtmlTextWriterTag.P, TagType.Inline);
	HtmlTextWriter.RegisterTag("param", HtmlTextWriterTag.Param, TagType.Other);
	HtmlTextWriter.RegisterTag("pre", HtmlTextWriterTag.Pre, TagType.Other);
	HtmlTextWriter.RegisterTag("q", HtmlTextWriterTag.Q, TagType.Inline);
	HtmlTextWriter.RegisterTag("rt", HtmlTextWriterTag.Rt, TagType.Other);
	HtmlTextWriter.RegisterTag("ruby", HtmlTextWriterTag.Ruby, TagType.Other);
	HtmlTextWriter.RegisterTag("s", HtmlTextWriterTag.S, TagType.Inline);
	HtmlTextWriter.RegisterTag("samp", HtmlTextWriterTag.Samp, TagType.Inline);
	HtmlTextWriter.RegisterTag("script", HtmlTextWriterTag.Script, TagType.Other);
	HtmlTextWriter.RegisterTag("select", HtmlTextWriterTag.Select, TagType.Other);
	HtmlTextWriter.RegisterTag("small", HtmlTextWriterTag.Small, TagType.Other);
	HtmlTextWriter.RegisterTag("span", HtmlTextWriterTag.Span, TagType.Inline);
	HtmlTextWriter.RegisterTag("strike", HtmlTextWriterTag.Strike, TagType.Inline);
	HtmlTextWriter.RegisterTag("strong", HtmlTextWriterTag.Strong, TagType.Inline);
	HtmlTextWriter.RegisterTag("style", HtmlTextWriterTag.Style, TagType.Other);
	HtmlTextWriter.RegisterTag("sub", HtmlTextWriterTag.Sub, TagType.Inline);
	HtmlTextWriter.RegisterTag("sup", HtmlTextWriterTag.Sup, TagType.Inline);
	HtmlTextWriter.RegisterTag("table", HtmlTextWriterTag.Table, TagType.Other);
	HtmlTextWriter.RegisterTag("tbody", HtmlTextWriterTag.Tbody, TagType.Other);
	HtmlTextWriter.RegisterTag("td", HtmlTextWriterTag.Td, TagType.Inline);
	HtmlTextWriter.RegisterTag("textarea", HtmlTextWriterTag.Textarea, TagType.Inline);
	HtmlTextWriter.RegisterTag("tfoot", HtmlTextWriterTag.Tfoot, TagType.Other);
	HtmlTextWriter.RegisterTag("th", HtmlTextWriterTag.Th, TagType.Inline);
	HtmlTextWriter.RegisterTag("thead", HtmlTextWriterTag.Thead, TagType.Other);
	HtmlTextWriter.RegisterTag("title", HtmlTextWriterTag.Title, TagType.Other);
	HtmlTextWriter.RegisterTag("tr", HtmlTextWriterTag.Tr, TagType.Other);
	HtmlTextWriter.RegisterTag("tt", HtmlTextWriterTag.Tt, TagType.Inline);
	HtmlTextWriter.RegisterTag("u", HtmlTextWriterTag.U, TagType.Inline);
	HtmlTextWriter.RegisterTag("ul", HtmlTextWriterTag.Ul, TagType.Other);
	HtmlTextWriter.RegisterTag("var", HtmlTextWriterTag.Var, TagType.Inline);
	HtmlTextWriter.RegisterTag("wbr", HtmlTextWriterTag.Wbr, TagType.NonClosing);
	HtmlTextWriter.RegisterTag("xml", HtmlTextWriterTag.Xml, TagType.Other);

	HtmlTextWriter._attrKeyLookupTable = new Hashtable(40,CaseInsensitiveHashCodeProvider.Default,
							   CaseInsensitiveComparer.Default);
	HtmlTextWriter._attrNameLookupArray = new AttributeInformation[40];
	HtmlTextWriter.RegisterAttribute("accesskey", HtmlTextWriterAttribute.Accesskey, true);
	HtmlTextWriter.RegisterAttribute("align", HtmlTextWriterAttribute.Align, false);
	HtmlTextWriter.RegisterAttribute("alt", HtmlTextWriterAttribute.Alt, true);
	HtmlTextWriter.RegisterAttribute("background", HtmlTextWriterAttribute.Background, true);
	HtmlTextWriter.RegisterAttribute("bgcolor", HtmlTextWriterAttribute.Bgcolor, false);
	HtmlTextWriter.RegisterAttribute("border", HtmlTextWriterAttribute.Border, false);
	HtmlTextWriter.RegisterAttribute("bordercolor", HtmlTextWriterAttribute.Bordercolor, false);
	HtmlTextWriter.RegisterAttribute("cellpadding", HtmlTextWriterAttribute.Cellpadding, false);
	HtmlTextWriter.RegisterAttribute("cellspacing", HtmlTextWriterAttribute.Cellspacing, false);
	HtmlTextWriter.RegisterAttribute("checked", HtmlTextWriterAttribute.Checked, false);
	HtmlTextWriter.RegisterAttribute("class", HtmlTextWriterAttribute.Class, true);
	HtmlTextWriter.RegisterAttribute("cols", HtmlTextWriterAttribute.Cols, false);
	HtmlTextWriter.RegisterAttribute("colspan", HtmlTextWriterAttribute.Colspan, false);
	HtmlTextWriter.RegisterAttribute("disabled", HtmlTextWriterAttribute.Disabled, false);
	HtmlTextWriter.RegisterAttribute("for", HtmlTextWriterAttribute.For, false);
	HtmlTextWriter.RegisterAttribute("height", HtmlTextWriterAttribute.Height, false);
	HtmlTextWriter.RegisterAttribute("href", HtmlTextWriterAttribute.Href, true);
	HtmlTextWriter.RegisterAttribute("id", HtmlTextWriterAttribute.Id, false);
	HtmlTextWriter.RegisterAttribute("maxlength", HtmlTextWriterAttribute.Maxlength, false);
	HtmlTextWriter.RegisterAttribute("multiple", HtmlTextWriterAttribute.Multiple, false);
	HtmlTextWriter.RegisterAttribute("name", HtmlTextWriterAttribute.Name, false);
	HtmlTextWriter.RegisterAttribute("nowrap", HtmlTextWriterAttribute.Nowrap, false);
	HtmlTextWriter.RegisterAttribute("onchange", HtmlTextWriterAttribute.Onchange, true);
	HtmlTextWriter.RegisterAttribute("onclick", HtmlTextWriterAttribute.Onclick, true);
	HtmlTextWriter.RegisterAttribute("readonly", HtmlTextWriterAttribute.ReadOnly, false);
	HtmlTextWriter.RegisterAttribute("rows", HtmlTextWriterAttribute.Rows, false);
	HtmlTextWriter.RegisterAttribute("rowspan", HtmlTextWriterAttribute.Rowspan, false);
	HtmlTextWriter.RegisterAttribute("rules", HtmlTextWriterAttribute.Rules, false);
	HtmlTextWriter.RegisterAttribute("selected", HtmlTextWriterAttribute.Selected, false);
	HtmlTextWriter.RegisterAttribute("size", HtmlTextWriterAttribute.Size, false);
	HtmlTextWriter.RegisterAttribute("src", HtmlTextWriterAttribute.Src, true);
	HtmlTextWriter.RegisterAttribute("style", HtmlTextWriterAttribute.Style, false);
	HtmlTextWriter.RegisterAttribute("tabindex", HtmlTextWriterAttribute.Tabindex, false);
	HtmlTextWriter.RegisterAttribute("target", HtmlTextWriterAttribute.Target, false);
	HtmlTextWriter.RegisterAttribute("title", HtmlTextWriterAttribute.Title, true);
	HtmlTextWriter.RegisterAttribute("type", HtmlTextWriterAttribute.Type, false);
	HtmlTextWriter.RegisterAttribute("valign", HtmlTextWriterAttribute.Valign, false);
	HtmlTextWriter.RegisterAttribute("value", HtmlTextWriterAttribute.Value, true);
	HtmlTextWriter.RegisterAttribute("width", HtmlTextWriterAttribute.Width, false);
	HtmlTextWriter.RegisterAttribute("wrap", HtmlTextWriterAttribute.Wrap, false);

#if NET_2_0
	HtmlTextWriter._styleNameLookupArray = new String[42];
#else
	HtmlTextWriter._styleNameLookupArray = new String[14];
#endif
	HtmlTextWriter._styleKeyLookupTable = new Hashtable (HtmlTextWriter._styleNameLookupArray.Length,
								CaseInsensitiveHashCodeProvider.Default, CaseInsensitiveComparer.Default);
	HtmlTextWriter.RegisterStyle("background-color", HtmlTextWriterStyle.BackgroundColor);
	HtmlTextWriter.RegisterStyle("background-image", HtmlTextWriterStyle.BackgroundImage);
	HtmlTextWriter.RegisterStyle("border-collapse", HtmlTextWriterStyle.BorderCollapse);
	HtmlTextWriter.RegisterStyle("border-color", HtmlTextWriterStyle.BorderColor);
	HtmlTextWriter.RegisterStyle("border-style", HtmlTextWriterStyle.BorderStyle);
	HtmlTextWriter.RegisterStyle("border-width", HtmlTextWriterStyle.BorderWidth);
	HtmlTextWriter.RegisterStyle("color", HtmlTextWriterStyle.Color);
	HtmlTextWriter.RegisterStyle("font-family", HtmlTextWriterStyle.FontFamily);
	HtmlTextWriter.RegisterStyle("font-size", HtmlTextWriterStyle.FontSize);
	HtmlTextWriter.RegisterStyle("font-style", HtmlTextWriterStyle.FontStyle);
	HtmlTextWriter.RegisterStyle("font-weight", HtmlTextWriterStyle.FontWeight);
	HtmlTextWriter.RegisterStyle("height", HtmlTextWriterStyle.Height);
	HtmlTextWriter.RegisterStyle("text-decoration", HtmlTextWriterStyle.TextDecoration);
	HtmlTextWriter.RegisterStyle("width", HtmlTextWriterStyle.Width);
#if NET_2_0
	HtmlTextWriter.RegisterStyle("list-style-image", HtmlTextWriterStyle.ListStyleImage);
	HtmlTextWriter.RegisterStyle("list-style-type", HtmlTextWriterStyle.ListStyleType);
	HtmlTextWriter.RegisterStyle("cursor", HtmlTextWriterStyle.Cursor);
	HtmlTextWriter.RegisterStyle("direction", HtmlTextWriterStyle.Direction);
	HtmlTextWriter.RegisterStyle("display", HtmlTextWriterStyle.Display);
	HtmlTextWriter.RegisterStyle("filter", HtmlTextWriterStyle.Filter);
	HtmlTextWriter.RegisterStyle("font-variant", HtmlTextWriterStyle.FontVariant);
	HtmlTextWriter.RegisterStyle("left", HtmlTextWriterStyle.Left);
	HtmlTextWriter.RegisterStyle("margin", HtmlTextWriterStyle.Margin);
	HtmlTextWriter.RegisterStyle("margin-bottom", HtmlTextWriterStyle.MarginBottom);
	HtmlTextWriter.RegisterStyle("margin-left", HtmlTextWriterStyle.MarginLeft);
	HtmlTextWriter.RegisterStyle("margin-right", HtmlTextWriterStyle.MarginRight);
	HtmlTextWriter.RegisterStyle("margin-top", HtmlTextWriterStyle.MarginTop);
	HtmlTextWriter.RegisterStyle("overflow", HtmlTextWriterStyle.Overflow);
	HtmlTextWriter.RegisterStyle("overflow-x", HtmlTextWriterStyle.OverflowX);
	HtmlTextWriter.RegisterStyle("overflow-y", HtmlTextWriterStyle.OverflowY);
	HtmlTextWriter.RegisterStyle("padding", HtmlTextWriterStyle.Padding);
	HtmlTextWriter.RegisterStyle("padding-bottom", HtmlTextWriterStyle.PaddingBottom);
	HtmlTextWriter.RegisterStyle("padding-left", HtmlTextWriterStyle.PaddingLeft);
	HtmlTextWriter.RegisterStyle("padding-right", HtmlTextWriterStyle.PaddingRight);
	HtmlTextWriter.RegisterStyle("padding-top", HtmlTextWriterStyle.PaddingTop);
	HtmlTextWriter.RegisterStyle("position", HtmlTextWriterStyle.Position);
	HtmlTextWriter.RegisterStyle("text-align", HtmlTextWriterStyle.TextAlign);
	HtmlTextWriter.RegisterStyle("text-overflow", HtmlTextWriterStyle.TextOverflow);
	HtmlTextWriter.RegisterStyle("top", HtmlTextWriterStyle.Top);
	HtmlTextWriter.RegisterStyle("visibility", HtmlTextWriterStyle.Visibility);
	HtmlTextWriter.RegisterStyle("white-space", HtmlTextWriterStyle.WhiteSpace);
	HtmlTextWriter.RegisterStyle("z-index", HtmlTextWriterStyle.ZIndex);
#endif
}

public HtmlTextWriter(TextWriter writer):this(writer, "	"){}

public HtmlTextWriter(TextWriter writer, string tabString) : base() {
	this.writer = writer;
	this.tabString = tabString;
	indentLevel = 0;
	tabsPending = false;
	_httpWriter = writer as HttpWriter;
	_isDescendant = GetType() == typeof(HtmlTextWriter) == false;
	_attrList = new RenderAttribute[20];
	_attrCount = 0;
	_styleList = new RenderStyle[20];
	_styleCount = 0;
	_endTags = new TagStackEntry[16];
	_endTagCount = 0;
	_inlineCount = 0;
}

public virtual void AddAttribute(HtmlTextWriterAttribute key, string value){
	if ((int) key >= 0 && (int) key < HtmlTextWriter._attrNameLookupArray.Length) {
		AttributeInformation attrInfo = HtmlTextWriter._attrNameLookupArray[(int) key];
		AddAttribute(attrInfo.name, value, key, attrInfo.encode);
	}
}

public virtual void AddAttribute(HtmlTextWriterAttribute key, string value, bool fEncode){
	if ((int) key >= 0 && (int) key < HtmlTextWriter._attrNameLookupArray.Length) {
		AddAttribute(HtmlTextWriter._attrNameLookupArray[(int) key].name, value, key, fEncode);
	}
}

public virtual void AddAttribute(string name, string value){
	HtmlTextWriterAttribute attr = GetAttributeKey(name);
	value = EncodeAttributeValue(GetAttributeKey(name), value);
	AddAttribute(name, value, attr);
}

public virtual void AddAttribute(string name, string value, bool fEndode){
	value = EncodeAttributeValue(value, fEndode);
	AddAttribute(name, value, GetAttributeKey(name));
}

protected virtual void AddAttribute(string name, string value, HtmlTextWriterAttribute key){
	AddAttribute(name, value, key, false);
}

private void AddAttribute(string name, string value, HtmlTextWriterAttribute key, bool encode){
	if (_attrCount >= (int) _attrList.Length) {
		RenderAttribute[] rAttrArr = new RenderAttribute[_attrList.Length * 2];
		System.Array.Copy(_attrList, rAttrArr, (int) _attrList.Length);
		_attrList = rAttrArr;
	}
	RenderAttribute rAttr;
	rAttr.name = name;
	rAttr.value = value;
	rAttr.key = key;
	rAttr.encode = encode;
	_attrList [_attrCount++] = rAttr;
}

public virtual void AddStyleAttribute(HtmlTextWriterStyle key, string value){
	AddStyleAttribute(GetStyleName(key), value, key);
}

public virtual void AddStyleAttribute(string name, string value){
	AddStyleAttribute(name, value, GetStyleKey(name));
}

protected virtual void AddStyleAttribute(string name, string value, HtmlTextWriterStyle key){
	if (_styleCount >= (int) _styleList.Length) {
		RenderStyle[] rAttrArr = new RenderStyle[_styleList.Length * 2];
		System.Array.Copy(_styleList, rAttrArr, (int) _styleList.Length);
		_styleList = rAttrArr;
	}
	RenderStyle rAttr;
	rAttr.name = name;
	rAttr.value = value;
	rAttr.key = key;
	_styleList [_styleCount++] = rAttr;
}

public override void Close(){
	writer.Close();
}

protected virtual string EncodeAttributeValue(HtmlTextWriterAttribute attrKey, string value){
	bool valid = true;
	if (0 <= (int) attrKey && (int) attrKey < HtmlTextWriter._attrNameLookupArray.Length)
		valid = HtmlTextWriter._attrNameLookupArray[(int) attrKey].encode;
	return EncodeAttributeValue(value, valid);
}

protected string EncodeAttributeValue(string value, bool fEncode){
	if (value == null)
		return null;
	if (!(fEncode))
		return value;
	return System.Web.HttpUtility.HtmlAttributeEncode(value);
}

protected string EncodeUrl(string url){
	if (url.IndexOf(SpaceChar) < 0)
		return url;
	System.Text.StringBuilder sb = new System.Text.StringBuilder();
	for(int i=0; i <= url.Length; i++){
		char temp = url[i];
		if (temp != 32)
			sb.Append(temp);
		else
			sb.Append("%20");
	}
	return sb.ToString();
}

protected virtual void FilterAttributes(){
	int count = 0;
	for(int i=0; i < _styleCount; i++){
		RenderStyle rStyle = _styleList[i];
		if (OnStyleAttributeRender(rStyle.name, rStyle.value, rStyle.key)) {
			count++;
		}
	}
	_styleCount = count;
	count = 0;
	for(int i=0; i <= _attrCount; i++){
		RenderAttribute rAttr = _attrList[i];
		if (OnAttributeRender(rAttr.name, rAttr.value, rAttr.key)) {
			count++;
		}
	}
	_attrCount = count;
}

public override void Flush(){
	writer.Flush();
}

protected HtmlTextWriterAttribute GetAttributeKey(string attrName){
	if (attrName != null && attrName.Length > 0) {
		object attr = HtmlTextWriter._attrKeyLookupTable[attrName];
		if (attr != null)
			return (HtmlTextWriterAttribute) attr;
	}
	return (HtmlTextWriterAttribute) (-1);
}

protected string GetAttributeName(HtmlTextWriterAttribute attrKey){
	if ((int) attrKey >= 0 && (int) attrKey < HtmlTextWriter._attrNameLookupArray.Length)
		return HtmlTextWriter._attrNameLookupArray[(int) attrKey].name;
	return System.String.Empty;
}

protected HtmlTextWriterStyle GetStyleKey(string styleName){
	if (styleName != null && styleName.Length > 0) {
		object style = HtmlTextWriter._styleKeyLookupTable[styleName];
		if (style != null)
			return (HtmlTextWriterStyle) style;
	}
	return (HtmlTextWriterStyle) (-1);
}

protected string GetStyleName(HtmlTextWriterStyle styleKey){
	return StaticGetStyleName (styleKey);
}

internal static string StaticGetStyleName (HtmlTextWriterStyle styleKey){
	if ((int) styleKey >= 0 && (int) styleKey < HtmlTextWriter._styleNameLookupArray.Length)
		return HtmlTextWriter._styleNameLookupArray[(int) styleKey];
	return System.String.Empty;
}

protected virtual HtmlTextWriterTag GetTagKey(string tagName){
	if (tagName != null && tagName.Length > 0) {
		object tag = HtmlTextWriter._tagKeyLookupTable[tagName];
		if (tag != null)
			return (HtmlTextWriterTag) tag;
	}
	return 0;
}

protected virtual string GetTagName(HtmlTextWriterTag tagKey){
	if ((int) tagKey >= 0 && (int) tagKey < HtmlTextWriter._tagNameLookupArray.Length)
		return HtmlTextWriter._tagNameLookupArray[(int) tagKey].name;
	return System.String.Empty;
}

protected bool IsAttributeDefined(HtmlTextWriterAttribute key){
	for (int i=0; i < _attrCount; i++) {
		if (_attrList[i].key == key)
			return true;
	}
	return false;
}

protected bool IsAttributeDefined(HtmlTextWriterAttribute key, out string value){
	value = null;
	for (int i=0; i < _attrCount; i++) {
		if (_attrList[i].key == key) {
			value = _attrList[i].value;
			return true;
		}
	}
	return false;
}

protected bool IsStyleAttributeDefined(HtmlTextWriterStyle key){
	for (int i= 0; i < _styleCount; i++) {
		if (_styleList[i].key == key)
			return true;
	}
	return false;
}

protected bool IsStyleAttributeDefined(HtmlTextWriterStyle key, out string value){
	value = null;
	for( int i=0; i < _styleCount; i++) {
		if (_styleList[i].key == key) {
			value = _styleList[i].value;
			return true;
		}
	}
	return false;
}

protected virtual bool OnAttributeRender(string name, string value, HtmlTextWriterAttribute key){
	return true;
}

protected virtual bool OnStyleAttributeRender(string name, string value, HtmlTextWriterStyle key){
	return true;
}

protected virtual bool OnTagRender(string name, HtmlTextWriterTag key){
	return true;
}

protected virtual void OutputTabs(){
	if (tabsPending) {
		for(int i=0; i < indentLevel; i++) {
			writer.Write(tabString);
		}
		tabsPending = false;
	}
}

protected string PopEndTag(){
	if (_endTagCount <= 0)
		throw new InvalidOperationException("A PopEndTag was called without a corresponding PushEndTag");
	_endTagCount--;
	TagKey = _endTags[_endTagCount].tagKey;
	return _endTags[_endTagCount].endTagText;
}

protected void PushEndTag(string endTag){
	if (_endTagCount >= (int) _endTags.Length) {
		TagStackEntry[] temp = new TagStackEntry[(int) _endTags.Length * 2];
		System.Array.Copy(_endTags, temp, (int) _endTags.Length);
		_endTags = temp;
	}
	_endTags[_endTagCount].tagKey = _tagKey;
	_endTags[_endTagCount].endTagText = endTag;
	_endTagCount++;
}

protected static void RegisterAttribute(string name, HtmlTextWriterAttribute key){
	HtmlTextWriter.RegisterAttribute(name, key, false);
}

private static void RegisterAttribute(string name, HtmlTextWriterAttribute key, bool fEncode){
	name = name.ToLower();
	HtmlTextWriter._attrKeyLookupTable.Add(name, key);
	if ((int) key < (int) HtmlTextWriter._attrNameLookupArray.Length)
		HtmlTextWriter._attrNameLookupArray[(int) key] = new AttributeInformation(name, fEncode);
}

protected static void RegisterStyle(string name, HtmlTextWriterStyle key){
	name = name.ToLower();
	HtmlTextWriter._styleKeyLookupTable.Add(name, key);
	if ((int) key < (int) HtmlTextWriter._styleNameLookupArray.Length)
		HtmlTextWriter._styleNameLookupArray[(int) key] = name;
}

protected static void RegisterTag(string name, HtmlTextWriterTag key){
	HtmlTextWriter.RegisterTag(name, key, TagType.Other);
}

private static void RegisterTag(string name, HtmlTextWriterTag key, TagType type){
	name = name.ToLower();
	HtmlTextWriter._tagKeyLookupTable.Add(name, key);
	string fullTag = null;
	if ((int) type != 1 && (int) key != 0) {
		fullTag = EndTagLeftChars + name + TagRightChar;
	}
	if ((int) key < HtmlTextWriter._tagNameLookupArray.Length)
		HtmlTextWriter._tagNameLookupArray[(int) key] = new TagInformation(name, type, fullTag);
}

protected virtual string RenderAfterContent(){
	return null;
}

protected virtual string RenderAfterTag(){
	return null;
}

protected virtual string RenderBeforeContent(){
	return null;
}

protected virtual string RenderBeforeTag(){
	return null;
}

public virtual void RenderBeginTag(HtmlTextWriterTag tagKey){
	TagKey = tagKey;
	bool tagRendered = true;
	bool tagRender = true;
	if (_isDescendant) {
		tagRender = OnTagRender(_tagName, _tagKey);
		FilterAttributes();
		string beforeTag = RenderBeforeTag();
		if (beforeTag != null) {
			if (tabsPending)
				OutputTabs();
			writer.Write(beforeTag);
		}
	}
	TagInformation currentTag = HtmlTextWriter._tagNameLookupArray[_tagIndex];
	if (currentTag.closingTag == null && currentTag.tagType == TagType.Other) {
		currentTag.closingTag = EndTagLeftChars + _tagName + TagRightChar;
	}

	if (tagRender) {
		tagRendered = false;
		if (tabsPending)
			OutputTabs();
		writer.Write(TagLeftChar);
		writer.Write(_tagName);
		RenderAttribute rAttr;
		string rAttrValue = null;
		for (int i=0; i < _attrCount; i++) {
			rAttr = _attrList[i];
			if (rAttr.key == HtmlTextWriterAttribute.Style)
				rAttrValue = rAttr.value;
			else {
				writer.Write(SpaceChar);
				writer.Write(rAttr.name);
				if (rAttr.value != null) {
					writer.Write(EqualsChar);
					writer.Write(DoubleQuoteChar);
					if (rAttr.encode) {
						if (_httpWriter == null) {
							System.Web.HttpUtility.HtmlAttributeEncode(rAttr.value, writer);
						}
						else {
							System.Web.HttpUtility.HtmlAttributeEncode(rAttr.value, (TextWriter) _httpWriter);
						}
					}
					else {
						writer.Write(rAttr.value);
					}
					writer.Write(DoubleQuoteChar);
				}
			}
		}
		if (_styleCount > 0 || rAttrValue != null) {
			writer.Write(SpaceChar);
			writer.Write("style");
			writer.Write(EqualsChar);
			writer.Write(DoubleQuoteChar);
			RenderStyle rStyle;
			for (int i=0; i < _styleCount; i++) {
				rStyle = _styleList[i];
				writer.Write(rStyle.name);
				writer.Write(StyleEqualsChar);
				writer.Write(rStyle.value);
				writer.Write(SemicolonChar);
			}
			if (rAttrValue != null)
				writer.Write(rAttrValue);
			writer.Write(DoubleQuoteChar);
		}
		if (currentTag.tagType == TagType.NonClosing) {
			writer.Write(SpaceChar);
			writer.Write(SlashChar);
			writer.Write(TagRightChar);
		}
		else
			writer.Write(TagRightChar);
	}
	string beforeContent = RenderBeforeContent();
	if (beforeContent != null) {
		if (tabsPending)
			OutputTabs();
		writer.Write(beforeContent);
	}
	if (tagRendered) {
		if (currentTag.tagType == TagType.Inline)
			_inlineCount++;
		else {
			WriteLine();
			Indent++;
		}
		if (currentTag.closingTag == null) {
			currentTag.closingTag = EndTagLeftChars + _tagName + TagRightChar;
		}
	}
	if (_isDescendant) {
		string afterContent = RenderAfterContent();
		if (afterContent != null) {
			if (currentTag.closingTag != null)
				currentTag.closingTag = afterContent;
		}
		string afterTag = RenderAfterTag();
		if (afterTag != null) {
			if (currentTag.closingTag != null)
				currentTag.closingTag = afterTag;
		}
	}
	PushEndTag(currentTag.closingTag);
	_attrCount = 0;
	_styleCount = 0;
}

public virtual void RenderBeginTag(string tagName){
	TagName = tagName;
	RenderBeginTag(_tagKey);
}

public virtual void RenderEndTag(){
	string endTagText = PopEndTag();
	if (endTagText != null) {
		if (HtmlTextWriter._tagNameLookupArray[_tagIndex].tagType == 0) {
			_inlineCount--;
			Write(endTagText);
		}
		else{
			WriteLine();
			Indent--;
			Write(endTagText);
		}
	}
}

public override void Write(bool value){
	if (tabsPending)
		OutputTabs();
	writer.Write(value);
}

public override void Write(char value){
	if (tabsPending)
		OutputTabs();
	writer.Write(value);
}

public override void Write(char[] buffer){
	if (tabsPending)
		OutputTabs();
	writer.Write(buffer);
}

public override void Write(char[] buffer, int index, int count){
	if (tabsPending)
		OutputTabs();
	writer.Write(buffer, index, count);
}

public override void Write(double value){
	if (tabsPending)
		OutputTabs();
	writer.Write(value);
}

public override void Write(int value){
	if (tabsPending)
		OutputTabs();
	writer.Write(value);
}

public override void Write(long value){
	if (tabsPending)
		OutputTabs();
	writer.Write(value);
}

public override void Write(object value){
	if (tabsPending)
		OutputTabs();
	writer.Write(value);
}

public override void Write(float value){
	if (tabsPending)
		OutputTabs();
	writer.Write(value);
}

public override void Write(string s){
	if (tabsPending)
		OutputTabs();
	writer.Write(s);
}

public override void Write(string format, object arg0){
	if (tabsPending)
		OutputTabs();
	writer.Write(format, arg0);
}

public override void Write(string format, object arg0, object arg1){
	if (tabsPending)
		OutputTabs();
	writer.Write(format, arg0, arg1);
}

public override void Write(string format, params object[] arg){
	if (tabsPending)
		OutputTabs();
	writer.Write(format, arg);
}

public virtual void WriteAttribute(string name, string value){
	WriteAttribute(name, value, false);
}

public virtual void WriteAttribute(string name, string value, bool fEncode){
	writer.Write(SpaceChar);
	writer.Write(name);
	if (value != null) {
		writer.Write(EqualsChar);
		writer.Write(DoubleQuoteChar);
		if (fEncode) {
			if (_httpWriter == null) {
				System.Web.HttpUtility.HtmlAttributeEncode(value, writer);
			}
			else{
				System.Web.HttpUtility.HtmlAttributeEncode(value, (TextWriter) _httpWriter);
			}
		}
		else{
			writer.Write(value);
		}
		writer.Write(DoubleQuoteChar);
	}
}

public virtual void WriteBeginTag(string tagName){
	if (tabsPending)
		OutputTabs();
	writer.Write(TagLeftChar);
	writer.Write(tagName);
}

public virtual void WriteEndTag(string tagName){
	if (tabsPending)
		OutputTabs();
	writer.Write(TagLeftChar);
	writer.Write(SlashChar);
	writer.Write(tagName);
	writer.Write(TagRightChar);
}

public virtual void WriteFullBeginTag(string tagName){
	if (tabsPending)
		OutputTabs();
	writer.Write(TagLeftChar);
	writer.Write(tagName);
	writer.Write(TagRightChar);
}

public override void WriteLine(){
	writer.WriteLine();
	tabsPending = true;
}

public override void WriteLine(bool value){
	if (tabsPending)
		OutputTabs();
	writer.WriteLine(value);
	tabsPending = true;
}

public override void WriteLine(char value){
	if (tabsPending)
		OutputTabs();
	writer.WriteLine(value);
	tabsPending = true;
}

public override void WriteLine(char[] buffer){
	if (tabsPending)
		OutputTabs();
	writer.WriteLine(buffer);
	tabsPending = true;
}

public override void WriteLine(char[] buffer, int index, int count){
	if (tabsPending)
		OutputTabs();
	writer.WriteLine(buffer, index, count);
	tabsPending = true;
}

public override void WriteLine(double value){
	if (tabsPending)
		OutputTabs();
	writer.WriteLine(value);
	tabsPending = true;
}

public override void WriteLine(int value){
	if (tabsPending)
		OutputTabs();
	writer.WriteLine(value);
	tabsPending = true;
}

public override void WriteLine(long value){
	if (tabsPending)
		OutputTabs();
	writer.WriteLine(value);
	tabsPending = true;
}

public override void WriteLine(object value){
	if (tabsPending)
		OutputTabs();
	writer.WriteLine(value);
	tabsPending = true;
}

public override void WriteLine(float value){
	if (tabsPending)
		OutputTabs();
	writer.WriteLine(value);
	tabsPending = true;
}

public override void WriteLine(string s){
	if (tabsPending)
		OutputTabs();
	writer.WriteLine(s);
	tabsPending = true;
}

public override void WriteLine(string format, object arg0){
	if (tabsPending)
		OutputTabs();
	writer.WriteLine(format, arg0);
	tabsPending = true;
}

public override void WriteLine(string format, object arg0, object arg1){
	if (tabsPending)
		OutputTabs();
	writer.WriteLine(format, arg0, arg1);
	tabsPending = true;
}

public override void WriteLine(string format, params object[] arg){
	if (tabsPending)
		OutputTabs();
	writer.WriteLine(format, arg);
	tabsPending = true;
}

[CLSCompliant(false)]
public override void WriteLine(uint value){
	if (tabsPending)
		OutputTabs();
	writer.WriteLine(value);
	tabsPending = true;
}

public void WriteLineNoTabs(string s){
	writer.WriteLine(s);
}

public virtual void WriteStyleAttribute(string name, string value){
	WriteStyleAttribute(name, value, false);
}

public virtual void WriteStyleAttribute(string name, string value, bool fEncode){
	writer.Write(name);
	writer.Write(StyleEqualsChar);
	if (fEncode) {
		if (_httpWriter == null) {
			System.Web.HttpUtility.HtmlAttributeEncode(value, writer);
		}
		else{
			System.Web.HttpUtility.HtmlAttributeEncode(value, (TextWriter) _httpWriter);
		}
	}
	else {
		writer.Write(value);
	}
	writer.Write(SemicolonChar);
}

public override System.Text.Encoding Encoding { 
	get{
		return writer.Encoding;
	}
}

public int Indent { 
	get{
		return indentLevel;
	}
	set{
		if (value < 0)
			value = 0;
		indentLevel = value;
	}
}

public TextWriter InnerWriter { 
	get{
		return writer;
	}
	set{
		writer = value;
		_httpWriter = value as HttpWriter;
	}
}

public override string NewLine { 
	get{
		return writer.NewLine;
	}
	set{
		writer.NewLine = value;
	}
}

protected HtmlTextWriterTag TagKey { 
	get{
		return _tagKey;
	}
	set{
		_tagIndex = (int) value;
		if (_tagIndex < 0 || _tagIndex >= (int) HtmlTextWriter._tagNameLookupArray.Length)
			throw new ArgumentOutOfRangeException("value");
		_tagKey = value;
		if (value != 0)
			_tagName = HtmlTextWriter._tagNameLookupArray[_tagIndex].name;
	}
}

protected string TagName { 
	get{
		return _tagName;
	}
	set{
		_tagName = value;
		_tagKey = GetTagKey(_tagName);
		_tagIndex = (int) _tagKey;
	}
}

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
public const char SpaceChar = ' ';
public const char StyleEqualsChar = ':';
public const char TagLeftChar = '<';
public const char TagRightChar = '>';

private int _attrCount;
private int _endTagCount;
private int _styleCount;
private int indentLevel;
private int _inlineCount;
private int _tagIndex;

private bool _isDescendant;
private bool tabsPending;

private HtmlTextWriterTag _tagKey;
private TextWriter writer;
private HttpWriter _httpWriter;

private static Hashtable _attrKeyLookupTable;
private static Hashtable _styleKeyLookupTable;
private static Hashtable _tagKeyLookupTable;

private string _tagName;
private string tabString;
private static string[] _styleNameLookupArray;

private RenderAttribute[] _attrList;
private static AttributeInformation[] _attrNameLookupArray;
private static TagInformation[] _tagNameLookupArray;
private TagStackEntry[] _endTags;
private RenderStyle[] _styleList;

} //HtmlTextWriter

struct AttributeInformation {
	public bool encode;
	public string name;

	public AttributeInformation(string name, bool encode){
		this.encode = encode;
		this.name = name;
	}
} 
 
struct RenderAttribute {
	public bool encode;
	public HtmlTextWriterAttribute key;
	public string name;
	public string value;
} 
 
struct RenderStyle {
	public HtmlTextWriterStyle key;
	public string name;
	public string value;
} 
 
struct TagInformation {
	public string closingTag;
	public string name;
	public TagType tagType;

	public TagInformation(string name, TagType tagType, string closingTag){
		this.name = name;
		this.tagType = tagType;
		this.closingTag = closingTag;	
	}
} 
 
struct TagStackEntry {
	public string endTagText;
	public HtmlTextWriterTag tagKey;
} 
 
enum TagType {
	Inline,
	NonClosing,
	Other
} 


} // namespace System.Web.UI.HtmlControls


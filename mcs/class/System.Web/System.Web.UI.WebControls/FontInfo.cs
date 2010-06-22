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
// Copyright (c) 2005-2010 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Dennis Bartok	(pbartok@novell.com)
//
//

using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.WebControls
{
	// CAS
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
	public sealed class FontInfo 
	{
		#region Fields
		static string[]	empty_names = new string[0];
		StateBag bag;
		Style _owner;
		#endregion	// Fields

		#region Constructors
		internal FontInfo(Style owner) 
		{
			_owner = owner;
			this.bag = owner.ViewState;
		}
		#endregion	// Constructors

		#region Public Instance Properties
		[DefaultValue(false)]
		[NotifyParentProperty(true)]
		[WebSysDescription ("")]
		[WebCategory ("Font")]
		public bool Bold {
			get  {
				if (!_owner.CheckBit((int)Style.Styles.FontBold))
					return false;

				return bag.GetBool("Font_Bold", false);
			}

			set {
				bag["Font_Bold"] = value;
				_owner.SetBit ((int) Style.Styles.FontBold);
			}
		}

		[DefaultValue(false)]
		[NotifyParentProperty(true)]
		[WebSysDescription ("")]
		[WebCategory ("Font")]
		public bool Italic  {
			get {
				if (!_owner.CheckBit ((int) Style.Styles.FontItalic))
					return false;

				return bag.GetBool("Font_Italic", false);
			}

			set {
				bag["Font_Italic"] = value;
				_owner.SetBit ((int) Style.Styles.FontItalic);
			}
		}

		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue("")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Editor("System.Drawing.Design.FontNameEditor, " + Consts.AssemblySystem_Drawing_Design, typeof(System.Drawing.Design.UITypeEditor))]
		[NotifyParentProperty(true)]
		[TypeConverter (typeof(System.Drawing.FontConverter.FontNameConverter))]
		[WebSysDescription ("")]
		[WebCategory ("Font")]
		public string Name {
			get {
				string [] names = Names;

				if (names.Length == 0)
					return string.Empty;

				return names[0];
			}

			set {
				// Seems to be a special case in MS, removing the names from the bag when Name is set to empty, 
				// but not when setting Names to an empty array
				if (value == string.Empty) {
					Names = null;
					return;
				}

				if (value == null)
					throw new ArgumentNullException("value", "Font name cannot be null");
				Names = new string[1] { value };
			}
		}

		[RefreshProperties (RefreshProperties.Repaint)]
		[Editor("System.Windows.Forms.Design.StringArrayEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		[NotifyParentProperty(true)]
		[TypeConverter(typeof(System.Web.UI.WebControls.FontNamesConverter))]
		[WebSysDescription ("")]
		[WebCategory ("Font")]
		public string[] Names {
			get  {
				string[] ret;

				if (!_owner.CheckBit ((int) Style.Styles.FontNames))
					return FontInfo.empty_names;

				ret = (string[])bag["Font_Names"];

				if (ret != null)
					return ret;
				
				return FontInfo.empty_names;
			}

			set {
				if (value == null) {
					bag.Remove ("Font_Names");
					_owner.RemoveBit ((int) Style.Styles.FontNames);
				} else {
					bag ["Font_Names"] = value;
					_owner.SetBit ((int) Style.Styles.FontNames);
				}
			}
		}

		[DefaultValue(false)]
		[NotifyParentProperty(true)]
		[WebSysDescription ("")]
		[WebCategory ("Font")]
		public bool Overline {
			get {
				if (!_owner.CheckBit ((int) Style.Styles.FontOverline)) 
					return false;

				return bag.GetBool("Font_Overline", false);
			}

			set {
				bag["Font_Overline"] = value;
				_owner.SetBit ((int) Style.Styles.FontOverline);
			}
		}

		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue(typeof (FontUnit), "")]
		[NotifyParentProperty(true)]
		[WebSysDescription ("")]
		[WebCategory ("Font")]
		public FontUnit Size {
			get {
				if (!_owner.CheckBit ((int) Style.Styles.FontSize)) 
					return FontUnit.Empty;

				return (FontUnit)bag["Font_Size"];
			}

			set {
				if (value.Unit.Value < 0)
					throw new ArgumentOutOfRangeException("Value", value.Unit.Value, "Font size cannot be negative");
				
				bag["Font_Size"] = value;
				_owner.SetBit ((int) Style.Styles.FontSize);
			}
		}

		[DefaultValue(false)]
		[NotifyParentProperty(true)]
		[WebSysDescription ("")]
		[WebCategory ("Font")]
		public bool Strikeout {
			get  {
				if (!_owner.CheckBit ((int) Style.Styles.FontStrikeout))
					return false;

				return bag.GetBool("Font_Strikeout", false);
			}

			set {
				bag["Font_Strikeout"] = value;
				_owner.SetBit ((int) Style.Styles.FontStrikeout);
			}
		}

		[DefaultValue(false)]
		[NotifyParentProperty(true)]
		[WebSysDescription ("")]
		[WebCategory ("Font")]
		public bool Underline {
			get  {
				if (!_owner.CheckBit ((int) Style.Styles.FontUnderline))
					return false;

				return bag.GetBool("Font_Underline", false);
			}

			set {
				bag["Font_Underline"] = value;
				_owner.SetBit ((int) Style.Styles.FontUnderline);
			}
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		public void CopyFrom (FontInfo f) 
		{
			if (f == null || f.IsEmpty)
				return;

			if (f == this)
				return;

			// MS stores the property in the bag if it's value is false
			if (f._owner.CheckBit((int) Style.Styles.FontBold))
				this.Bold = f.Bold;

			if (f._owner.CheckBit ((int) Style.Styles.FontItalic))
				this.Italic = f.Italic;

			// MS seems to have some weird behaviour, even if f's Name has been set to String.Empty we still get an empty array
			this.Names = f.Names;

			if (f._owner.CheckBit ((int) Style.Styles.FontOverline))
				this.Overline = f.Overline;

			if (f._owner.CheckBit ((int) Style.Styles.FontSize))
				this.Size = f.Size;

			if (f._owner.CheckBit ((int) Style.Styles.FontStrikeout))
				this.Strikeout = f.Strikeout;

			if (f._owner.CheckBit ((int) Style.Styles.FontUnderline))
				this.Underline = f.Underline;
		}

		public void MergeWith (FontInfo f) 
		{
			if (!_owner.CheckBit ((int) Style.Styles.FontBold) && f._owner.CheckBit ((int) Style.Styles.FontBold))
				this.Bold = f.Bold;

			if (!_owner.CheckBit ((int) Style.Styles.FontItalic) && f._owner.CheckBit ((int) Style.Styles.FontItalic))
				this.Italic = f.Italic;

			if (!_owner.CheckBit ((int) Style.Styles.FontNames) && f._owner.CheckBit ((int) Style.Styles.FontNames))
				this.Names = f.Names;

			if (!_owner.CheckBit ((int) Style.Styles.FontOverline) && f._owner.CheckBit ((int) Style.Styles.FontOverline))
				this.Overline = f.Overline;

			if (!_owner.CheckBit ((int) Style.Styles.FontSize) && f._owner.CheckBit ((int) Style.Styles.FontSize))
				this.Size = f.Size;

			if (!_owner.CheckBit ((int) Style.Styles.FontStrikeout) && f._owner.CheckBit ((int) Style.Styles.FontStrikeout))
				this.Strikeout = f.Strikeout;

			if (!_owner.CheckBit ((int) Style.Styles.FontUnderline) && f._owner.CheckBit ((int) Style.Styles.FontUnderline))
				this.Underline = f.Underline;
		}

		public bool ShouldSerializeNames () 
		{
			return (Names.Length != 0);
		}

		public override string ToString () 
		{
			if (this.Names.Length == 0)
				return this.Size.ToString();

			return this.Name + ", " + this.Size.ToString();
		}

		public void ClearDefaults ()
		{
			Reset ();
		}

		#endregion	// Public Instance Methods

		#region Private Methods
		internal void Reset() 
		{
			bag.Remove("Font_Bold");
			bag.Remove("Font_Italic");
			bag.Remove("Font_Names");
			bag.Remove("Font_Overline");
			bag.Remove("Font_Size");
			bag.Remove("Font_Strikeout");
			bag.Remove("Font_Underline");
			_owner.RemoveBit ((int) Style.Styles.FontAll);
		}

		internal void FillStyleAttributes (CssStyleCollection attributes, bool alwaysRenderTextDecoration)
		{
			if (IsEmpty) {
				if(alwaysRenderTextDecoration)
					attributes.Add (HtmlTextWriterStyle.TextDecoration, "none");
				return;
			}

			string s;
			// Fonts are a bit weird
			s = String.Join (",", Names);
			if (s.Length > 0)
				attributes.Add (HtmlTextWriterStyle.FontFamily, s);

			if (_owner.CheckBit ((int) Style.Styles.FontBold))
				attributes.Add (HtmlTextWriterStyle.FontWeight, Bold ? "bold" : "normal");

			if (_owner.CheckBit ((int) Style.Styles.FontItalic))
				attributes.Add (HtmlTextWriterStyle.FontStyle, Italic ? "italic" : "normal");

			if (!Size.IsEmpty)
				attributes.Add (HtmlTextWriterStyle.FontSize, Size.ToString ());

			// These styles are munged into a attribute decoration
			s = String.Empty;
			bool hasTextDecoration = false;

			if (_owner.CheckBit ((int) Style.Styles.FontOverline)) {
				if (Overline)
					s += "overline ";
				hasTextDecoration = true;
			}

			if (_owner.CheckBit ((int) Style.Styles.FontStrikeout)) {
				if (Strikeout)
					s += "line-through ";
				hasTextDecoration = true;
			}

			if (_owner.CheckBit ((int) Style.Styles.FontUnderline)) {
				if (Underline)
					s += "underline ";
				hasTextDecoration = true;
			}

			s = (s.Length > 0) ? s.Trim () : (alwaysRenderTextDecoration || hasTextDecoration) ? "none" : String.Empty;
			if (s.Length > 0)
				attributes.Add (HtmlTextWriterStyle.TextDecoration, s);
		}
		#endregion	// Private Methods


		bool IsEmpty {
			get { return !_owner.CheckBit ((int) Style.Styles.FontAll); }
		}
	}
}

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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Dennis Bartok	(pbartok@novell.com)
//
//

using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
	public sealed class FontInfo 
	{
		[Flags]
		internal enum FontStyles 
		{
			None		= 0,
			Bold		= 0x0001,
			Italic		= 0x0002,
			Names		= 0x0004,
			Overline	= 0x0008,
			Size		= 0x0010,
			Strikeout	= 0x0020,
			Underline	= 0x0040
		}

		#region Fields
		private static string[]	empty_names = new string[0];
		private FontStyles	fontstyles;
		private StateBag	bag;
		#endregion	// Fields

		#region Constructors
		internal FontInfo(Style owner) 
		{
			this.bag = owner.ViewState;
		}
		#endregion	// Constructors

		#region Public Instance Properties
#if ONLY_1_1
		[Bindable(true)]
#endif
		[DefaultValue(false)]
		[NotifyParentProperty(true)]
		[WebSysDescription ("")]
		[WebCategory ("Font")]
		public bool Bold 
		{
			get 
			{
				if ((fontstyles & FontStyles.Bold) == 0) 
				{
					return false;
				}

				return bag.GetBool("Font_Bold", false);
			}

			set 
			{
				fontstyles |= FontStyles.Bold;
				bag["Font_Bold"] = value;
			}
		}

#if ONLY_1_1
		[Bindable(true)]
#endif
		[DefaultValue(false)]
		[NotifyParentProperty(true)]
		[WebSysDescription ("")]
		[WebCategory ("Font")]
		public bool Italic 
		{
			get 
			{
				if ((fontstyles & FontStyles.Italic) == 0) 
				{
					return false;
				}

				return bag.GetBool("Font_Italic", false);
			}

			set 
			{
				fontstyles |= FontStyles.Italic;
				bag["Font_Italic"] = value;
			}
		}

#if NET_2_0
		[RefreshProperties (RefreshProperties.Repaint)]
#else
		[Bindable(true)]
#endif
		[DefaultValue("")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Editor("System.Drawing.Design.FontNameEditor, " + Consts.AssemblySystem_Drawing_Design, typeof(System.Drawing.Design.UITypeEditor))]
		[NotifyParentProperty(true)]
		[TypeConverter (typeof(System.Drawing.FontConverter.FontNameConverter))]
		[WebSysDescription ("")]
		[WebCategory ("Font")]
		public string Name 
		{
			get 
			{
				string[] names;

				if ((fontstyles & FontStyles.Names) == 0) 
				{
					return string.Empty;
				}

				names = (string[])bag["Font_Names"];

				if (names.Length == 0) 
				{
					return string.Empty;
				}

				return names[0];
			}

			set 
			{
				// Seems to be a special case in MS, removing the names from the bag when Name is set to empty, 
				// but not when setting Names to an empty array
				if (value == string.Empty) {
					bag.Remove("Font_Names");
					return;
				}

				if (value == null) {
					throw new ArgumentNullException("value", "Font name cannot be null");
				}
				Names = new string[1] { value };
			}
		}

#if NET_2_0
		[RefreshProperties (RefreshProperties.Repaint)]
#endif
		[Editor("System.Windows.Forms.Design.StringArrayEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		[NotifyParentProperty(true)]
		[TypeConverter(typeof(System.Web.UI.WebControls.FontNamesConverter))]
		[WebSysDescription ("")]
		[WebCategory ("Font")]
		public string[] Names 
		{
			get 
			{
				string[] ret;

				if ((fontstyles & FontStyles.Names) == 0) 
				{
					return FontInfo.empty_names;
				}

				ret = (string[])bag["Font_Names"];

				if (ret != null) {
					return ret;
				}
				return FontInfo.empty_names;
			}

			set 
			{
				fontstyles |= FontStyles.Names;
				bag["Font_Names"] = value;
			}
		}

#if ONLY_1_1
		[Bindable(true)]
#endif
		[DefaultValue(false)]
		[NotifyParentProperty(true)]
		[WebSysDescription ("")]
		[WebCategory ("Font")]
		public bool Overline 
		{
			get 
			{
				if ((fontstyles & FontStyles.Overline) == 0) 
				{
					return false;
				}

				return bag.GetBool("Font_Overline", false);
			}

			set 
			{
				fontstyles |= FontStyles.Overline;
				bag["Font_Overline"] = value;
			}
		}

#if NET_2_0
		[RefreshProperties (RefreshProperties.Repaint)]
#else
		[Bindable(true)]
#endif
		[DefaultValue(typeof (FontUnit), "")]
		[NotifyParentProperty(true)]
		[WebSysDescription ("")]
		[WebCategory ("Font")]
		public FontUnit Size 
		{
			get 
			{
				if ((fontstyles & FontStyles.Size) == 0) 
				{
					return FontUnit.Empty;
				}

				return (FontUnit)bag["Font_Size"];
			}

			set 
			{
				if (value.Unit.Value < 0) 
				{
					throw new ArgumentOutOfRangeException("Value", value.Unit.Value, "Font size cannot be negative");
				}
				fontstyles |= FontStyles.Size;
				bag["Font_Size"] = value;
			}
		}

#if ONLY_1_1
		[Bindable(true)]
#endif
		[DefaultValue(false)]
		[NotifyParentProperty(true)]
		[WebSysDescription ("")]
		[WebCategory ("Font")]
		public bool Strikeout 
		{
			get 
			{
				if ((fontstyles & FontStyles.Strikeout) == 0) 
				{
					return false;
				}

				return bag.GetBool("Font_Strikeout", false);
			}

			set 
			{
				fontstyles |= FontStyles.Strikeout;
				bag["Font_Strikeout"] = value;
			}
		}

#if ONLY_1_1
		[Bindable(true)]
#endif
		[DefaultValue(false)]
		[NotifyParentProperty(true)]
		[WebSysDescription ("")]
		[WebCategory ("Font")]
		public bool Underline 
		{
			get 
			{
				if ((fontstyles & FontStyles.Underline) == 0) 
				{
					return false;
				}

				return bag.GetBool("Font_Underline", false);
			}

			set 
			{
				fontstyles |= FontStyles.Underline;
				bag["Font_Underline"] = value;
			}
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		public void CopyFrom(FontInfo f) 
		{
			this.Reset();

			// MS does not store the property in the bag if it's value is false
			if (((f.fontstyles & FontStyles.Bold) != 0) && f.Bold) 
			{
				this.Bold = true;
			}

			if (((f.fontstyles & FontStyles.Italic) != 0) && f.Italic) 
			{
				this.Italic = true;
			}

			// MS seems to have some weird behaviour, even if f's Name has been set to String.Empty we still get an empty array
			if (((f.fontstyles & FontStyles.Names) != 0)) 
			{
				this.Names = f.Names;
			}

			if (((f.fontstyles & FontStyles.Overline) != 0) && f.Overline) 
			{
				this.Overline = true;
			}

			if (((f.fontstyles & FontStyles.Size) != 0) && (f.Size != FontUnit.Empty)) 
			{
				this.Size = f.Size;
			}

			if (((f.fontstyles & FontStyles.Strikeout) != 0) && f.Strikeout) 
			{
				this.Strikeout = true;
			}

			if (((f.fontstyles & FontStyles.Underline) != 0) && f.Underline) 
			{
				this.Underline = true;
			}
		}

		public void MergeWith(FontInfo f) 
		{
			if (((fontstyles & FontStyles.Bold) == 0) && ((f.fontstyles & FontStyles.Bold) != 0) && f.Bold) 
			{
				this.Bold = true;
			}

			if (((fontstyles & FontStyles.Italic) == 0) && ((f.fontstyles & FontStyles.Italic) != 0) && f.Italic) 
			{
				this.Italic = true;
			}

			if (((fontstyles & FontStyles.Names) == 0) && ((f.fontstyles & FontStyles.Names) != 0)) 
			{
				this.Names = f.Names;
			}

			if (((fontstyles & FontStyles.Overline) == 0) && ((f.fontstyles & FontStyles.Overline) != 0) && f.Overline) 
			{
				this.Overline = true;
			}

			if (((fontstyles & FontStyles.Size) == 0) && ((f.fontstyles & FontStyles.Size) != 0) && (f.Size != FontUnit.Empty)) 
			{
				this.Size = f.Size;
			}

			if (((fontstyles & FontStyles.Strikeout) == 0) && ((f.fontstyles & FontStyles.Strikeout) != 0) && f.Strikeout) 
			{
				this.Strikeout = true;
			}

			if (((fontstyles & FontStyles.Underline) == 0) && ((f.fontstyles & FontStyles.Underline) != 0) && f.Underline) 
			{
				this.Underline = true;
			}
		}

		[MonoTODO]
		public bool ShouldSerializeNames() 
		{
			throw new NotImplementedException("Microsoft Internal, not sure what to do");
		}

		public override string ToString() 
		{
			if (this.Names.Length == 0) 
			{
				return string.Empty;
			}

			return this.Name + ", " + this.Size.ToString();
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
			fontstyles = FontStyles.None;
		}

		internal void LoadViewState() {
			fontstyles = FontStyles.None;

			if (bag["Font_Bold"] != null)
			{
				fontstyles |= FontStyles.Bold;
			}
			if (bag["Font_Italic"] != null)
			{
				fontstyles |= FontStyles.Italic;
			}
			if (bag["Font_Names"] != null)
			{
				fontstyles |= FontStyles.Names;
			}
			if (bag["Font_Overline"] != null)
			{
				fontstyles |= FontStyles.Overline;
			}
			if (bag["Font_Size"] != null)
			{
				fontstyles |= FontStyles.Size;
			}
			if (bag["Font_Strikeout"] != null)
			{
				fontstyles |= FontStyles.Strikeout;
			}
			if (bag["Font_Underline"] != null)
			{
				fontstyles |= FontStyles.Underline;
			}
		}
		#endregion	// Private Methods


		internal bool IsEmpty {
			get {
				return fontstyles == FontStyles.None;
			}
		}
	}
}

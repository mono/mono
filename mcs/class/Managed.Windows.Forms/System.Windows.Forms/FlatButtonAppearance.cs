//
//  FlatButtonAppearance.cs
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
// Copyright (c) 2006 Daniel Nauck
//
// Author:
//      Daniel Nauck    (dna(at)mono-project(dot)de)


using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;

namespace System.Windows.Forms
{
#if NET_2_0
	[TypeConverter (typeof (FlatButtonAppearanceConverter))]
	public 
#endif
	class FlatButtonAppearance
	{
		private Color borderColor = Color.Empty;
		private int borderSize = 1;
		private Color checkedBackColor = Color.Empty;
		private Color mouseDownBackColor = Color.Empty;
		private Color mouseOverBackColor = Color.Empty;
		private ButtonBase owner = null;

		internal FlatButtonAppearance (ButtonBase owner)
		{
			this.owner = owner;
		}

		[EditorBrowsable(EditorBrowsableState.Always)]
		[DefaultValue(typeof(Color), "")]
		[NotifyParentProperty(true)]
		[Browsable(true)]
		public Color BorderColor
		{
			get { return borderColor; }
			set {
				if(borderColor == value)
					return;

				borderColor = value;
				
				if(owner != null)
					owner.Invalidate ();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Always)]
		[DefaultValue(1)]
		[NotifyParentProperty(true)]
		[Browsable(true)]
		public int BorderSize
		{
			get { return borderSize; }
			set {
				if(borderSize == value)
					return;

				if (value < 0)
					throw new ArgumentOutOfRangeException ("value", string.Format ("'{0}' is not a valid value for 'BorderSize'. 'BorderSize' must be greater or equal than {1}.", value, 0));

				borderSize = value;

				if(owner != null)
					owner.Invalidate ();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Always)]
		[DefaultValue(typeof(Color), "")]
		[NotifyParentProperty(true)]
		[Browsable(true)]
		public Color CheckedBackColor 
		{
			get { return checkedBackColor; }
			set {
				if(checkedBackColor == value)
					return;

				checkedBackColor = value;

				if(owner != null)
					owner.Invalidate ();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Always)]
		[DefaultValue(typeof(Color), "")]
		[Browsable(true)]
		[NotifyParentProperty(true)]
		public Color MouseDownBackColor
		{
			get { return mouseDownBackColor; }
			set {
				if(mouseDownBackColor == value)
					return;

				mouseDownBackColor = value;

				if(owner != null)
					owner.Invalidate ();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Always)]
		[DefaultValue(typeof(Color), "")]
		[NotifyParentProperty(true)]
		[Browsable(true)]
		public Color MouseOverBackColor
		{
			get { return mouseOverBackColor; }
			set {
				if(mouseOverBackColor == value)
					return;

				mouseOverBackColor = value;

				if(owner != null)
					owner.Invalidate ();
			}
		}
	}
	
#if NET_2_0
	internal class FlatButtonAppearanceConverter : TypeConverter
	{
		public override object ConvertTo (ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			if ((value == null) || !(value is FlatButtonAppearance) || (destinationType != typeof (string)))
				return base.ConvertTo (context, culture, value, destinationType);

			if (culture == null)
				culture = CultureInfo.CurrentCulture;
				
			FlatButtonAppearance fba = (FlatButtonAppearance)value;
			
			return string.Format ("{0}{5} {1}{5} {2}{5} {3}{5} {4}", fba.BorderColor.ToArgb (), fba.BorderSize.ToString (), fba.CheckedBackColor.ToArgb (), fba.MouseDownBackColor.ToArgb (), fba.MouseOverBackColor.ToArgb (), culture.TextInfo.ListSeparator);
		}

		public override object ConvertFrom (ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			if ((value == null) || !(value is String))
				return base.ConvertFrom (context, culture, value);

			if (culture == null)
				culture = CultureInfo.CurrentCulture;

			string[] parts = ((string)value).Split (culture.TextInfo.ListSeparator.ToCharArray ());

			FlatButtonAppearance fba = new FlatButtonAppearance (null);
			fba.BorderColor = Color.FromArgb (int.Parse (parts[0].Trim ()));
			fba.BorderSize = int.Parse (parts[1].Trim ());
			fba.CheckedBackColor = Color.FromArgb (int.Parse (parts[2].Trim ()));
			fba.MouseDownBackColor = Color.FromArgb (int.Parse (parts[3].Trim ()));
			fba.MouseOverBackColor = Color.FromArgb (int.Parse (parts[4].Trim ()));
			
			return fba;
		}
	}
#endif
}
//
// System.Drawing.StringFormat.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2002 Ximian, Inc
// (C) 2003 Novell, Inc.
//
using System;
using System.Drawing.Text;

namespace System.Drawing
{
	/// <summary>
	/// Summary description for StringFormat.
	/// </summary>
	public sealed class StringFormat : MarshalByRefObject, IDisposable, ICloneable
	{
		private static StringFormat genericDefault;
		StringAlignment alignment;
		StringAlignment line_alignment;
		StringFormatFlags format_flags;
		HotkeyPrefix hotkey_prefix;
		StringTrimming trimming;
		
		public StringFormat()
		{
			//
			// TODO: Add constructor logic here
			//
			alignment = StringAlignment.Center;
			line_alignment = StringAlignment.Center;
			format_flags = 0;
		}

		public void Dispose()
		{
		}

		public StringFormat (StringFormat source)
		{
			alignment = source.alignment;
			line_alignment = source.line_alignment;
			format_flags = source.format_flags;
			hotkey_prefix = source.hotkey_prefix;
		}

		public StringFormat (StringFormatFlags flags)
		{
			alignment = StringAlignment.Center;
			line_alignment = StringAlignment.Center;
			format_flags = flags;
		}
		
		public StringAlignment Alignment {
			get {
				return alignment;
			}

			set {
				alignment = value;
			}
		}

		public StringAlignment LineAlignment {
			get {
				return line_alignment;
			}

			set {
				line_alignment = value;
			}
		}

		public StringFormatFlags FormatFlags {
			get {
				return format_flags;
			}

			set {
				format_flags = value;
			}
		}

		public HotkeyPrefix HotkeyPrefix {
			get {
				return hotkey_prefix;
			}

			set {
				hotkey_prefix = value;
			}
		}

		public void SetMeasurableCharacterRanges (CharacterRange [] range)
		{
		}

		public StringTrimming Trimming {
			get {
				return trimming;
			}

			set {
				trimming = value;
			}
		}

		public static StringFormat GenericDefault {
			get {
				lock (typeof (StringFormat)) {
					if (genericDefault == null)  {
						genericDefault = new StringFormat ();
						genericDefault.Alignment = StringAlignment.Near;
						//genericDefault.DigitSubstitutionMethod = StringDigitSubstitute.User;
						genericDefault.LineAlignment = StringAlignment.Near;
						genericDefault.Trimming = StringTrimming.Character;
					}
				}
				return(genericDefault);
			}
		}

		[MonoTODO("Just to return a valid result")]
		public static StringFormat GenericTypographic {
			get {
				return GenericDefault;
			}
		}

		[MonoTODO]
		public object Clone () {
			throw new NotImplementedException ();
		}
	}
}

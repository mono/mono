//
// System.Drawing.StringFormat.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//   Miguel de Icaza (miguel@ximian.com)
//   Jordi Mas i Hernandez (jordi@ximian.com)
//
// Copyright (C) 2002 Ximian, Inc (http://www.ximian.com)
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Drawing.Text;

namespace System.Drawing
{
	/// <summary>
	/// Summary description for StringFormat.
	/// </summary>
	public sealed class StringFormat : IDisposable, ICloneable
	{
		private static StringFormat genericDefault;
		private int language = 0;
		internal CharacterRange [] CharRanges;

		//local storage
		internal StringAlignment alignment;
		internal StringAlignment linealignment;
		internal HotkeyPrefix prefix;
		internal StringFormatFlags flags;
		internal StringDigitSubstitute subst;
		internal StringTrimming trimming;
		internal float firstTabOffset;
		internal float [] tabStops;
		
		public StringFormat()
		{					   
			
		}		
		
		public StringFormat(StringFormatFlags options, int lang)
		{
				flags = options;
				LineAlignment =  StringAlignment.Near;
				Alignment =  StringAlignment.Near;			
				language = lang;
		}
		
		public StringFormat(StringFormatFlags options):this(options,0)
		{
		}
		
//		~StringFormat()
//		{	
//			Dispose ();
//		}
		
		public void Dispose()
		{	
		}


		public StringFormat (StringFormat source)
		{
			Alignment = source.Alignment;
			LineAlignment = source.LineAlignment;
			HotkeyPrefix = source.HotkeyPrefix;
			subst = source.subst;
			flags = source.flags;
		}

		
		public StringAlignment Alignment 
		{
			get 
			{
				return alignment;
			}

			set 
			{
				alignment = value;
			}
		}

		public StringAlignment LineAlignment 
		{
			get 
			{
				return linealignment;
			}
			set 
			{
				linealignment = value;
			}
		}

		public StringFormatFlags FormatFlags 
		{
			get 
			{				
				return flags;
			}

			set 
			{
				flags = value;
			}
		}

		public HotkeyPrefix HotkeyPrefix 
		{
			get 
			{				
				return prefix;
			}

			set 
			{
				prefix = value;
			}
		}


		public StringTrimming Trimming 
		{
			get 
			{
				return trimming;
			}

			set 
			{
				trimming = value;
			}
		}

		public static StringFormat GenericDefault 
		{
			get 
			{
				return genericDefault;
			}
		}
		
		
		public int DigitSubstitutionLanguage 
		{
			get
			{
				return language;
			}
		}

		
		public static StringFormat GenericTypographic 
		{
			get 
			{
				throw new NotImplementedException();
			}
		}

		public StringDigitSubstitute  DigitSubstitutionMethod  
		{
			get 
			{
				return subst;     
			}
		}


		public void SetMeasurableCharacterRanges (CharacterRange [] range)
		{
			CharRanges=(CharacterRange [])range.Clone();
		}

		internal CharacterRange [] GetCharRanges
		{
			get 
			{
				return(CharRanges);
			}
		}
	
		public object Clone()
		{
			throw new NotImplementedException();
		}

		public override string ToString()
		{
			return "[StringFormat, FormatFlags=" + this.FormatFlags.ToString() + "]";
		}
		
		public void SetTabStops(float firstTabOffset, float[] tabStops)
		{
			this.firstTabOffset = firstTabOffset;
			this.tabStops = tabStops;
		}

		public void SetDigitSubstitution(int language,  StringDigitSubstitute substitute)
		{
			subst = substitute;
		}

		public float[] GetTabStops(out float firstTabOffset)
		{
			firstTabOffset = this.firstTabOffset;
			return this.tabStops;
		}

	}
}

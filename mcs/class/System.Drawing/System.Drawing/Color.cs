//
// System.Drawing.Color.cs
//
// Authors:
// 	Dennis Hayes (dennish@raytek.com)
// 	Ben Houston  (ben@exocortex.org)
// 	Gonzalo Paniagua (gonzalo@ximian.com)
// 	Juraj Skripsky (juraj@hotfeet.ch)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Dennis Hayes
// (c) 2002 Ximian, Inc. (http://www.ximiam.com)
// (C) 2005 HotFeet GmbH (http://www.hotfeet.ch)
// Copyright (C) 2004,2006-2007 Novell, Inc (http://www.novell.com)
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

using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Drawing 
{
	[TypeConverter(typeof(ColorConverter))]
#if ONLY_1_1
	[ComVisible (true)]
#endif
#if !TARGET_JVM
	[Editor ("System.Drawing.Design.ColorEditor, " + Consts.AssemblySystem_Drawing_Design, typeof (System.Drawing.Design.UITypeEditor))]
#endif
	[Serializable]
	public struct Color {

		// Private transparency (A) and R,G,B fields.
		private long value;

		// The specs also indicate that all three of these properties are true
		// if created with FromKnownColor or FromNamedColor, false otherwise (FromARGB).
		// Per Microsoft and ECMA specs these varibles are set by which constructor is used, not by their values.
		[Flags]
		internal enum ColorType : short {
			Empty=0,
			Known=1,
			ARGB=2,
			Named=4,
			System=8
		}

		internal short state;
		internal short knownColor;
// #if ONLY_1_1
// Mono bug #324144 is holding this change
		// MS 1.1 requires this member to be present for serialization (not so in 2.0)
		// however it's bad to keep a string (reference) in a struct
		internal string name;
// #endif
#if TARGET_JVM
		internal java.awt.Color NativeObject {
			get {
				return new java.awt.Color (R, G, B, A);
			}
		}

		internal static Color FromArgbNamed (int alpha, int red, int green, int blue, string name, KnownColor knownColor)
		{
			Color color = FromArgb (alpha, red, green, blue);
			color.state = (short) (ColorType.Known|ColorType.Named);
			color.name = KnownColors.GetName (knownColor);
			color.knownColor = (short) knownColor;
			return color;
		}

		internal static Color FromArgbSystem (int alpha, int red, int green, int blue, string name, KnownColor knownColor)
		{
			Color color = FromArgbNamed (alpha, red, green, blue, name, knownColor);
			color.state |= (short) ColorType.System;
			return color;
		}
#endif

		public string Name {
			get {
#if NET_2_0_ONCE_MONO_BUG_324144_IS_FIXED
				if (IsNamedColor)
					return KnownColors.GetName (knownColor);
				else
					return String.Format ("{0:x}", ToArgb ());
#else
				// name is required for serialization under 1.x, but not under 2.0
				if (name == null) {
					// Can happen with stuff deserialized from MS
					if (IsNamedColor)
						name = KnownColors.GetName (knownColor);
					else
						name = String.Format ("{0:x}", ToArgb ());
				}
				return name;
#endif
			}
		}

		public bool IsKnownColor {
			get{
				return (state & ((short) ColorType.Known)) != 0;
			}
		}

		public bool IsSystemColor {
			get{
				return (state & ((short) ColorType.System)) != 0;
			}
		}

		public bool IsNamedColor {
			get{
				return (state & (short)(ColorType.Known|ColorType.Named)) != 0;
			}
		}

		internal long Value {
			get {
				// Optimization for known colors that were deserialized
				// from an MS serialized stream.  
				if (value == 0 && IsKnownColor) {
					value = KnownColors.FromKnownColor ((KnownColor)knownColor).ToArgb () & 0xFFFFFFFF;
				}
				return value;
			}
			set { this.value = value; }
		}

		public static Color FromArgb (int red, int green, int blue)
		{
			return FromArgb (255, red, green, blue);
		}
		
		public static Color FromArgb (int alpha, int red, int green, int blue)
		{
			CheckARGBValues (alpha, red, green, blue);
			Color color = new Color ();
			color.state = (short) ColorType.ARGB;
			color.Value = (int)((uint) alpha << 24) + (red << 16) + (green << 8) + blue;
			return color;
		}

		public int ToArgb()
		{
			return (int) Value;
		} 

		public static Color FromArgb (int alpha, Color baseColor)
		{
			return FromArgb (alpha, baseColor.R, baseColor.G, baseColor.B);
		}

		public static Color FromArgb (int argb)
		{
			return FromArgb ((argb >> 24) & 0x0FF, (argb >> 16) & 0x0FF, (argb >> 8) & 0x0FF, argb & 0x0FF);
		}

		public static Color FromKnownColor (KnownColor color)
		{
			return KnownColors.FromKnownColor (color);
		}

		public static Color FromName (string name)
		{
			try {
				KnownColor kc = (KnownColor) Enum.Parse (typeof (KnownColor), name, true);
				return KnownColors.FromKnownColor (kc);
			}
			catch {
				// This is what it returns! 	 
				Color d = FromArgb (0, 0, 0, 0);
				d.name = name;
				d.state |= (short) ColorType.Named;
				return d;
			}
		}

	
		// -----------------------
		// Public Shared Members
		// -----------------------

		/// <summary>
		///	Empty Shared Field
		/// </summary>
		///
		/// <remarks>
		///	An uninitialized Color Structure
		/// </remarks>
		
		public static readonly Color Empty;
		
		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two Color objects. The return value is
		///	based on the equivalence of the A,R,G,B properties 
		///	of the two Colors.
		/// </remarks>

		public static bool operator == (Color left, Color right)
		{
			if (left.Value != right.Value)
				return false;
			if (left.IsNamedColor != right.IsNamedColor)
				return false;
			if (left.IsSystemColor != right.IsSystemColor)
				return false;
			if (left.IsEmpty != right.IsEmpty)
				return false;
			if (left.IsNamedColor) {
				// then both are named (see previous check) and so we need to compare them
				// but otherwise we don't as it kills performance (Name calls String.Format)
				if (left.Name != right.Name)
					return false;
			}
			return true;
		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two Color objects. The return value is
		///	based on the equivalence of the A,R,G,B properties 
		///	of the two colors.
		/// </remarks>

		public static bool operator != (Color left, Color right)
		{
			return ! (left == right);
		}
		
		public float GetBrightness ()
		{
			byte minval = Math.Min (R, Math.Min (G, B));
			byte maxval = Math.Max (R, Math.Max (G, B));
	
			return (float)(maxval + minval) / 510;
		}

		public float GetSaturation ()
		{
			byte minval = (byte) Math.Min (R, Math.Min (G, B));
			byte maxval = (byte) Math.Max (R, Math.Max (G, B));
			
			if (maxval == minval)
					return 0.0f;

			int sum = maxval + minval;
			if (sum > 255)
				sum = 510 - sum;

			return (float)(maxval - minval) / sum;
		}

		public float GetHue ()
		{
			int r = R;
			int g = G;
			int b = B;
			byte minval = (byte) Math.Min (r, Math.Min (g, b));
			byte maxval = (byte) Math.Max (r, Math.Max (g, b));
			
			if (maxval == minval)
					return 0.0f;
			
			float diff = (float)(maxval - minval);
			float rnorm = (maxval - r) / diff;
			float gnorm = (maxval - g) / diff;
			float bnorm = (maxval - b) / diff;
	
			float hue = 0.0f;
			if (r == maxval) 
				hue = 60.0f * (6.0f + bnorm - gnorm);
			if (g == maxval) 
				hue = 60.0f * (2.0f + rnorm - bnorm);
			if (b  == maxval) 
				hue = 60.0f * (4.0f + gnorm - rnorm);
			if (hue > 360.0f) 
				hue = hue - 360.0f;

			return hue;
		}
		
		// -----------------------
		// Public Instance Members
		// -----------------------

		/// <summary>
		///	ToKnownColor method
		/// </summary>
		///
		/// <remarks>
		///	Returns the KnownColor enum value for this color, 0 if is not known.
		/// </remarks>
		public KnownColor ToKnownColor ()
		{
			return (KnownColor) knownColor;
		}

		/// <summary>
		///	IsEmpty Property
		/// </summary>
		///
		/// <remarks>
		///	Indicates transparent black. R,G,B = 0; A=0?
		/// </remarks>
		
		public bool IsEmpty 
		{
			get {
				return state == (short) ColorType.Empty;
			}
		}

		public byte A {
			get { return (byte) (Value >> 24); }
		}

		public byte R {
			get { return (byte) (Value >> 16); }
		}

		public byte G {
			get { return (byte) (Value >> 8); }
		}

		public byte B {
			get { return (byte) Value; }
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this Color and another object.
		/// </remarks>
		
		public override bool Equals (object obj)
		{
			if (!(obj is Color))
				return false;
			Color c = (Color) obj;
			return this == c;
		}

		/// <summary>
		///	Reference Equals Method
		///	Is commented out because this is handled by the base class.
		///	TODO: Is it correct to let the base class handel reference equals
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this Color and another object.
		/// </remarks>
		//public bool ReferenceEquals (object o)
		//{
		//	if (!(o is Color))return false;
		//	return (this == (Color) o);
		//}



		/// <summary>
		///	GetHashCode Method
		/// </summary>
		///
		/// <remarks>
		///	Calculates a hashing value.
		/// </remarks>
		
		public override int GetHashCode ()
		{
			int hc = (int)(Value ^ (Value >> 32) ^ state ^ (knownColor >> 16));
			if (IsNamedColor)
				hc ^= Name.GetHashCode ();
			return hc;
		}

		/// <summary>
		///	ToString Method
		/// </summary>
		///
		/// <remarks>
		///	Formats the Color as a string in ARGB notation.
		/// </remarks>
		
		public override string ToString ()
		{
			if (IsEmpty)
				return "Color [Empty]";

			// Use the property here, not the field.
			if (IsNamedColor)
				return "Color [" + Name + "]";

			return String.Format ("Color [A={0}, R={1}, G={2}, B={3}]", A, R, G, B);
		}
 
		private static void CheckRGBValues (int red,int green,int blue)
		{
			if( (red > 255) || (red < 0))
				throw CreateColorArgumentException(red, "red");
			if( (green > 255) || (green < 0))
				throw CreateColorArgumentException (green, "green");
			if( (blue > 255) || (blue < 0))
				throw CreateColorArgumentException (blue, "blue");
		}

		private static ArgumentException CreateColorArgumentException (int value, string color)
		{
			return new ArgumentException (string.Format ("'{0}' is not a valid"
				+ " value for '{1}'. '{1}' should be greater or equal to 0 and"
				+ " less than or equal to 255.", value, color));
		}

		private static void CheckARGBValues (int alpha,int red,int green,int blue)
		{
			if( (alpha > 255) || (alpha < 0))
				throw CreateColorArgumentException (alpha, "alpha");
			CheckRGBValues(red,green,blue);
		}


		static public Color Transparent {
			get { return KnownColors.FromKnownColor (KnownColor.Transparent); }
		}

		static public Color AliceBlue {
			get { return KnownColors.FromKnownColor (KnownColor.AliceBlue); }
		}

		static public Color AntiqueWhite {
			get { return KnownColors.FromKnownColor (KnownColor.AntiqueWhite); }
		}

		static public Color Aqua {
			get { return KnownColors.FromKnownColor (KnownColor.Aqua); }
		}

		static public Color Aquamarine {
			get { return KnownColors.FromKnownColor (KnownColor.Aquamarine); }
		}

		static public Color Azure {
			get { return KnownColors.FromKnownColor (KnownColor.Azure); }
		}

		static public Color Beige {
			get { return KnownColors.FromKnownColor (KnownColor.Beige); }
		}

		static public Color Bisque {
			get { return KnownColors.FromKnownColor (KnownColor.Bisque); }
		}

		static public Color Black {
			get { return KnownColors.FromKnownColor (KnownColor.Black); }
		}

		static public Color BlanchedAlmond {
			get { return KnownColors.FromKnownColor (KnownColor.BlanchedAlmond); }
		}

		static public Color Blue {
			get { return KnownColors.FromKnownColor (KnownColor.Blue); }
		}

		static public Color BlueViolet {
			get { return KnownColors.FromKnownColor (KnownColor.BlueViolet); }
		}

		static public Color Brown {
			get { return KnownColors.FromKnownColor (KnownColor.Brown); }
		}

		static public Color BurlyWood {
			get { return KnownColors.FromKnownColor (KnownColor.BurlyWood); }
		}

		static public Color CadetBlue {
			get { return KnownColors.FromKnownColor (KnownColor.CadetBlue); }
		}

		static public Color Chartreuse {
			get { return KnownColors.FromKnownColor (KnownColor.Chartreuse); }
		}

		static public Color Chocolate {
			get { return KnownColors.FromKnownColor (KnownColor.Chocolate); }
		}

		static public Color Coral {
			get { return KnownColors.FromKnownColor (KnownColor.Coral); }
		}

		static public Color CornflowerBlue {
			get { return KnownColors.FromKnownColor (KnownColor.CornflowerBlue); }
		}

		static public Color Cornsilk {
			get { return KnownColors.FromKnownColor (KnownColor.Cornsilk); }
		}

		static public Color Crimson {
			get { return KnownColors.FromKnownColor (KnownColor.Crimson); }
		}

		static public Color Cyan {
			get { return KnownColors.FromKnownColor (KnownColor.Cyan); }
		}

		static public Color DarkBlue {
			get { return KnownColors.FromKnownColor (KnownColor.DarkBlue); }
		}

		static public Color DarkCyan {
			get { return KnownColors.FromKnownColor (KnownColor.DarkCyan); }
		}

		static public Color DarkGoldenrod {
			get { return KnownColors.FromKnownColor (KnownColor.DarkGoldenrod); }
		}

		static public Color DarkGray {
			get { return KnownColors.FromKnownColor (KnownColor.DarkGray); }
		}

		static public Color DarkGreen {
			get { return KnownColors.FromKnownColor (KnownColor.DarkGreen); }
		}

		static public Color DarkKhaki {
			get { return KnownColors.FromKnownColor (KnownColor.DarkKhaki); }
		}

		static public Color DarkMagenta {
			get { return KnownColors.FromKnownColor (KnownColor.DarkMagenta); }
		}

		static public Color DarkOliveGreen {
			get { return KnownColors.FromKnownColor (KnownColor.DarkOliveGreen); }
		}

		static public Color DarkOrange {
			get { return KnownColors.FromKnownColor (KnownColor.DarkOrange); }
		}

		static public Color DarkOrchid {
			get { return KnownColors.FromKnownColor (KnownColor.DarkOrchid); }
		}

		static public Color DarkRed {
			get { return KnownColors.FromKnownColor (KnownColor.DarkRed); }
		}

		static public Color DarkSalmon {
			get { return KnownColors.FromKnownColor (KnownColor.DarkSalmon); }
		}

		static public Color DarkSeaGreen {
			get { return KnownColors.FromKnownColor (KnownColor.DarkSeaGreen); }
		}

		static public Color DarkSlateBlue {
			get { return KnownColors.FromKnownColor (KnownColor.DarkSlateBlue); }
		}

		static public Color DarkSlateGray {
			get { return KnownColors.FromKnownColor (KnownColor.DarkSlateGray); }
		}

		static public Color DarkTurquoise {
			get { return KnownColors.FromKnownColor (KnownColor.DarkTurquoise); }
		}

		static public Color DarkViolet {
			get { return KnownColors.FromKnownColor (KnownColor.DarkViolet); }
		}

		static public Color DeepPink {
			get { return KnownColors.FromKnownColor (KnownColor.DeepPink); }
		}

		static public Color DeepSkyBlue {
			get { return KnownColors.FromKnownColor (KnownColor.DeepSkyBlue); }
		}

		static public Color DimGray {
			get { return KnownColors.FromKnownColor (KnownColor.DimGray); }
		}

		static public Color DodgerBlue {
			get { return KnownColors.FromKnownColor (KnownColor.DodgerBlue); }
		}

		static public Color Firebrick {
			get { return KnownColors.FromKnownColor (KnownColor.Firebrick); }
		}

		static public Color FloralWhite {
			get { return KnownColors.FromKnownColor (KnownColor.FloralWhite); }
		}

		static public Color ForestGreen {
			get { return KnownColors.FromKnownColor (KnownColor.ForestGreen); }
		}

		static public Color Fuchsia {
			get { return KnownColors.FromKnownColor (KnownColor.Fuchsia); }
		}

		static public Color Gainsboro {
			get { return KnownColors.FromKnownColor (KnownColor.Gainsboro); }
		}

		static public Color GhostWhite {
			get { return KnownColors.FromKnownColor (KnownColor.GhostWhite); }
		}

		static public Color Gold {
			get { return KnownColors.FromKnownColor (KnownColor.Gold); }
		}

		static public Color Goldenrod {
			get { return KnownColors.FromKnownColor (KnownColor.Goldenrod); }
		}

		static public Color Gray {
			get { return KnownColors.FromKnownColor (KnownColor.Gray); }
		}

		static public Color Green {
			get { return KnownColors.FromKnownColor (KnownColor.Green); }
		}

		static public Color GreenYellow {
			get { return KnownColors.FromKnownColor (KnownColor.GreenYellow); }
		}

		static public Color Honeydew {
			get { return KnownColors.FromKnownColor (KnownColor.Honeydew); }
		}

		static public Color HotPink {
			get { return KnownColors.FromKnownColor (KnownColor.HotPink); }
		}

		static public Color IndianRed {
			get { return KnownColors.FromKnownColor (KnownColor.IndianRed); }
		}

		static public Color Indigo {
			get { return KnownColors.FromKnownColor (KnownColor.Indigo); }
		}

		static public Color Ivory {
			get { return KnownColors.FromKnownColor (KnownColor.Ivory); }
		}

		static public Color Khaki {
			get { return KnownColors.FromKnownColor (KnownColor.Khaki); }
		}

		static public Color Lavender {
			get { return KnownColors.FromKnownColor (KnownColor.Lavender); }
		}

		static public Color LavenderBlush {
			get { return KnownColors.FromKnownColor (KnownColor.LavenderBlush); }
		}

		static public Color LawnGreen {
			get { return KnownColors.FromKnownColor (KnownColor.LawnGreen); }
		}

		static public Color LemonChiffon {
			get { return KnownColors.FromKnownColor (KnownColor.LemonChiffon); }
		}

		static public Color LightBlue {
			get { return KnownColors.FromKnownColor (KnownColor.LightBlue); }
		}

		static public Color LightCoral {
			get { return KnownColors.FromKnownColor (KnownColor.LightCoral); }
		}

		static public Color LightCyan {
			get { return KnownColors.FromKnownColor (KnownColor.LightCyan); }
		}

		static public Color LightGoldenrodYellow {
			get { return KnownColors.FromKnownColor (KnownColor.LightGoldenrodYellow); }
		}

		static public Color LightGreen {
			get { return KnownColors.FromKnownColor (KnownColor.LightGreen); }
		}

		static public Color LightGray {
			get { return KnownColors.FromKnownColor (KnownColor.LightGray); }
		}

		static public Color LightPink {
			get { return KnownColors.FromKnownColor (KnownColor.LightPink); }
		}

		static public Color LightSalmon {
			get { return KnownColors.FromKnownColor (KnownColor.LightSalmon); }
		}

		static public Color LightSeaGreen {
			get { return KnownColors.FromKnownColor (KnownColor.LightSeaGreen); }
		}

		static public Color LightSkyBlue {
			get { return KnownColors.FromKnownColor (KnownColor.LightSkyBlue); }
		}

		static public Color LightSlateGray {
			get { return KnownColors.FromKnownColor (KnownColor.LightSlateGray); }
		}

		static public Color LightSteelBlue {
			get { return KnownColors.FromKnownColor (KnownColor.LightSteelBlue); }
		}

		static public Color LightYellow {
			get { return KnownColors.FromKnownColor (KnownColor.LightYellow); }
		}

		static public Color Lime {
			get { return KnownColors.FromKnownColor (KnownColor.Lime); }
		}

		static public Color LimeGreen {
			get { return KnownColors.FromKnownColor (KnownColor.LimeGreen); }
		}

		static public Color Linen {
			get { return KnownColors.FromKnownColor (KnownColor.Linen); }
		}

		static public Color Magenta {
			get { return KnownColors.FromKnownColor (KnownColor.Magenta); }
		}

		static public Color Maroon {
			get { return KnownColors.FromKnownColor (KnownColor.Maroon); }
		}

		static public Color MediumAquamarine {
			get { return KnownColors.FromKnownColor (KnownColor.MediumAquamarine); }
		}

		static public Color MediumBlue {
			get { return KnownColors.FromKnownColor (KnownColor.MediumBlue); }
		}

		static public Color MediumOrchid {
			get { return KnownColors.FromKnownColor (KnownColor.MediumOrchid); }
		}

		static public Color MediumPurple {
			get { return KnownColors.FromKnownColor (KnownColor.MediumPurple); }
		}

		static public Color MediumSeaGreen {
			get { return KnownColors.FromKnownColor (KnownColor.MediumSeaGreen); }
		}

		static public Color MediumSlateBlue {
			get { return KnownColors.FromKnownColor (KnownColor.MediumSlateBlue); }
		}

		static public Color MediumSpringGreen {
			get { return KnownColors.FromKnownColor (KnownColor.MediumSpringGreen); }
		}

		static public Color MediumTurquoise {
			get { return KnownColors.FromKnownColor (KnownColor.MediumTurquoise); }
		}

		static public Color MediumVioletRed {
			get { return KnownColors.FromKnownColor (KnownColor.MediumVioletRed); }
		}

		static public Color MidnightBlue {
			get { return KnownColors.FromKnownColor (KnownColor.MidnightBlue); }
		}

		static public Color MintCream {
			get { return KnownColors.FromKnownColor (KnownColor.MintCream); }
		}

		static public Color MistyRose {
			get { return KnownColors.FromKnownColor (KnownColor.MistyRose); }
		}

		static public Color Moccasin {
			get { return KnownColors.FromKnownColor (KnownColor.Moccasin); }
		}

		static public Color NavajoWhite {
			get { return KnownColors.FromKnownColor (KnownColor.NavajoWhite); }
		}

		static public Color Navy {
			get { return KnownColors.FromKnownColor (KnownColor.Navy); }
		}

		static public Color OldLace {
			get { return KnownColors.FromKnownColor (KnownColor.OldLace); }
		}

		static public Color Olive {
			get { return KnownColors.FromKnownColor (KnownColor.Olive); }
		}

		static public Color OliveDrab {
			get { return KnownColors.FromKnownColor (KnownColor.OliveDrab); }
		}

		static public Color Orange {
			get { return KnownColors.FromKnownColor (KnownColor.Orange); }
		}

		static public Color OrangeRed {
			get { return KnownColors.FromKnownColor (KnownColor.OrangeRed); }
		}

		static public Color Orchid {
			get { return KnownColors.FromKnownColor (KnownColor.Orchid); }
		}

		static public Color PaleGoldenrod {
			get { return KnownColors.FromKnownColor (KnownColor.PaleGoldenrod); }
		}

		static public Color PaleGreen {
			get { return KnownColors.FromKnownColor (KnownColor.PaleGreen); }
		}

		static public Color PaleTurquoise {
			get { return KnownColors.FromKnownColor (KnownColor.PaleTurquoise); }
		}

		static public Color PaleVioletRed {
			get { return KnownColors.FromKnownColor (KnownColor.PaleVioletRed); }
		}

		static public Color PapayaWhip {
			get { return KnownColors.FromKnownColor (KnownColor.PapayaWhip); }
		}

		static public Color PeachPuff {
			get { return KnownColors.FromKnownColor (KnownColor.PeachPuff); }
		}

		static public Color Peru {
			get { return KnownColors.FromKnownColor (KnownColor.Peru); }
		}

		static public Color Pink {
			get { return KnownColors.FromKnownColor (KnownColor.Pink); }
		}

		static public Color Plum {
			get { return KnownColors.FromKnownColor (KnownColor.Plum); }
		}

		static public Color PowderBlue {
			get { return KnownColors.FromKnownColor (KnownColor.PowderBlue); }
		}

		static public Color Purple {
			get { return KnownColors.FromKnownColor (KnownColor.Purple); }
		}

		static public Color Red {
			get { return KnownColors.FromKnownColor (KnownColor.Red); }
		}

		static public Color RosyBrown {
			get { return KnownColors.FromKnownColor (KnownColor.RosyBrown); }
		}

		static public Color RoyalBlue {
			get { return KnownColors.FromKnownColor (KnownColor.RoyalBlue); }
		}

		static public Color SaddleBrown {
			get { return KnownColors.FromKnownColor (KnownColor.SaddleBrown); }
		}

		static public Color Salmon {
			get { return KnownColors.FromKnownColor (KnownColor.Salmon); }
		}

		static public Color SandyBrown {
			get { return KnownColors.FromKnownColor (KnownColor.SandyBrown); }
		}

		static public Color SeaGreen {
			get { return KnownColors.FromKnownColor (KnownColor.SeaGreen); }
		}

		static public Color SeaShell {
			get { return KnownColors.FromKnownColor (KnownColor.SeaShell); }
		}

		static public Color Sienna {
			get { return KnownColors.FromKnownColor (KnownColor.Sienna); }
		}

		static public Color Silver {
			get { return KnownColors.FromKnownColor (KnownColor.Silver); }
		}

		static public Color SkyBlue {
			get { return KnownColors.FromKnownColor (KnownColor.SkyBlue); }
		}

		static public Color SlateBlue {
			get { return KnownColors.FromKnownColor (KnownColor.SlateBlue); }
		}

		static public Color SlateGray {
			get { return KnownColors.FromKnownColor (KnownColor.SlateGray); }
		}

		static public Color Snow {
			get { return KnownColors.FromKnownColor (KnownColor.Snow); }
		}

		static public Color SpringGreen {
			get { return KnownColors.FromKnownColor (KnownColor.SpringGreen); }
		}

		static public Color SteelBlue {
			get { return KnownColors.FromKnownColor (KnownColor.SteelBlue); }
		}

		static public Color Tan {
			get { return KnownColors.FromKnownColor (KnownColor.Tan); }
		}

		static public Color Teal {
			get { return KnownColors.FromKnownColor (KnownColor.Teal); }
		}

		static public Color Thistle {
			get { return KnownColors.FromKnownColor (KnownColor.Thistle); }
		}

		static public Color Tomato {
			get { return KnownColors.FromKnownColor (KnownColor.Tomato); }
		}

		static public Color Turquoise {
			get { return KnownColors.FromKnownColor (KnownColor.Turquoise); }
		}

		static public Color Violet {
			get { return KnownColors.FromKnownColor (KnownColor.Violet); }
		}

		static public Color Wheat {
			get { return KnownColors.FromKnownColor (KnownColor.Wheat); }
		}

		static public Color White {
			get { return KnownColors.FromKnownColor (KnownColor.White); }
		}

		static public Color WhiteSmoke {
			get { return KnownColors.FromKnownColor (KnownColor.WhiteSmoke); }
		}

		static public Color Yellow {
			get { return KnownColors.FromKnownColor (KnownColor.Yellow); }
		}

		static public Color YellowGreen {
			get { return KnownColors.FromKnownColor (KnownColor.YellowGreen); }
		}
	}
}

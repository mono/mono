
//
// System.Drawing.Color.cs
//
// (C) 2002 Dennis Hayes
// Author:
// Dennis Hayes (dennish@raytek.com)
// Ben Houston  (ben@exocortex.org)
//
// TODO: Are the static/non static functions declared correctly

using System;
using System.ComponentModel;
using System.Reflection;

namespace System.Drawing 
{
	[TypeConverter("System.Drawing.ColorConverter,System.Drawing")]
	[Serializable]
	public struct Color
	{
		// Private transparancy (A) and R,G,B fields.
		byte a;
		byte r;
		byte g;
		byte b;

		// The specs also indicate that all three of these propities are true
		// if created with FromKnownColor or FromNamedColor, false otherwise (FromARGB).
		// Per Microsoft and ECMA specs these varibles are set by which constructor is used, not by their values.
		bool isknowncolor;
		bool isnamedcolor;
		bool issystemcolor;

		string myname;

		public string Name {
			get{
				return myname;
			}
		}

		public bool IsKnownColor {
			get{
				return isknowncolor;
			}
		}

		public bool IsSystemColor {
			get{
				return issystemcolor;
			}
		}

		public bool IsNamedColor {
			get{
				return isnamedcolor;
			}
		}


		public static Color FromArgb (int red, int green, int blue)
		{
			//TODO: convert rgb to name format "12345678"
			CheckRGBValues(red, green, blue);
			Color color;
			color.myname = "";
			color.isknowncolor = false;
			color.isnamedcolor = false;
			color.issystemcolor = false;
			color.a = 255;
			color.r = (byte)red;
			color.g = (byte)green;
			color.b = (byte)blue;
			return color;
		}
		
		public static Color FromArgb (int alpha, int red, int green, int blue)
		{
			//TODO: convert rgb to name format "12345678"
			CheckARGBValues(alpha, red, green, blue);
			Color color;
			color.isknowncolor = false;
			color.isnamedcolor = false;
			color.issystemcolor = false;
			color.myname = "";
			color.a = (byte)alpha;
			color.r = (byte)red;
			color.g = (byte)green;
			color.b = (byte)blue;
			return color;
		}
		public int ToArgb()
		{
			return a << 24 | r << 16 | g << 8 | b;
		} 

		public static Color FromArgb (int alpha, Color baseColor)
		{
			//TODO: convert basecolor rgb to name
			//check alpha, use valid dummy values for rgb.
			CheckARGBValues(alpha, 0, 0, 0);
			Color color;
			color.isknowncolor = false;
			color.isnamedcolor = false;
			color.issystemcolor = false;
			color.myname = "";
			color.a = (byte)alpha;
			color.r = baseColor.r;
			color.g = baseColor.g;
			color.b = baseColor.b;
			return color;
		}

		public static Color FromArgb (int argb)
		{
			//TODO: convert irgb to name
			Color color;
			color.isknowncolor = false;
			color.isnamedcolor = false;
			color.issystemcolor = false;
			color.myname = "";
			color.a = (byte) (argb >> 24);
			color.r = (byte) (argb >> 16);
			color.g = (byte) (argb >> 8);
			color.b = (byte)argb;
			return color;
		}

		public static Color FromKnownColor (KnownColor KnownColorToConvert)
		{
//			isknowncolor = true;
//			isnamedcolor = true;
//			issystemcolor = true;

//			name = KnownColorToConvert.ToString();

			return FromName(KnownColorToConvert.ToString());
		}

		public KnownColor ToKnownColor () {
			if(isknowncolor){
				// TODO: return correct enumeration of knowncolor. note the return 0 in the else block is correct.
				return (KnownColor)0;
			}
			else{
				return (KnownColor)0;
			}
			//return KnownColor.FromName(KnownColorToConvert.ToString());
		}
		public static Color FromName( string ColorName ) 
		{
//			isknowncolor = true;
//			isnamedcolor = true;
//			issystemcolor = true;
			
			string name = ColorName;

			Type colorType = typeof( Color );
			PropertyInfo[] properties =
				colorType.GetProperties();
			foreach( PropertyInfo property in properties ){ 
				if( property.Name == name ){ 
					MethodInfo method =	property.GetGetMethod();
					if( method != null &&
						method.IsStatic == true
						&&
						method.ReturnType ==
						colorType ){ 
							return (Color) 
								    method.Invoke( null, new object[0] );
					}
				}
			}
			throw new System.ArgumentException(name + " is not a named color","name");
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

		public static bool operator == (Color colorA, Color colorB)
		{
			return ((colorA.a == colorB.a) && (colorA.r == colorB.r)
			&& (colorA.g == colorB.g) && (colorA.b == colorB.b));
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

		public static bool operator != (Color colorA, Color colorB)
		{
			return ((colorA.a != colorB.a) || (colorA.r != colorB.r)
			|| (colorA.g != colorB.g) || (colorA.b != colorB.b));
		}
		
		public float GetBrightness (){
			// Intensity is the normalized sum of the three RGB values.;
			return ((float)(r + g + b))/(255*3);
		}
		public float GetSaturation (){
			// S = 1 - I * Min(r,g,b)
			return (255 - 
				(((float)(r + g +b))/3)*Math.Min(r,Math.Min(g,b))
				)/255;
		}

		public float GetHue (){
			float top = ((float)(2*r-g-b))/(2*255);
			float bottom = (float)Math.Sqrt(((r-g)*(r-g) + (r-b)*(g-b))/255);
			return (float)Math.Acos(top/bottom);
		}
		
		// -----------------------
		// Public Constructors
		// -----------------------
		public Color(int alpha, int red, int green, int blue)
		{
			CheckARGBValues(alpha, red, green, blue);
			a = (byte)alpha;
			r = (byte)red;
			g = (byte)green;
			b = (byte)blue;
			isknowncolor = false;
			isnamedcolor = false;
			issystemcolor = false;
			myname = "";
		}

		// -----------------------
		// Public Instance Members
		// -----------------------

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
				return (a + r + g + b) == 0;
			}
		}

		/// <summary>
		///	A Property
		/// </summary>
		///
		/// <remarks>
		///	The transparancy of the Color.
		/// </remarks>
		
		public byte A
		{
			get {
				return a;
			}
		}

		/// <summary>
		///	R Property
		/// </summary>
		///
		/// <remarks>
		///	The red value of the Color.
		/// </remarks>
		
		public byte R
		{
			get {
				return r;
			}
		}

		/// <summary>
		///	G Property
		/// </summary>
		///
		/// <remarks>
		///	The green value of the Color.
		/// </remarks>
		
		public byte G
		{
			get {
				return g;
			}
		}

		/// <summary>
		///	B Property
		/// </summary>
		///
		/// <remarks>
		///	The blue value of the Color.
		/// </remarks>
		
		public byte B
		{
			get {
				return b;
			}
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this Color and another object.
		/// </remarks>
		
		public override bool Equals (object o)
		{
			if (!(o is Color))return false;
			return (this == (Color) o);
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
			return ToArgb().GetHashCode();
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
			return String.Format ("[{0},{1},{2},{3}]", a, r, g, b);
		}
  
		private static void CheckRGBValues (int red,int green,int blue)
		{
			if( (red > 255) || (red < 0))
				throw new System.ArgumentOutOfRangeException
					("red",red,"Value must be in the range 0 - 255");
			if( (green > 255) || (green < 0))
				throw new System.ArgumentOutOfRangeException
					("green",green,"Value must be in the range 0 - 255");
			if( (blue > 255) || (blue < 0))
				throw new System.ArgumentOutOfRangeException
					("blue",blue,"Value must be in the range 0 - 255");
		}

		private static void CheckARGBValues (int alpha,int red,int green,int blue)
		{
			if( (alpha > 255) || (alpha < 0))
				throw new System.ArgumentOutOfRangeException
					("alpha",alpha,"Value must be in the range 0 - 255");
			CheckRGBValues(red,green,blue);
		}

		//Documentation, do not remove!
		//This is the program that was used to generate the C# source code below.
		//static void Main(string[] args)
		//{
		//	Type cType = typeof( Color );
		//	PropertyInfo[] properties = cType.GetProperties();
		//	foreach( PropertyInfo property in properties ) 
		//	{
		//		MethodInfo method = property.GetGetMethod();
		//		if( method != null && method.IsStatic && method.ReturnType == cType
		//			) 
		//		{
		//			Color c = (Color) method.Invoke( null, new object[0] );
		//			Debug.WriteLine( "static public Color " + property.Name + " {" );
		//			Debug.WriteLine( "\tget{" );
		//			Debug.WriteLine( "\t\treturn Color.FromArgb( " + c.A + ", " + c.R
		//				+ ", " + c.G + ", " + c.B + " );" );
		//			Debug.WriteLine( "\t}" );
		//			Debug.WriteLine( "}" );
		//		}
		//	}
		//}

		static public Color Transparent 
		{
			get	{
				return Color.FromArgb( 0, 255, 255, 255 );
			}
		}
		static public Color AliceBlue 
		{
			get	{
				return Color.FromArgb( 255, 240, 248, 255 );
			}
		}
		static public Color AntiqueWhite 
		{
			get	{
				return Color.FromArgb( 255, 250, 235, 215 );
			}
		}
		static public Color Aqua 
		{
			get	{
				return Color.FromArgb( 255, 0, 255, 255 );
			}
		}
		static public Color Aquamarine 
		{
			get	{
				return Color.FromArgb( 255, 127, 255, 212 );
			}
		}
		static public Color Azure 
		{
			get	{
				return Color.FromArgb( 255, 240, 255, 255 );
			}
		}
		static public Color Beige 
		{
			get	{
				return Color.FromArgb( 255, 245, 245, 220 );
			}
		}
		static public Color Bisque 
		{
			get	{
				return Color.FromArgb( 255, 255, 228, 196 );
			}
		}
		static public Color Black 
		{
			get	{
				return Color.FromArgb( 255, 0, 0, 0 );
			}
		}
		static public Color BlanchedAlmond 
		{
			get	{
				return Color.FromArgb( 255, 255, 235, 205 );
			}
		}
		static public Color Blue 
		{
			get	{
				return Color.FromArgb( 255, 0, 0, 255 );
			}
		}
		static public Color BlueViolet 
		{
			get	{
				return Color.FromArgb( 255, 138, 43, 226 );
			}
		}
		static public Color Brown 
		{
			get	{
				return Color.FromArgb( 255, 165, 42, 42 );
			}
		}
		static public Color BurlyWood 
		{
			get	{
				return Color.FromArgb( 255, 222, 184, 135 );
			}
		}
		static public Color CadetBlue 
		{
			get	{
				return Color.FromArgb( 255, 95, 158, 160 );
			}
		}
		static public Color Chartreuse 
		{
			get	{
				return Color.FromArgb( 255, 127, 255, 0 );
			}
		}
		static public Color Chocolate 
		{
			get	{
				return Color.FromArgb( 255, 210, 105, 30 );
			}
		}
		static public Color Coral 
		{
			get	{
				return Color.FromArgb( 255, 255, 127, 80 );
			}
		}
		static public Color CornflowerBlue 
		{
			get	{
				return Color.FromArgb( 255, 100, 149, 237 );
			}
		}
		static public Color Cornsilk 
		{
			get	{
				return Color.FromArgb( 255, 255, 248, 220 );
			}
		}
		static public Color Crimson 
		{
			get	{
				return Color.FromArgb( 255, 220, 20, 60 );
			}
		}
		static public Color Cyan 
		{
			get	{
				return Color.FromArgb( 255, 0, 255, 255 );
			}
		}
		static public Color DarkBlue 
		{
			get	{
				return Color.FromArgb( 255, 0, 0, 139 );
			}
		}
		static public Color DarkCyan 
		{
			get	{
				return Color.FromArgb( 255, 0, 139, 139 );
			}
		}
		static public Color DarkGoldenrod 
		{
			get	{
				return Color.FromArgb( 255, 184, 134, 11 );
			}
		}
		static public Color DarkGray 
		{
			get	{
				return Color.FromArgb( 255, 169, 169, 169 );
			}
		}
		static public Color DarkGreen 
		{
			get	{
				return Color.FromArgb( 255, 0, 100, 0 );
			}
		}
		static public Color DarkKhaki 
		{
			get	{
				return Color.FromArgb( 255, 189, 183, 107 );
			}
		}
		static public Color DarkMagenta 
		{
			get	{
				return Color.FromArgb( 255, 139, 0, 139 );
			}
		}
		static public Color DarkOliveGreen 
		{
			get	{
				return Color.FromArgb( 255, 85, 107, 47 );
			}
		}
		static public Color DarkOrange 
		{
			get	{
				return Color.FromArgb( 255, 255, 140, 0 );
			}
		}
		static public Color DarkOrchid 
		{
			get	{
				return Color.FromArgb( 255, 153, 50, 204 );
			}
		}
		static public Color DarkRed 
		{
			get	{
				return Color.FromArgb( 255, 139, 0, 0 );
			}
		}
		static public Color DarkSalmon 
		{
			get	{
				return Color.FromArgb( 255, 233, 150, 122 );
			}
		}
		static public Color DarkSeaGreen 
		{
			get	{
				return Color.FromArgb( 255, 143, 188, 139 );
			}
		}
		static public Color DarkSlateBlue 
		{
			get	{
				return Color.FromArgb( 255, 72, 61, 139 );
			}
		}
		static public Color DarkSlateGray 
		{
			get	{
				return Color.FromArgb( 255, 47, 79, 79 );
			}
		}
		static public Color DarkTurquoise 
		{
			get	{
				return Color.FromArgb( 255, 0, 206, 209 );
			}
		}
		static public Color DarkViolet 
		{
			get	{
				return Color.FromArgb( 255, 148, 0, 211 );
			}
		}
		static public Color DeepPink 
		{
			get	{
				return Color.FromArgb( 255, 255, 20, 147 );
			}
		}
		static public Color DeepSkyBlue 
		{
			get	{
				return Color.FromArgb( 255, 0, 191, 255 );
			}
		}
		static public Color DimGray 
		{
			get	{
				return Color.FromArgb( 255, 105, 105, 105 );
			}
		}
		static public Color DodgerBlue 
		{
			get	{
				return Color.FromArgb( 255, 30, 144, 255 );
			}
		}
		static public Color Firebrick 
		{
			get	{
				return Color.FromArgb( 255, 178, 34, 34 );
			}
		}
		static public Color FloralWhite 
		{
			get	{
				return Color.FromArgb( 255, 255, 250, 240 );
			}
		}
		static public Color ForestGreen 
		{
			get	{
				return Color.FromArgb( 255, 34, 139, 34 );
			}
		}
		static public Color Fuchsia 
		{
			get	{
				return Color.FromArgb( 255, 255, 0, 255 );
			}
		}
		static public Color Gainsboro 
		{
			get	{
				return Color.FromArgb( 255, 220, 220, 220 );
			}
		}
		static public Color GhostWhite 
		{
			get	{
				return Color.FromArgb( 255, 248, 248, 255 );
			}
		}
		static public Color Gold 
		{
			get	{
				return Color.FromArgb( 255, 255, 215, 0 );
			}
		}
		static public Color Goldenrod 
		{
			get	{
				return Color.FromArgb( 255, 218, 165, 32 );
			}
		}
		static public Color Gray 
		{
			get	{
				return Color.FromArgb( 255, 128, 128, 128 );
			}
		}
		static public Color Green 
		{
			get	{
				return Color.FromArgb( 255, 0, 128, 0 );
			}
		}
		static public Color GreenYellow 
		{
			get	{
				return Color.FromArgb( 255, 173, 255, 47 );
			}
		}
		static public Color Honeydew 
		{
			get	{
				return Color.FromArgb( 255, 240, 255, 240 );
			}
		}
		static public Color HotPink 
		{
			get	{
				return Color.FromArgb( 255, 255, 105, 180 );
			}
		}
		static public Color IndianRed 
		{
			get	{
				return Color.FromArgb( 255, 205, 92, 92 );
			}
		}
		static public Color Indigo 
		{
			get	{
				return Color.FromArgb( 255, 75, 0, 130 );
			}
		}
		static public Color Ivory 
		{
			get	{
				return Color.FromArgb( 255, 255, 255, 240 );
			}
		}
		static public Color Khaki 
		{
			get	{
				return Color.FromArgb( 255, 240, 230, 140 );
			}
		}
		static public Color Lavender 
		{
			get	{
				return Color.FromArgb( 255, 230, 230, 250 );
			}
		}
		static public Color LavenderBlush 
		{
			get	{
				return Color.FromArgb( 255, 255, 240, 245 );
			}
		}
		static public Color LawnGreen 
		{
			get	{
				return Color.FromArgb( 255, 124, 252, 0 );
			}
		}
		static public Color LemonChiffon 
		{
			get	{
				return Color.FromArgb( 255, 255, 250, 205 );
			}
		}
		static public Color LightBlue 
		{
			get	{
				return Color.FromArgb( 255, 173, 216, 230 );
			}
		}
		static public Color LightCoral 
		{
			get	{
				return Color.FromArgb( 255, 240, 128, 128 );
			}
		}
		static public Color LightCyan 
		{
			get	{
				return Color.FromArgb( 255, 224, 255, 255 );
			}
		}
		static public Color LightGoldenrodYellow 
		{
			get	{
				return Color.FromArgb( 255, 250, 250, 210 );
			}
		}
		static public Color LightGreen 
		{
			get	{
				return Color.FromArgb( 255, 144, 238, 144 );
			}
		}
		static public Color LightGray 
		{
			get	{
				return Color.FromArgb( 255, 211, 211, 211 );
			}
		}
		static public Color LightPink 
		{
			get	{
				return Color.FromArgb( 255, 255, 182, 193 );
			}
		}
		static public Color LightSalmon 
		{
			get	{
				return Color.FromArgb( 255, 255, 160, 122 );
			}
		}
		static public Color LightSeaGreen 
		{
			get	{
				return Color.FromArgb( 255, 32, 178, 170 );
			}
		}
		static public Color LightSkyBlue 
		{
			get	{
				return Color.FromArgb( 255, 135, 206, 250 );
			}
		}
		static public Color LightSlateGray 
		{
			get	{
				return Color.FromArgb( 255, 119, 136, 153 );
			}
		}
		static public Color LightSteelBlue 
		{
			get	{
				return Color.FromArgb( 255, 176, 196, 222 );
			}
		}
		static public Color LightYellow 
		{
			get	{
				return Color.FromArgb( 255, 255, 255, 224 );
			}
		}
		static public Color Lime 
		{
			get	{
				return Color.FromArgb( 255, 0, 255, 0 );
			}
		}
		static public Color LimeGreen 
		{
			get	{
				return Color.FromArgb( 255, 50, 205, 50 );
			}
		}
		static public Color Linen 
		{
			get	{
				return Color.FromArgb( 255, 250, 240, 230 );
			}
		}
		static public Color Magenta 
		{
			get	{
				return Color.FromArgb( 255, 255, 0, 255 );
			}
		}
		static public Color Maroon 
		{
			get	{
				return Color.FromArgb( 255, 128, 0, 0 );
			}
		}
		static public Color MediumAquamarine 
		{
			get	{
				return Color.FromArgb( 255, 102, 205, 170 );
			}
		}
		static public Color MediumBlue 
		{
			get	{
				return Color.FromArgb( 255, 0, 0, 205 );
			}
		}
		static public Color MediumOrchid 
		{
			get	{
				return Color.FromArgb( 255, 186, 85, 211 );
			}
		}
		static public Color MediumPurple 
		{
			get	{
				return Color.FromArgb( 255, 147, 112, 219 );
			}
		}
		static public Color MediumSeaGreen 
		{
			get	{
				return Color.FromArgb( 255, 60, 179, 113 );
			}
		}
		static public Color MediumSlateBlue 
		{
			get	{
				return Color.FromArgb( 255, 123, 104, 238 );
			}
		}
		static public Color MediumSpringGreen 
		{
			get	{
				return Color.FromArgb( 255, 0, 250, 154 );
			}
		}
		static public Color MediumTurquoise 
		{
			get	{
				return Color.FromArgb( 255, 72, 209, 204 );
			}
		}
		static public Color MediumVioletRed 
		{
			get	{
				return Color.FromArgb( 255, 199, 21, 133 );
			}
		}
		static public Color MidnightBlue 
		{
			get	{
				return Color.FromArgb( 255, 25, 25, 112 );
			}
		}
		static public Color MintCream 
		{
			get	{
				return Color.FromArgb( 255, 245, 255, 250 );
			}
		}
		static public Color MistyRose 
		{
			get	{
				return Color.FromArgb( 255, 255, 228, 225 );
			}
		}
		static public Color Moccasin 
		{
			get	{
				return Color.FromArgb( 255, 255, 228, 181 );
			}
		}
		static public Color NavajoWhite 
		{
			get	{
				return Color.FromArgb( 255, 255, 222, 173 );
			}
		}
		static public Color Navy 
		{
			get	{
				return Color.FromArgb( 255, 0, 0, 128 );
			}
		}
		static public Color OldLace 
		{
			get	{
				return Color.FromArgb( 255, 253, 245, 230 );
			}
		}
		static public Color Olive 
		{
			get	{
				return Color.FromArgb( 255, 128, 128, 0 );
			}
		}
		static public Color OliveDrab 
		{
			get	{
				return Color.FromArgb( 255, 107, 142, 35 );
			}
		}
		static public Color Orange 
		{
			get	{
				return Color.FromArgb( 255, 255, 165, 0 );
			}
		}
		static public Color OrangeRed 
		{
			get	{
				return Color.FromArgb( 255, 255, 69, 0 );
			}
		}
		static public Color Orchid 
		{
			get	{
				return Color.FromArgb( 255, 218, 112, 214 );
			}
		}
		static public Color PaleGoldenrod 
		{
			get	{
				return Color.FromArgb( 255, 238, 232, 170 );
			}
		}
		static public Color PaleGreen 
		{
			get	{
				return Color.FromArgb( 255, 152, 251, 152 );
			}
		}
		static public Color PaleTurquoise 
		{
			get	{
				return Color.FromArgb( 255, 175, 238, 238 );
			}
		}
		static public Color PaleVioletRed 
		{
			get	{
				return Color.FromArgb( 255, 219, 112, 147 );
			}
		}
		static public Color PapayaWhip 
		{
			get	{
				return Color.FromArgb( 255, 255, 239, 213 );
			}
		}
		static public Color PeachPuff 
		{
			get	{
				return Color.FromArgb( 255, 255, 218, 185 );
			}
		}
		static public Color Peru 
		{
			get	{
				return Color.FromArgb( 255, 205, 133, 63 );
			}
		}
		static public Color Pink 
		{
			get	{
				return Color.FromArgb( 255, 255, 192, 203 );
			}
		}
		static public Color Plum 
		{
			get	{
				return Color.FromArgb( 255, 221, 160, 221 );
			}
		}
		static public Color PowderBlue 
		{
			get	{
				return Color.FromArgb( 255, 176, 224, 230 );
			}
		}
		static public Color Purple 
		{
			get	{
				return Color.FromArgb( 255, 128, 0, 128 );
			}
		}
		static public Color Red 
		{
			get	{
				return Color.FromArgb( 255, 255, 0, 0 );
			}
		}
		static public Color RosyBrown 
		{
			get	{
				return Color.FromArgb( 255, 188, 143, 143 );
			}
		}
		static public Color RoyalBlue 
		{
			get	{
				return Color.FromArgb( 255, 65, 105, 225 );
			}
		}
		static public Color SaddleBrown 
		{
			get	{
				return Color.FromArgb( 255, 139, 69, 19 );
			}
		}
		static public Color Salmon 
		{
			get	{
				return Color.FromArgb( 255, 250, 128, 114 );
			}
		}
		static public Color SandyBrown 
		{
			get	{
				return Color.FromArgb( 255, 244, 164, 96 );
			}
		}
		static public Color SeaGreen 
		{
			get	{
				return Color.FromArgb( 255, 46, 139, 87 );
			}
		}
		static public Color SeaShell 
		{
			get	{
				return Color.FromArgb( 255, 255, 245, 238 );
			}
		}
		static public Color Sienna 
		{
			get	{
				return Color.FromArgb( 255, 160, 82, 45 );
			}
		}
		static public Color Silver 
		{
			get	{
				return Color.FromArgb( 255, 192, 192, 192 );
			}
		}
		static public Color SkyBlue 
		{
			get	{
				return Color.FromArgb( 255, 135, 206, 235 );
			}
		}
		static public Color SlateBlue 
		{
			get	{
				return Color.FromArgb( 255, 106, 90, 205 );
			}
		}
		static public Color SlateGray 
		{
			get	{
				return Color.FromArgb( 255, 112, 128, 144 );
			}
		}
		static public Color Snow 
		{
			get	{
				return Color.FromArgb( 255, 255, 250, 250 );
			}
		}
		static public Color SpringGreen 
		{
			get	{
				return Color.FromArgb( 255, 0, 255, 127 );
			}
		}
		static public Color SteelBlue 
		{
			get	{
				return Color.FromArgb( 255, 70, 130, 180 );
			}
		}
		static public Color Tan 
		{
			get	{
				return Color.FromArgb( 255, 210, 180, 140 );
			}
		}
		static public Color Teal 
		{
			get	{
				return Color.FromArgb( 255, 0, 128, 128 );
			}
		}
		static public Color Thistle 
		{
			get	{
				return Color.FromArgb( 255, 216, 191, 216 );
			}
		}
		static public Color Tomato 
		{
			get	{
				return Color.FromArgb( 255, 255, 99, 71 );
			}
		}
		static public Color Turquoise 
		{
			get	{
				return Color.FromArgb( 255, 64, 224, 208 );
			}
		}
		static public Color Violet 
		{
			get	{
				return Color.FromArgb( 255, 238, 130, 238 );
			}
		}
		static public Color Wheat 
		{
			get	{
				return Color.FromArgb( 255, 245, 222, 179 );
			}
		}
		static public Color White 
		{
			get	{
				return Color.FromArgb( 255, 255, 255, 255 );
			}
		}
		static public Color WhiteSmoke 
		{
			get	{
				return Color.FromArgb( 255, 245, 245, 245 );
			}
		}
		static public Color Yellow 
		{
			get	{
				return Color.FromArgb( 255, 255, 255, 0 );
			}
		}
		static public Color YellowGreen 
		{
			get	{
				return Color.FromArgb( 255, 154, 205, 50 );
			}
		}
	}
}

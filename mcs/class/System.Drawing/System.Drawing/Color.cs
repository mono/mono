//
// System.Drawing.Color.cs
//
// Authors:
// 	Dennis Hayes (dennish@raytek.com)
// 	Ben Houston  (ben@exocortex.org)
// 	Gonzalo Paniagua (gonzalo@ximian.com)
//
// (C) 2002 Dennis Hayes
// (c) 2002 Ximian, Inc. (http://www.ximiam.com)
//
// TODO: Are the static/non static functions declared correctly

using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace System.Drawing 
{
	[TypeConverter(typeof(ColorConverter))]
	[Serializable]
	public struct Color
	{
		private static Hashtable namedColors;
		private static Hashtable systemColors;
		// Private transparancy (A) and R,G,B fields.
		byte a;
		byte r;
		byte g;
		byte b;
		private static string creatingColorNames = "creatingColorNames";

		// The specs also indicate that all three of these propities are true
		// if created with FromKnownColor or FromNamedColor, false otherwise (FromARGB).
		// Per Microsoft and ECMA specs these varibles are set by which constructor is used, not by their values.
		bool isknowncolor;
		bool isnamedcolor;
		bool issystemcolor;
		KnownColor knownColor;

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
				if (!isnamedcolor)
					return IsKnownColor;
				return isnamedcolor;
			}
		}


		public static Color FromArgb (int red, int green, int blue)
		{
			return FromArgb (255, red, green, blue);
		}
		
		public static Color FromArgb (int alpha, int red, int green, int blue)
		{
			CheckARGBValues (alpha, red, green, blue);
			Color color = new Color ();
			color.a = (byte) alpha;
			color.r = (byte) red;
			color.g = (byte) green;
			color.b = (byte) blue;
			color.myname = String.Empty;
			return color;
		}

		private static Color FromArgbNamed (int alpha, int red, int green, int blue, string name, KnownColor knownColor)
		{
			Color color = FromArgb (alpha, red, green, blue);
			color.isknowncolor = true;
			color.isnamedcolor = true;
			//color.issystemcolor = false; //???
			color.myname = name;
			// FIXME: here happens SEGFAULT.
			//color.knownColor = (KnownColor) Enum.Parse (typeof (KnownColor), name, false);
			color.knownColor = knownColor;
			return color;
		}

		internal static Color FromArgbSystem (int alpha, int red, int green, int blue, string name, KnownColor knownColor)
		{
			Color color = FromArgbNamed (alpha, red, green, blue, name, knownColor);
			color.issystemcolor = true;
			return color;
		}

		public int ToArgb()
		{
			return a << 24 | r << 16 | g << 8 | b;
		} 

		public static Color FromArgb (int alpha, Color baseColor)
		{
			return FromArgb (alpha, baseColor.r, baseColor.g, baseColor.b);
		}

		public static Color FromArgb (int argb)
		{
			return FromArgb (argb >> 24, (argb >> 16) & 0x0FF, (argb >> 8) & 0x0FF, argb & 0x0FF);
		}

		public static Color FromKnownColor (KnownColor knownColorToConvert)
		{
			Color c = FromName (knownColorToConvert.ToString ());
			c.knownColor = knownColorToConvert;
			return c;
		}

		private static Hashtable GetColorHashtableFromType (Type type)
		{
			Hashtable colorHash = new Hashtable (CaseInsensitiveHashCodeProvider.Default,
							     CaseInsensitiveComparer.Default);

			PropertyInfo [] props = type.GetProperties ();
			foreach (PropertyInfo prop in props){
				if (prop.PropertyType != typeof (Color))
					continue;

				MethodInfo getget = prop.GetGetMethod ();
				if (getget == null || getget.IsStatic == false)
					continue;

				colorHash.Add (prop.Name, prop.GetValue (null, null));
			}
			return colorHash;
		}

		private static void FillColorNames ()
		{
			if (systemColors != null)
				return;

			lock (creatingColorNames) {
				if (systemColors != null)
					return;
				
				Hashtable colorHash = GetColorHashtableFromType (typeof (Color));
				namedColors = colorHash;

				colorHash = GetColorHashtableFromType (typeof (SystemColors));
				systemColors = colorHash;
			}
		}
		
		public static Color FromName (string colorName)
		{
			object c = NamedColors [colorName];
			if (c == null) {
				c = SystemColors [colorName];
				if (c == null) {
					// This is what it returns!
					Color d = FromArgb (0, 0, 0, 0);
					d.myname = colorName;
					d.isnamedcolor = true;
					c = d;
				}
			}

			return (Color) c;
		}

		internal static Hashtable NamedColors
		{
			get {
				FillColorNames ();
				return namedColors;
			}
		}

		internal static Hashtable SystemColors
		{
			get {
				FillColorNames ();
				return systemColors;
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
			return knownColor;
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
			if (!(o is Color))
				return false;

			Color c = (Color) o;
			if (c.r == r && c.g == g && c.b == b) {
				if (myname != null || c.myname != null)
					return (myname == c.myname);
				return true;
			}
			return false;
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
			if (myname != "")
				return "Color [" + myname + "]";

			return String.Format ("Color [A={0}, R={1}, G={2}, B={3}]", a, r, g, b);
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
		//using System;
		//using System.Diagnostics;
		//using System.Drawing;
		//using System.Reflection;
		//public class m {
		//static void Main(string[] args)
		//{
		//	Type cType = typeof (Color);
		//	PropertyInfo [] properties = cType.GetProperties ();
		//	foreach (PropertyInfo property in properties) {
		//		MethodInfo method = property.GetGetMethod();
		//		if (method != null && method.IsStatic && method.ReturnType == cType) {
		//			Color c = (Color) method.Invoke( null, new object[0] );
		//			Console.WriteLine("static public Color " + property.Name);
		//			Console.WriteLine("{\t\n\tget {");
		//			Console.WriteLine("\t\treturn Color.FromArgbNamed ({0}, {1}, {2}, {3}, \"{4}\");",
		//						c.A, c.R, c.G, c.B, property.Name);
		//			Console.WriteLine("\t}");
		//			Console.WriteLine("}\n");
		//		}
		//	}
		//}
		//}

		static public Color Transparent
		{	
			get {
				return Color.FromArgbNamed (0, 255, 255, 255, "Transparent", KnownColor.Transparent);
			}
		}

		static public Color AliceBlue
		{	
			get {
				return Color.FromArgbNamed (255, 240, 248, 255, "AliceBlue", KnownColor.AliceBlue);
			}
		}

		static public Color AntiqueWhite
		{	
			get {
				return Color.FromArgbNamed (255, 250, 235, 215, "AntiqueWhite", KnownColor.AntiqueWhite);
			}
		}

		static public Color Aqua
		{	
			get {
				return Color.FromArgbNamed (255, 0, 255, 255, "Aqua", KnownColor.Aqua);
			}
		}

		static public Color Aquamarine
		{	
			get {
				return Color.FromArgbNamed (255, 127, 255, 212, "Aquamarine", KnownColor.Aquamarine);
			}
		}

		static public Color Azure
		{	
			get {
				return Color.FromArgbNamed (255, 240, 255, 255, "Azure", KnownColor.Azure);
			}
		}

		static public Color Beige
		{	
			get {
				return Color.FromArgbNamed (255, 245, 245, 220, "Beige", KnownColor.Beige);
			}
		}

		static public Color Bisque
		{	
			get {
				return Color.FromArgbNamed (255, 255, 228, 196, "Bisque", KnownColor.Bisque);
			}
		}

		static public Color Black
		{	
			get {
				return Color.FromArgbNamed (255, 0, 0, 0, "Black", KnownColor.Black);
			}
		}

		static public Color BlanchedAlmond
		{	
			get {
				return Color.FromArgbNamed (255, 255, 235, 205, "BlanchedAlmond", KnownColor.BlanchedAlmond);
			}
		}

		static public Color Blue
		{	
			get {
				return Color.FromArgbNamed (255, 0, 0, 255, "Blue", KnownColor.Blue);
			}
		}

		static public Color BlueViolet
		{	
			get {
				return Color.FromArgbNamed (255, 138, 43, 226, "BlueViolet", KnownColor.BlueViolet);
			}
		}

		static public Color Brown
		{	
			get {
				return Color.FromArgbNamed (255, 165, 42, 42, "Brown", KnownColor.Brown);
			}
		}

		static public Color BurlyWood
		{	
			get {
				return Color.FromArgbNamed (255, 222, 184, 135, "BurlyWood", KnownColor.BurlyWood);
			}
		}

		static public Color CadetBlue
		{	
			get {
				return Color.FromArgbNamed (255, 95, 158, 160, "CadetBlue", KnownColor.CadetBlue);
			}
		}

		static public Color Chartreuse
		{	
			get {
				return Color.FromArgbNamed (255, 127, 255, 0, "Chartreuse", KnownColor.Chartreuse);
			}
		}

		static public Color Chocolate
		{	
			get {
				return Color.FromArgbNamed (255, 210, 105, 30, "Chocolate", KnownColor.Chocolate);
			}
		}

		static public Color Coral
		{	
			get {
				return Color.FromArgbNamed (255, 255, 127, 80, "Coral", KnownColor.Coral);
			}
		}

		static public Color CornflowerBlue
		{	
			get {
				return Color.FromArgbNamed (255, 100, 149, 237, "CornflowerBlue", KnownColor.CornflowerBlue);
			}
		}

		static public Color Cornsilk
		{	
			get {
				return Color.FromArgbNamed (255, 255, 248, 220, "Cornsilk", KnownColor.Cornsilk);
			}
		}

		static public Color Crimson
		{	
			get {
				return Color.FromArgbNamed (255, 220, 20, 60, "Crimson", KnownColor.Crimson);
			}
		}

		static public Color Cyan
		{	
			get {
				return Color.FromArgbNamed (255, 0, 255, 255, "Cyan", KnownColor.Cyan);
			}
		}

		static public Color DarkBlue
		{	
			get {
				return Color.FromArgbNamed (255, 0, 0, 139, "DarkBlue", KnownColor.DarkBlue);
			}
		}

		static public Color DarkCyan
		{	
			get {
				return Color.FromArgbNamed (255, 0, 139, 139, "DarkCyan", KnownColor.DarkCyan);
			}
		}

		static public Color DarkGoldenrod
		{	
			get {
				return Color.FromArgbNamed (255, 184, 134, 11, "DarkGoldenrod", KnownColor.DarkGoldenrod);
			}
		}

		static public Color DarkGray
		{	
			get {
				return Color.FromArgbNamed (255, 169, 169, 169, "DarkGray", KnownColor.DarkGray);
			}
		}

		static public Color DarkGreen
		{	
			get {
				return Color.FromArgbNamed (255, 0, 100, 0, "DarkGreen", KnownColor.DarkGreen);
			}
		}

		static public Color DarkKhaki
		{	
			get {
				return Color.FromArgbNamed (255, 189, 183, 107, "DarkKhaki", KnownColor.DarkKhaki);
			}
		}

		static public Color DarkMagenta
		{	
			get {
				return Color.FromArgbNamed (255, 139, 0, 139, "DarkMagenta", KnownColor.DarkMagenta);
			}
		}

		static public Color DarkOliveGreen
		{	
			get {
				return Color.FromArgbNamed (255, 85, 107, 47, "DarkOliveGreen", KnownColor.DarkOliveGreen);
			}
		}

		static public Color DarkOrange
		{	
			get {
				return Color.FromArgbNamed (255, 255, 140, 0, "DarkOrange", KnownColor.DarkOrange);
			}
		}

		static public Color DarkOrchid
		{	
			get {
				return Color.FromArgbNamed (255, 153, 50, 204, "DarkOrchid", KnownColor.DarkOrchid);
			}
		}

		static public Color DarkRed
		{	
			get {
				return Color.FromArgbNamed (255, 139, 0, 0, "DarkRed", KnownColor.DarkRed);
			}
		}

		static public Color DarkSalmon
		{	
			get {
				return Color.FromArgbNamed (255, 233, 150, 122, "DarkSalmon", KnownColor.DarkSalmon);
			}
		}

		static public Color DarkSeaGreen
		{	
			get {
				return Color.FromArgbNamed (255, 143, 188, 139, "DarkSeaGreen", KnownColor.DarkSeaGreen);
			}
		}

		static public Color DarkSlateBlue
		{	
			get {
				return Color.FromArgbNamed (255, 72, 61, 139, "DarkSlateBlue", KnownColor.DarkSlateBlue);
			}
		}

		static public Color DarkSlateGray
		{	
			get {
				return Color.FromArgbNamed (255, 47, 79, 79, "DarkSlateGray", KnownColor.DarkSlateGray);
			}
		}

		static public Color DarkTurquoise
		{	
			get {
				return Color.FromArgbNamed (255, 0, 206, 209, "DarkTurquoise", KnownColor.DarkTurquoise);
			}
		}

		static public Color DarkViolet
		{	
			get {
				return Color.FromArgbNamed (255, 148, 0, 211, "DarkViolet", KnownColor.DarkViolet);
			}
		}

		static public Color DeepPink
		{	
			get {
				return Color.FromArgbNamed (255, 255, 20, 147, "DeepPink", KnownColor.DeepPink);
			}
		}

		static public Color DeepSkyBlue
		{	
			get {
				return Color.FromArgbNamed (255, 0, 191, 255, "DeepSkyBlue", KnownColor.DeepSkyBlue);
			}
		}

		static public Color DimGray
		{	
			get {
				return Color.FromArgbNamed (255, 105, 105, 105, "DimGray", KnownColor.DimGray);
			}
		}

		static public Color DodgerBlue
		{	
			get {
				return Color.FromArgbNamed (255, 30, 144, 255, "DodgerBlue", KnownColor.DodgerBlue);
			}
		}

		static public Color Firebrick
		{	
			get {
				return Color.FromArgbNamed (255, 178, 34, 34, "Firebrick", KnownColor.Firebrick);
			}
		}

		static public Color FloralWhite
		{	
			get {
				return Color.FromArgbNamed (255, 255, 250, 240, "FloralWhite", KnownColor.FloralWhite);
			}
		}

		static public Color ForestGreen
		{	
			get {
				return Color.FromArgbNamed (255, 34, 139, 34, "ForestGreen", KnownColor.ForestGreen);
			}
		}

		static public Color Fuchsia
		{	
			get {
				return Color.FromArgbNamed (255, 255, 0, 255, "Fuchsia", KnownColor.Fuchsia);
			}
		}

		static public Color Gainsboro
		{	
			get {
				return Color.FromArgbNamed (255, 220, 220, 220, "Gainsboro", KnownColor.Gainsboro);
			}
		}

		static public Color GhostWhite
		{	
			get {
				return Color.FromArgbNamed (255, 248, 248, 255, "GhostWhite", KnownColor.GhostWhite);
			}
		}

		static public Color Gold
		{	
			get {
				return Color.FromArgbNamed (255, 255, 215, 0, "Gold", KnownColor.Gold);
			}
		}

		static public Color Goldenrod
		{	
			get {
				return Color.FromArgbNamed (255, 218, 165, 32, "Goldenrod", KnownColor.Goldenrod);
			}
		}

		static public Color Gray
		{	
			get {
				return Color.FromArgbNamed (255, 128, 128, 128, "Gray", KnownColor.Gray);
			}
		}

		static public Color Green
		{	
			get {
				return Color.FromArgbNamed (255, 0, 128, 0, "Green", KnownColor.Green);
			}
		}

		static public Color GreenYellow
		{	
			get {
				return Color.FromArgbNamed (255, 173, 255, 47, "GreenYellow", KnownColor.GreenYellow);
			}
		}

		static public Color Honeydew
		{	
			get {
				return Color.FromArgbNamed (255, 240, 255, 240, "Honeydew", KnownColor.Honeydew);
			}
		}

		static public Color HotPink
		{	
			get {
				return Color.FromArgbNamed (255, 255, 105, 180, "HotPink", KnownColor.HotPink);
			}
		}

		static public Color IndianRed
		{	
			get {
				return Color.FromArgbNamed (255, 205, 92, 92, "IndianRed", KnownColor.IndianRed);
			}
		}

		static public Color Indigo
		{	
			get {
				return Color.FromArgbNamed (255, 75, 0, 130, "Indigo", KnownColor.Indigo);
			}
		}

		static public Color Ivory
		{	
			get {
				return Color.FromArgbNamed (255, 255, 255, 240, "Ivory", KnownColor.Ivory);
			}
		}

		static public Color Khaki
		{	
			get {
				return Color.FromArgbNamed (255, 240, 230, 140, "Khaki", KnownColor.Khaki);
			}
		}

		static public Color Lavender
		{	
			get {
				return Color.FromArgbNamed (255, 230, 230, 250, "Lavender", KnownColor.Lavender);
			}
		}

		static public Color LavenderBlush
		{	
			get {
				return Color.FromArgbNamed (255, 255, 240, 245, "LavenderBlush", KnownColor.LavenderBlush);
			}
		}

		static public Color LawnGreen
		{	
			get {
				return Color.FromArgbNamed (255, 124, 252, 0, "LawnGreen", KnownColor.LawnGreen);
			}
		}

		static public Color LemonChiffon
		{	
			get {
				return Color.FromArgbNamed (255, 255, 250, 205, "LemonChiffon", KnownColor.LemonChiffon);
			}
		}

		static public Color LightBlue
		{	
			get {
				return Color.FromArgbNamed (255, 173, 216, 230, "LightBlue", KnownColor.LightBlue);
			}
		}

		static public Color LightCoral
		{	
			get {
				return Color.FromArgbNamed (255, 240, 128, 128, "LightCoral", KnownColor.LightCoral);
			}
		}

		static public Color LightCyan
		{	
			get {
				return Color.FromArgbNamed (255, 224, 255, 255, "LightCyan", KnownColor.LightCyan);
			}
		}

		static public Color LightGoldenrodYellow
		{	
			get {
				return Color.FromArgbNamed (255, 250, 250, 210, "LightGoldenrodYellow", KnownColor.LightGoldenrodYellow);
			}
		}

		static public Color LightGreen
		{	
			get {
				return Color.FromArgbNamed (255, 144, 238, 144, "LightGreen", KnownColor.LightGreen);
			}
		}

		static public Color LightGray
		{	
			get {
				return Color.FromArgbNamed (255, 211, 211, 211, "LightGray", KnownColor.LightGray);
			}
		}

		static public Color LightPink
		{	
			get {
				return Color.FromArgbNamed (255, 255, 182, 193, "LightPink", KnownColor.LightPink);
			}
		}

		static public Color LightSalmon
		{	
			get {
				return Color.FromArgbNamed (255, 255, 160, 122, "LightSalmon", KnownColor.LightSalmon);
			}
		}

		static public Color LightSeaGreen
		{	
			get {
				return Color.FromArgbNamed (255, 32, 178, 170, "LightSeaGreen", KnownColor.LightSeaGreen);
			}
		}

		static public Color LightSkyBlue
		{	
			get {
				return Color.FromArgbNamed (255, 135, 206, 250, "LightSkyBlue", KnownColor.LightSkyBlue);
			}
		}

		static public Color LightSlateGray
		{	
			get {
				return Color.FromArgbNamed (255, 119, 136, 153, "LightSlateGray", KnownColor.LightSlateGray);
			}
		}

		static public Color LightSteelBlue
		{	
			get {
				return Color.FromArgbNamed (255, 176, 196, 222, "LightSteelBlue", KnownColor.LightSteelBlue);
			}
		}

		static public Color LightYellow
		{	
			get {
				return Color.FromArgbNamed (255, 255, 255, 224, "LightYellow", KnownColor.LightYellow);
			}
		}

		static public Color Lime
		{	
			get {
				return Color.FromArgbNamed (255, 0, 255, 0, "Lime", KnownColor.Lime);
			}
		}

		static public Color LimeGreen
		{	
			get {
				return Color.FromArgbNamed (255, 50, 205, 50, "LimeGreen", KnownColor.LimeGreen);
			}
		}

		static public Color Linen
		{	
			get {
				return Color.FromArgbNamed (255, 250, 240, 230, "Linen", KnownColor.Linen);
			}
		}

		static public Color Magenta
		{	
			get {
				return Color.FromArgbNamed (255, 255, 0, 255, "Magenta", KnownColor.Magenta);
			}
		}

		static public Color Maroon
		{	
			get {
				return Color.FromArgbNamed (255, 128, 0, 0, "Maroon", KnownColor.Maroon);
			}
		}

		static public Color MediumAquamarine
		{	
			get {
				return Color.FromArgbNamed (255, 102, 205, 170, "MediumAquamarine", KnownColor.MediumAquamarine);
			}
		}

		static public Color MediumBlue
		{	
			get {
				return Color.FromArgbNamed (255, 0, 0, 205, "MediumBlue", KnownColor.MediumBlue);
			}
		}

		static public Color MediumOrchid
		{	
			get {
				return Color.FromArgbNamed (255, 186, 85, 211, "MediumOrchid", KnownColor.MediumOrchid);
			}
		}

		static public Color MediumPurple
		{	
			get {
				return Color.FromArgbNamed (255, 147, 112, 219, "MediumPurple", KnownColor.MediumPurple);
			}
		}

		static public Color MediumSeaGreen
		{	
			get {
				return Color.FromArgbNamed (255, 60, 179, 113, "MediumSeaGreen", KnownColor.MediumSeaGreen);
			}
		}

		static public Color MediumSlateBlue
		{	
			get {
				return Color.FromArgbNamed (255, 123, 104, 238, "MediumSlateBlue", KnownColor.MediumSlateBlue);
			}
		}

		static public Color MediumSpringGreen
		{	
			get {
				return Color.FromArgbNamed (255, 0, 250, 154, "MediumSpringGreen", KnownColor.MediumSpringGreen);
			}
		}

		static public Color MediumTurquoise
		{	
			get {
				return Color.FromArgbNamed (255, 72, 209, 204, "MediumTurquoise", KnownColor.MediumTurquoise);
			}
		}

		static public Color MediumVioletRed
		{	
			get {
				return Color.FromArgbNamed (255, 199, 21, 133, "MediumVioletRed", KnownColor.MediumVioletRed);
			}
		}

		static public Color MidnightBlue
		{	
			get {
				return Color.FromArgbNamed (255, 25, 25, 112, "MidnightBlue", KnownColor.MidnightBlue);
			}
		}

		static public Color MintCream
		{	
			get {
				return Color.FromArgbNamed (255, 245, 255, 250, "MintCream", KnownColor.MintCream);
			}
		}

		static public Color MistyRose
		{	
			get {
				return Color.FromArgbNamed (255, 255, 228, 225, "MistyRose", KnownColor.MistyRose);
			}
		}

		static public Color Moccasin
		{	
			get {
				return Color.FromArgbNamed (255, 255, 228, 181, "Moccasin", KnownColor.Moccasin);
			}
		}

		static public Color NavajoWhite
		{	
			get {
				return Color.FromArgbNamed (255, 255, 222, 173, "NavajoWhite", KnownColor.NavajoWhite);
			}
		}

		static public Color Navy
		{	
			get {
				return Color.FromArgbNamed (255, 0, 0, 128, "Navy", KnownColor.Navy);
			}
		}

		static public Color OldLace
		{	
			get {
				return Color.FromArgbNamed (255, 253, 245, 230, "OldLace", KnownColor.OldLace);
			}
		}

		static public Color Olive
		{	
			get {
				return Color.FromArgbNamed (255, 128, 128, 0, "Olive", KnownColor.Olive);
			}
		}

		static public Color OliveDrab
		{	
			get {
				return Color.FromArgbNamed (255, 107, 142, 35, "OliveDrab", KnownColor.OliveDrab);
			}
		}

		static public Color Orange
		{	
			get {
				return Color.FromArgbNamed (255, 255, 165, 0, "Orange", KnownColor.Orange);
			}
		}

		static public Color OrangeRed
		{	
			get {
				return Color.FromArgbNamed (255, 255, 69, 0, "OrangeRed", KnownColor.OrangeRed);
			}
		}

		static public Color Orchid
		{	
			get {
				return Color.FromArgbNamed (255, 218, 112, 214, "Orchid", KnownColor.Orchid);
			}
		}

		static public Color PaleGoldenrod
		{	
			get {
				return Color.FromArgbNamed (255, 238, 232, 170, "PaleGoldenrod", KnownColor.PaleGoldenrod);
			}
		}

		static public Color PaleGreen
		{	
			get {
				return Color.FromArgbNamed (255, 152, 251, 152, "PaleGreen", KnownColor.PaleGreen);
			}
		}

		static public Color PaleTurquoise
		{	
			get {
				return Color.FromArgbNamed (255, 175, 238, 238, "PaleTurquoise", KnownColor.PaleTurquoise);
			}
		}

		static public Color PaleVioletRed
		{	
			get {
				return Color.FromArgbNamed (255, 219, 112, 147, "PaleVioletRed", KnownColor.PaleVioletRed);
			}
		}

		static public Color PapayaWhip
		{	
			get {
				return Color.FromArgbNamed (255, 255, 239, 213, "PapayaWhip", KnownColor.PapayaWhip);
			}
		}

		static public Color PeachPuff
		{	
			get {
				return Color.FromArgbNamed (255, 255, 218, 185, "PeachPuff", KnownColor.PeachPuff);
			}
		}

		static public Color Peru
		{	
			get {
				return Color.FromArgbNamed (255, 205, 133, 63, "Peru", KnownColor.Peru);
			}
		}

		static public Color Pink
		{	
			get {
				return Color.FromArgbNamed (255, 255, 192, 203, "Pink", KnownColor.Pink);
			}
		}

		static public Color Plum
		{	
			get {
				return Color.FromArgbNamed (255, 221, 160, 221, "Plum", KnownColor.Plum);
			}
		}

		static public Color PowderBlue
		{	
			get {
				return Color.FromArgbNamed (255, 176, 224, 230, "PowderBlue", KnownColor.PowderBlue);
			}
		}

		static public Color Purple
		{	
			get {
				return Color.FromArgbNamed (255, 128, 0, 128, "Purple", KnownColor.Purple);
			}
		}

		static public Color Red
		{	
			get {
				return Color.FromArgbNamed (255, 255, 0, 0, "Red", KnownColor.Red);
			}
		}

		static public Color RosyBrown
		{	
			get {
				return Color.FromArgbNamed (255, 188, 143, 143, "RosyBrown", KnownColor.RosyBrown);
			}
		}

		static public Color RoyalBlue
		{	
			get {
				return Color.FromArgbNamed (255, 65, 105, 225, "RoyalBlue", KnownColor.RoyalBlue);
			}
		}

		static public Color SaddleBrown
		{	
			get {
				return Color.FromArgbNamed (255, 139, 69, 19, "SaddleBrown", KnownColor.SaddleBrown);
			}
		}

		static public Color Salmon
		{	
			get {
				return Color.FromArgbNamed (255, 250, 128, 114, "Salmon", KnownColor.Salmon);
			}
		}

		static public Color SandyBrown
		{	
			get {
				return Color.FromArgbNamed (255, 244, 164, 96, "SandyBrown", KnownColor.SandyBrown);
			}
		}

		static public Color SeaGreen
		{	
			get {
				return Color.FromArgbNamed (255, 46, 139, 87, "SeaGreen", KnownColor.SeaGreen);
			}
		}

		static public Color SeaShell
		{	
			get {
				return Color.FromArgbNamed (255, 255, 245, 238, "SeaShell", KnownColor.SeaShell);
			}
		}

		static public Color Sienna
		{	
			get {
				return Color.FromArgbNamed (255, 160, 82, 45, "Sienna", KnownColor.Sienna);
			}
		}

		static public Color Silver
		{	
			get {
				return Color.FromArgbNamed (255, 192, 192, 192, "Silver", KnownColor.Silver);
			}
		}

		static public Color SkyBlue
		{	
			get {
				return Color.FromArgbNamed (255, 135, 206, 235, "SkyBlue", KnownColor.SkyBlue);
			}
		}

		static public Color SlateBlue
		{	
			get {
				return Color.FromArgbNamed (255, 106, 90, 205, "SlateBlue", KnownColor.SlateBlue);
			}
		}

		static public Color SlateGray
		{	
			get {
				return Color.FromArgbNamed (255, 112, 128, 144, "SlateGray", KnownColor.SlateGray);
			}
		}

		static public Color Snow
		{	
			get {
				return Color.FromArgbNamed (255, 255, 250, 250, "Snow", KnownColor.Snow);
			}
		}

		static public Color SpringGreen
		{	
			get {
				return Color.FromArgbNamed (255, 0, 255, 127, "SpringGreen", KnownColor.SpringGreen);
			}
		}

		static public Color SteelBlue
		{	
			get {
				return Color.FromArgbNamed (255, 70, 130, 180, "SteelBlue", KnownColor.SteelBlue);
			}
		}

		static public Color Tan
		{	
			get {
				return Color.FromArgbNamed (255, 210, 180, 140, "Tan", KnownColor.Tan);
			}
		}

		static public Color Teal
		{	
			get {
				return Color.FromArgbNamed (255, 0, 128, 128, "Teal", KnownColor.Teal);
			}
		}

		static public Color Thistle
		{	
			get {
				return Color.FromArgbNamed (255, 216, 191, 216, "Thistle", KnownColor.Thistle);
			}
		}

		static public Color Tomato
		{	
			get {
				return Color.FromArgbNamed (255, 255, 99, 71, "Tomato", KnownColor.Tomato);
			}
		}

		static public Color Turquoise
		{	
			get {
				return Color.FromArgbNamed (255, 64, 224, 208, "Turquoise", KnownColor.Turquoise);
			}
		}

		static public Color Violet
		{	
			get {
				return Color.FromArgbNamed (255, 238, 130, 238, "Violet", KnownColor.Violet);
			}
		}

		static public Color Wheat
		{	
			get {
				return Color.FromArgbNamed (255, 245, 222, 179, "Wheat", KnownColor.Wheat);
			}
		}

		static public Color White
		{	
			get {
				return Color.FromArgbNamed (255, 255, 255, 255, "White", KnownColor.White);
			}
		}

		static public Color WhiteSmoke
		{	
			get {
				return Color.FromArgbNamed (255, 245, 245, 245, "WhiteSmoke", KnownColor.WhiteSmoke);
			}
		}

		static public Color Yellow
		{	
			get {
				return Color.FromArgbNamed (255, 255, 255, 0, "Yellow", KnownColor.Yellow);
			}
		}

		static public Color YellowGreen
		{	
			get {
				return Color.FromArgbNamed (255, 154, 205, 50, "YellowGreen", KnownColor.YellowGreen);
			}
		}
	}
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// ------------------------------------------------------------------------------
// Changes to this file must follow the http://aka.ms/api-review process.
// ------------------------------------------------------------------------------

// MONO: these files were copied from CoreFX (System.Drawing.Primitives/ref and System.Drawing.Common/ref)

// System.Drawing.Primitives/ref:
namespace System.Drawing
{

#if !MONODROID

    public readonly partial struct Color : System.IEquatable<System.Drawing.Color>
    {
        private readonly object _dummy;
        private readonly int _dummyPrimitive;
        public static readonly System.Drawing.Color Empty;
        public byte A { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color AliceBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color AntiqueWhite { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Aqua { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Aquamarine { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Azure { get { throw new PlatformNotSupportedException(); } }
        public byte B { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Beige { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Bisque { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Black { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color BlanchedAlmond { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Blue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color BlueViolet { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Brown { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color BurlyWood { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color CadetBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Chartreuse { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Chocolate { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Coral { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color CornflowerBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Cornsilk { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Crimson { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Cyan { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color DarkBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color DarkCyan { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color DarkGoldenrod { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color DarkGray { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color DarkGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color DarkKhaki { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color DarkMagenta { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color DarkOliveGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color DarkOrange { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color DarkOrchid { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color DarkRed { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color DarkSalmon { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color DarkSeaGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color DarkSlateBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color DarkSlateGray { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color DarkTurquoise { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color DarkViolet { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color DeepPink { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color DeepSkyBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color DimGray { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color DodgerBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Firebrick { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color FloralWhite { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color ForestGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Fuchsia { get { throw new PlatformNotSupportedException(); } }
        public byte G { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Gainsboro { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color GhostWhite { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Gold { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Goldenrod { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Gray { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Green { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color GreenYellow { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Honeydew { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color HotPink { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color IndianRed { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Indigo { get { throw new PlatformNotSupportedException(); } }
        public bool IsEmpty { get { throw new PlatformNotSupportedException(); } }
        public bool IsKnownColor { get { throw new PlatformNotSupportedException(); } }
        public bool IsNamedColor { get { throw new PlatformNotSupportedException(); } }
        public bool IsSystemColor { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Ivory { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Khaki { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Lavender { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color LavenderBlush { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color LawnGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color LemonChiffon { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color LightBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color LightCoral { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color LightCyan { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color LightGoldenrodYellow { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color LightGray { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color LightGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color LightPink { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color LightSalmon { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color LightSeaGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color LightSkyBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color LightSlateGray { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color LightSteelBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color LightYellow { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Lime { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color LimeGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Linen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Magenta { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Maroon { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color MediumAquamarine { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color MediumBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color MediumOrchid { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color MediumPurple { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color MediumSeaGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color MediumSlateBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color MediumSpringGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color MediumTurquoise { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color MediumVioletRed { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color MidnightBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color MintCream { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color MistyRose { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Moccasin { get { throw new PlatformNotSupportedException(); } }
        public string Name { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color NavajoWhite { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Navy { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color OldLace { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Olive { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color OliveDrab { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Orange { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color OrangeRed { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Orchid { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color PaleGoldenrod { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color PaleGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color PaleTurquoise { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color PaleVioletRed { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color PapayaWhip { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color PeachPuff { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Peru { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Pink { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Plum { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color PowderBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Purple { get { throw new PlatformNotSupportedException(); } }
        public byte R { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Red { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color RosyBrown { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color RoyalBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color SaddleBrown { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Salmon { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color SandyBrown { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color SeaGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color SeaShell { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Sienna { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Silver { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color SkyBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color SlateBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color SlateGray { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Snow { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color SpringGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color SteelBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Tan { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Teal { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Thistle { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Tomato { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Transparent { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Turquoise { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Violet { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Wheat { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color White { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color WhiteSmoke { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Yellow { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color YellowGreen { get { throw new PlatformNotSupportedException(); } }
        public bool Equals(System.Drawing.Color other) { throw new PlatformNotSupportedException(); }
        public override bool Equals(object obj) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Color FromArgb(int argb) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Color FromArgb(int alpha, System.Drawing.Color baseColor) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Color FromArgb(int red, int green, int blue) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Color FromArgb(int alpha, int red, int green, int blue) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Color FromKnownColor(System.Drawing.KnownColor color) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Color FromName(string name) { throw new PlatformNotSupportedException(); }
        public float GetBrightness() { throw new PlatformNotSupportedException(); }
        public override int GetHashCode() { throw new PlatformNotSupportedException(); }
        public float GetHue() { throw new PlatformNotSupportedException(); }
        public float GetSaturation() { throw new PlatformNotSupportedException(); }
        public static bool operator ==(System.Drawing.Color left, System.Drawing.Color right) { throw new PlatformNotSupportedException(); }
        public static bool operator !=(System.Drawing.Color left, System.Drawing.Color right) { throw new PlatformNotSupportedException(); }
        public int ToArgb() { throw new PlatformNotSupportedException(); }
        public System.Drawing.KnownColor ToKnownColor() { throw new PlatformNotSupportedException(); }
        public override string ToString() { throw new PlatformNotSupportedException(); }
    }
    public enum KnownColor
    {
        ActiveBorder = 1,
        ActiveCaption = 2,
        ActiveCaptionText = 3,
        AliceBlue = 28,
        AntiqueWhite = 29,
        AppWorkspace = 4,
        Aqua = 30,
        Aquamarine = 31,
        Azure = 32,
        Beige = 33,
        Bisque = 34,
        Black = 35,
        BlanchedAlmond = 36,
        Blue = 37,
        BlueViolet = 38,
        Brown = 39,
        BurlyWood = 40,
        ButtonFace = 168,
        ButtonHighlight = 169,
        ButtonShadow = 170,
        CadetBlue = 41,
        Chartreuse = 42,
        Chocolate = 43,
        Control = 5,
        ControlDark = 6,
        ControlDarkDark = 7,
        ControlLight = 8,
        ControlLightLight = 9,
        ControlText = 10,
        Coral = 44,
        CornflowerBlue = 45,
        Cornsilk = 46,
        Crimson = 47,
        Cyan = 48,
        DarkBlue = 49,
        DarkCyan = 50,
        DarkGoldenrod = 51,
        DarkGray = 52,
        DarkGreen = 53,
        DarkKhaki = 54,
        DarkMagenta = 55,
        DarkOliveGreen = 56,
        DarkOrange = 57,
        DarkOrchid = 58,
        DarkRed = 59,
        DarkSalmon = 60,
        DarkSeaGreen = 61,
        DarkSlateBlue = 62,
        DarkSlateGray = 63,
        DarkTurquoise = 64,
        DarkViolet = 65,
        DeepPink = 66,
        DeepSkyBlue = 67,
        Desktop = 11,
        DimGray = 68,
        DodgerBlue = 69,
        Firebrick = 70,
        FloralWhite = 71,
        ForestGreen = 72,
        Fuchsia = 73,
        Gainsboro = 74,
        GhostWhite = 75,
        Gold = 76,
        Goldenrod = 77,
        GradientActiveCaption = 171,
        GradientInactiveCaption = 172,
        Gray = 78,
        GrayText = 12,
        Green = 79,
        GreenYellow = 80,
        Highlight = 13,
        HighlightText = 14,
        Honeydew = 81,
        HotPink = 82,
        HotTrack = 15,
        InactiveBorder = 16,
        InactiveCaption = 17,
        InactiveCaptionText = 18,
        IndianRed = 83,
        Indigo = 84,
        Info = 19,
        InfoText = 20,
        Ivory = 85,
        Khaki = 86,
        Lavender = 87,
        LavenderBlush = 88,
        LawnGreen = 89,
        LemonChiffon = 90,
        LightBlue = 91,
        LightCoral = 92,
        LightCyan = 93,
        LightGoldenrodYellow = 94,
        LightGray = 95,
        LightGreen = 96,
        LightPink = 97,
        LightSalmon = 98,
        LightSeaGreen = 99,
        LightSkyBlue = 100,
        LightSlateGray = 101,
        LightSteelBlue = 102,
        LightYellow = 103,
        Lime = 104,
        LimeGreen = 105,
        Linen = 106,
        Magenta = 107,
        Maroon = 108,
        MediumAquamarine = 109,
        MediumBlue = 110,
        MediumOrchid = 111,
        MediumPurple = 112,
        MediumSeaGreen = 113,
        MediumSlateBlue = 114,
        MediumSpringGreen = 115,
        MediumTurquoise = 116,
        MediumVioletRed = 117,
        Menu = 21,
        MenuBar = 173,
        MenuHighlight = 174,
        MenuText = 22,
        MidnightBlue = 118,
        MintCream = 119,
        MistyRose = 120,
        Moccasin = 121,
        NavajoWhite = 122,
        Navy = 123,
        OldLace = 124,
        Olive = 125,
        OliveDrab = 126,
        Orange = 127,
        OrangeRed = 128,
        Orchid = 129,
        PaleGoldenrod = 130,
        PaleGreen = 131,
        PaleTurquoise = 132,
        PaleVioletRed = 133,
        PapayaWhip = 134,
        PeachPuff = 135,
        Peru = 136,
        Pink = 137,
        Plum = 138,
        PowderBlue = 139,
        Purple = 140,
        Red = 141,
        RosyBrown = 142,
        RoyalBlue = 143,
        SaddleBrown = 144,
        Salmon = 145,
        SandyBrown = 146,
        ScrollBar = 23,
        SeaGreen = 147,
        SeaShell = 148,
        Sienna = 149,
        Silver = 150,
        SkyBlue = 151,
        SlateBlue = 152,
        SlateGray = 153,
        Snow = 154,
        SpringGreen = 155,
        SteelBlue = 156,
        Tan = 157,
        Teal = 158,
        Thistle = 159,
        Tomato = 160,
        Transparent = 27,
        Turquoise = 161,
        Violet = 162,
        Wheat = 163,
        White = 164,
        WhiteSmoke = 165,
        Window = 24,
        WindowFrame = 25,
        WindowText = 26,
        Yellow = 166,
        YellowGreen = 167,
    }
    public partial struct Point : System.IEquatable<System.Drawing.Point>
    {
        private int _dummyPrimitive;
        public static readonly System.Drawing.Point Empty;
        public Point(System.Drawing.Size sz) { throw new PlatformNotSupportedException(); }
        public Point(int dw) { throw new PlatformNotSupportedException(); }
        public Point(int x, int y) { throw new PlatformNotSupportedException(); }
        [System.ComponentModel.BrowsableAttribute(false)]
        public bool IsEmpty { get { throw new PlatformNotSupportedException(); } }
        public int X { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public int Y { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public static System.Drawing.Point Add(System.Drawing.Point pt, System.Drawing.Size sz) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Point Ceiling(System.Drawing.PointF value) { throw new PlatformNotSupportedException(); }
        public bool Equals(System.Drawing.Point other) { throw new PlatformNotSupportedException(); }
        public override bool Equals(object obj) { throw new PlatformNotSupportedException(); }
        public override int GetHashCode() { throw new PlatformNotSupportedException(); }
        public void Offset(System.Drawing.Point p) => throw new PlatformNotSupportedException();
        public void Offset(int dx, int dy) => throw new PlatformNotSupportedException();
        public static System.Drawing.Point operator +(System.Drawing.Point pt, System.Drawing.Size sz) { throw new PlatformNotSupportedException(); }
        public static bool operator ==(System.Drawing.Point left, System.Drawing.Point right) { throw new PlatformNotSupportedException(); }
        public static explicit operator System.Drawing.Size (System.Drawing.Point p) { throw new PlatformNotSupportedException(); }
        public static implicit operator System.Drawing.PointF (System.Drawing.Point p) { throw new PlatformNotSupportedException(); }
        public static bool operator !=(System.Drawing.Point left, System.Drawing.Point right) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Point operator -(System.Drawing.Point pt, System.Drawing.Size sz) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Point Round(System.Drawing.PointF value) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Point Subtract(System.Drawing.Point pt, System.Drawing.Size sz) { throw new PlatformNotSupportedException(); }
        public override string ToString() { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Point Truncate(System.Drawing.PointF value) { throw new PlatformNotSupportedException(); }
    }
    public partial struct PointF : System.IEquatable<System.Drawing.PointF>
    {
        private int _dummyPrimitive;
        public static readonly System.Drawing.PointF Empty;
        public PointF(float x, float y) { throw new PlatformNotSupportedException(); }
        [System.ComponentModel.BrowsableAttribute(false)]
        public bool IsEmpty { get { throw new PlatformNotSupportedException(); } }
        public float X { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float Y { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public static System.Drawing.PointF Add(System.Drawing.PointF pt, System.Drawing.Size sz) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.PointF Add(System.Drawing.PointF pt, System.Drawing.SizeF sz) { throw new PlatformNotSupportedException(); }
        public bool Equals(System.Drawing.PointF other) { throw new PlatformNotSupportedException(); }
        public override bool Equals(object obj) { throw new PlatformNotSupportedException(); }
        public override int GetHashCode() { throw new PlatformNotSupportedException(); }
        public static System.Drawing.PointF operator +(System.Drawing.PointF pt, System.Drawing.Size sz) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.PointF operator +(System.Drawing.PointF pt, System.Drawing.SizeF sz) { throw new PlatformNotSupportedException(); }
        public static bool operator ==(System.Drawing.PointF left, System.Drawing.PointF right) { throw new PlatformNotSupportedException(); }
        public static bool operator !=(System.Drawing.PointF left, System.Drawing.PointF right) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.PointF operator -(System.Drawing.PointF pt, System.Drawing.Size sz) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.PointF operator -(System.Drawing.PointF pt, System.Drawing.SizeF sz) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.PointF Subtract(System.Drawing.PointF pt, System.Drawing.Size sz) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.PointF Subtract(System.Drawing.PointF pt, System.Drawing.SizeF sz) { throw new PlatformNotSupportedException(); }
        public override string ToString() { throw new PlatformNotSupportedException(); }
    }
    public partial struct Rectangle : System.IEquatable<System.Drawing.Rectangle>
    {
        private int _dummyPrimitive;
        public static readonly System.Drawing.Rectangle Empty;
        public Rectangle(System.Drawing.Point location, System.Drawing.Size size) { throw new PlatformNotSupportedException(); }
        public Rectangle(int x, int y, int width, int height) { throw new PlatformNotSupportedException(); }
        [System.ComponentModel.BrowsableAttribute(false)]
        public int Bottom { get { throw new PlatformNotSupportedException(); } }
        public int Height { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        [System.ComponentModel.BrowsableAttribute(false)]
        public bool IsEmpty { get { throw new PlatformNotSupportedException(); } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public int Left { get { throw new PlatformNotSupportedException(); } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public System.Drawing.Point Location { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        [System.ComponentModel.BrowsableAttribute(false)]
        public int Right { get { throw new PlatformNotSupportedException(); } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public System.Drawing.Size Size { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        [System.ComponentModel.BrowsableAttribute(false)]
        public int Top { get { throw new PlatformNotSupportedException(); } }
        public int Width { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public int X { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public int Y { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public static System.Drawing.Rectangle Ceiling(System.Drawing.RectangleF value) { throw new PlatformNotSupportedException(); }
        public bool Contains(System.Drawing.Point pt) { throw new PlatformNotSupportedException(); }
        public bool Contains(System.Drawing.Rectangle rect) { throw new PlatformNotSupportedException(); }
        public bool Contains(int x, int y) { throw new PlatformNotSupportedException(); }
        public bool Equals(System.Drawing.Rectangle other) { throw new PlatformNotSupportedException(); }
        public override bool Equals(object obj) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Rectangle FromLTRB(int left, int top, int right, int bottom) { throw new PlatformNotSupportedException(); }
        public override int GetHashCode() { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Rectangle Inflate(System.Drawing.Rectangle rect, int x, int y) { throw new PlatformNotSupportedException(); }
        public void Inflate(System.Drawing.Size size) => throw new PlatformNotSupportedException();
        public void Inflate(int width, int height) => throw new PlatformNotSupportedException();
        public void Intersect(System.Drawing.Rectangle rect) => throw new PlatformNotSupportedException();
        public static System.Drawing.Rectangle Intersect(System.Drawing.Rectangle a, System.Drawing.Rectangle b) { throw new PlatformNotSupportedException(); }
        public bool IntersectsWith(System.Drawing.Rectangle rect) { throw new PlatformNotSupportedException(); }
        public void Offset(System.Drawing.Point pos) => throw new PlatformNotSupportedException();
        public void Offset(int x, int y) => throw new PlatformNotSupportedException();
        public static bool operator ==(System.Drawing.Rectangle left, System.Drawing.Rectangle right) { throw new PlatformNotSupportedException(); }
        public static bool operator !=(System.Drawing.Rectangle left, System.Drawing.Rectangle right) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Rectangle Round(System.Drawing.RectangleF value) { throw new PlatformNotSupportedException(); }
        public override string ToString() { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Rectangle Truncate(System.Drawing.RectangleF value) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Rectangle Union(System.Drawing.Rectangle a, System.Drawing.Rectangle b) { throw new PlatformNotSupportedException(); }
    }
    public partial struct RectangleF : System.IEquatable<System.Drawing.RectangleF>
    {
        private int _dummyPrimitive;
        public static readonly System.Drawing.RectangleF Empty;
        public RectangleF(System.Drawing.PointF location, System.Drawing.SizeF size) { throw new PlatformNotSupportedException(); }
        public RectangleF(float x, float y, float width, float height) { throw new PlatformNotSupportedException(); }
        [System.ComponentModel.BrowsableAttribute(false)]
        public float Bottom { get { throw new PlatformNotSupportedException(); } }
        public float Height { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        [System.ComponentModel.BrowsableAttribute(false)]
        public bool IsEmpty { get { throw new PlatformNotSupportedException(); } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public float Left { get { throw new PlatformNotSupportedException(); } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public System.Drawing.PointF Location { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        [System.ComponentModel.BrowsableAttribute(false)]
        public float Right { get { throw new PlatformNotSupportedException(); } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public System.Drawing.SizeF Size { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        [System.ComponentModel.BrowsableAttribute(false)]
        public float Top { get { throw new PlatformNotSupportedException(); } }
        public float Width { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float X { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float Y { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public bool Contains(System.Drawing.PointF pt) { throw new PlatformNotSupportedException(); }
        public bool Contains(System.Drawing.RectangleF rect) { throw new PlatformNotSupportedException(); }
        public bool Contains(float x, float y) { throw new PlatformNotSupportedException(); }
        public bool Equals(System.Drawing.RectangleF other) { throw new PlatformNotSupportedException(); }
        public override bool Equals(object obj) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.RectangleF FromLTRB(float left, float top, float right, float bottom) { throw new PlatformNotSupportedException(); }
        public override int GetHashCode() { throw new PlatformNotSupportedException(); }
        public static System.Drawing.RectangleF Inflate(System.Drawing.RectangleF rect, float x, float y) { throw new PlatformNotSupportedException(); }
        public void Inflate(System.Drawing.SizeF size) => throw new PlatformNotSupportedException();
        public void Inflate(float x, float y) => throw new PlatformNotSupportedException();
        public void Intersect(System.Drawing.RectangleF rect) => throw new PlatformNotSupportedException();
        public static System.Drawing.RectangleF Intersect(System.Drawing.RectangleF a, System.Drawing.RectangleF b) { throw new PlatformNotSupportedException(); }
        public bool IntersectsWith(System.Drawing.RectangleF rect) { throw new PlatformNotSupportedException(); }
        public void Offset(System.Drawing.PointF pos) => throw new PlatformNotSupportedException();
        public void Offset(float x, float y) => throw new PlatformNotSupportedException();
        public static bool operator ==(System.Drawing.RectangleF left, System.Drawing.RectangleF right) { throw new PlatformNotSupportedException(); }
        public static implicit operator System.Drawing.RectangleF (System.Drawing.Rectangle r) { throw new PlatformNotSupportedException(); }
        public static bool operator !=(System.Drawing.RectangleF left, System.Drawing.RectangleF right) { throw new PlatformNotSupportedException(); }
        public override string ToString() { throw new PlatformNotSupportedException(); }
        public static System.Drawing.RectangleF Union(System.Drawing.RectangleF a, System.Drawing.RectangleF b) { throw new PlatformNotSupportedException(); }
    }
    public partial struct Size : System.IEquatable<System.Drawing.Size>
    {
        private int _dummyPrimitive;
        public static readonly System.Drawing.Size Empty;
        public Size(System.Drawing.Point pt) { throw new PlatformNotSupportedException(); }
        public Size(int width, int height) { throw new PlatformNotSupportedException(); }
        public int Height { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        [System.ComponentModel.BrowsableAttribute(false)]
        public bool IsEmpty { get { throw new PlatformNotSupportedException(); } }
        public int Width { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public static System.Drawing.Size Add(System.Drawing.Size sz1, System.Drawing.Size sz2) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Size Ceiling(System.Drawing.SizeF value) { throw new PlatformNotSupportedException(); }
        public bool Equals(System.Drawing.Size other) { throw new PlatformNotSupportedException(); }
        public override bool Equals(object obj) { throw new PlatformNotSupportedException(); }
        public override int GetHashCode() { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Size operator +(System.Drawing.Size sz1, System.Drawing.Size sz2) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Size operator /(System.Drawing.Size left, int right) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.SizeF operator /(System.Drawing.Size left, float right) { throw new PlatformNotSupportedException(); }
        public static bool operator ==(System.Drawing.Size sz1, System.Drawing.Size sz2) { throw new PlatformNotSupportedException(); }
        public static explicit operator System.Drawing.Point (System.Drawing.Size size) { throw new PlatformNotSupportedException(); }
        public static implicit operator System.Drawing.SizeF (System.Drawing.Size p) { throw new PlatformNotSupportedException(); }
        public static bool operator !=(System.Drawing.Size sz1, System.Drawing.Size sz2) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Size operator *(System.Drawing.Size left, int right) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.SizeF operator *(System.Drawing.Size left, float right) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Size operator *(int left, System.Drawing.Size right) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.SizeF operator *(float left, System.Drawing.Size right) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Size operator -(System.Drawing.Size sz1, System.Drawing.Size sz2) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Size Round(System.Drawing.SizeF value) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Size Subtract(System.Drawing.Size sz1, System.Drawing.Size sz2) { throw new PlatformNotSupportedException(); }
        public override string ToString() { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Size Truncate(System.Drawing.SizeF value) { throw new PlatformNotSupportedException(); }
    }
    public partial struct SizeF : System.IEquatable<System.Drawing.SizeF>
    {
        private int _dummyPrimitive;
        public static readonly System.Drawing.SizeF Empty;
        public SizeF(System.Drawing.PointF pt) { throw new PlatformNotSupportedException(); }
        public SizeF(System.Drawing.SizeF size) { throw new PlatformNotSupportedException(); }
        public SizeF(float width, float height) { throw new PlatformNotSupportedException(); }
        public float Height { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        [System.ComponentModel.BrowsableAttribute(false)]
        public bool IsEmpty { get { throw new PlatformNotSupportedException(); } }
        public float Width { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public static System.Drawing.SizeF Add(System.Drawing.SizeF sz1, System.Drawing.SizeF sz2) { throw new PlatformNotSupportedException(); }
        public bool Equals(System.Drawing.SizeF other) { throw new PlatformNotSupportedException(); }
        public override bool Equals(object obj) { throw new PlatformNotSupportedException(); }
        public override int GetHashCode() { throw new PlatformNotSupportedException(); }
        public static System.Drawing.SizeF operator +(System.Drawing.SizeF sz1, System.Drawing.SizeF sz2) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.SizeF operator /(System.Drawing.SizeF left, float right) { throw new PlatformNotSupportedException(); }
        public static bool operator ==(System.Drawing.SizeF sz1, System.Drawing.SizeF sz2) { throw new PlatformNotSupportedException(); }
        public static explicit operator System.Drawing.PointF (System.Drawing.SizeF size) { throw new PlatformNotSupportedException(); }
        public static bool operator !=(System.Drawing.SizeF sz1, System.Drawing.SizeF sz2) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.SizeF operator *(System.Drawing.SizeF left, float right) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.SizeF operator *(float left, System.Drawing.SizeF right) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.SizeF operator -(System.Drawing.SizeF sz1, System.Drawing.SizeF sz2) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.SizeF Subtract(System.Drawing.SizeF sz1, System.Drawing.SizeF sz2) { throw new PlatformNotSupportedException(); }
        public System.Drawing.PointF ToPointF() { throw new PlatformNotSupportedException(); }
        public System.Drawing.Size ToSize() { throw new PlatformNotSupportedException(); }
        public override string ToString() { throw new PlatformNotSupportedException(); }
    }

#endif // !MONODROID

}

// System.Drawing.Common/ref:
namespace System.Drawing
{
    public sealed partial class Bitmap : System.Drawing.Image
    {
        public Bitmap(System.Drawing.Image original) => throw new PlatformNotSupportedException();
        public Bitmap(System.Drawing.Image original, System.Drawing.Size newSize) => throw new PlatformNotSupportedException();
        public Bitmap(System.Drawing.Image original, int width, int height) => throw new PlatformNotSupportedException();
        public Bitmap(int width, int height) => throw new PlatformNotSupportedException();
        public Bitmap(int width, int height, System.Drawing.Graphics g) => throw new PlatformNotSupportedException();
        public Bitmap(int width, int height, System.Drawing.Imaging.PixelFormat format) => throw new PlatformNotSupportedException();
        public Bitmap(int width, int height, int stride, System.Drawing.Imaging.PixelFormat format, System.IntPtr scan0) => throw new PlatformNotSupportedException();
        public Bitmap(System.IO.Stream stream) => throw new PlatformNotSupportedException();
        public Bitmap(System.IO.Stream stream, bool useIcm) => throw new PlatformNotSupportedException();
        public Bitmap(string filename) => throw new PlatformNotSupportedException();
        public Bitmap(string filename, bool useIcm) => throw new PlatformNotSupportedException();
        public Bitmap(System.Type type, string resource) => throw new PlatformNotSupportedException();
        public System.Drawing.Bitmap Clone(System.Drawing.Rectangle rect, System.Drawing.Imaging.PixelFormat format) { throw new PlatformNotSupportedException(); }
        public System.Drawing.Bitmap Clone(System.Drawing.RectangleF rect, System.Drawing.Imaging.PixelFormat format) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Bitmap FromHicon(System.IntPtr hicon) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Bitmap FromResource(System.IntPtr hinstance, string bitmapName) { throw new PlatformNotSupportedException(); }
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(2))]
        public System.IntPtr GetHbitmap() { throw new PlatformNotSupportedException(); }
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(2))]
        public System.IntPtr GetHbitmap(System.Drawing.Color background) { throw new PlatformNotSupportedException(); }
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(2))]
        public System.IntPtr GetHicon() { throw new PlatformNotSupportedException(); }
        public System.Drawing.Color GetPixel(int x, int y) { throw new PlatformNotSupportedException(); }
        public System.Drawing.Imaging.BitmapData LockBits(System.Drawing.Rectangle rect, System.Drawing.Imaging.ImageLockMode flags, System.Drawing.Imaging.PixelFormat format) { throw new PlatformNotSupportedException(); }
        public System.Drawing.Imaging.BitmapData LockBits(System.Drawing.Rectangle rect, System.Drawing.Imaging.ImageLockMode flags, System.Drawing.Imaging.PixelFormat format, System.Drawing.Imaging.BitmapData bitmapData) { throw new PlatformNotSupportedException(); }
        public void MakeTransparent() => throw new PlatformNotSupportedException();
        public void MakeTransparent(System.Drawing.Color transparentColor) => throw new PlatformNotSupportedException();
        public void SetPixel(int x, int y, System.Drawing.Color color) => throw new PlatformNotSupportedException();
        public void SetResolution(float xDpi, float yDpi) => throw new PlatformNotSupportedException();
        public void UnlockBits(System.Drawing.Imaging.BitmapData bitmapdata) => throw new PlatformNotSupportedException();
    }
    [System.AttributeUsageAttribute((System.AttributeTargets)(1))]
    public partial class BitmapSuffixInSameAssemblyAttribute : System.Attribute
    {
        public BitmapSuffixInSameAssemblyAttribute() => throw new PlatformNotSupportedException();
    }
    [System.AttributeUsageAttribute((System.AttributeTargets)(1))]
    public partial class BitmapSuffixInSatelliteAssemblyAttribute : System.Attribute
    {
        public BitmapSuffixInSatelliteAssemblyAttribute() => throw new PlatformNotSupportedException();
    }
    public abstract partial class Brush : System.MarshalByRefObject, System.ICloneable, System.IDisposable
    {
        protected Brush() => throw new PlatformNotSupportedException();
        public abstract object Clone();
        public void Dispose() => throw new PlatformNotSupportedException();
        protected virtual void Dispose(bool disposing) => throw new PlatformNotSupportedException();
        ~Brush() => throw new PlatformNotSupportedException();
        protected internal void SetNativeBrush(System.IntPtr brush) => throw new PlatformNotSupportedException();
    }
    public static partial class Brushes
    {
        public static System.Drawing.Brush AliceBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush AntiqueWhite { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Aqua { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Aquamarine { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Azure { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Beige { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Bisque { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Black { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush BlanchedAlmond { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Blue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush BlueViolet { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Brown { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush BurlyWood { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush CadetBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Chartreuse { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Chocolate { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Coral { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush CornflowerBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Cornsilk { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Crimson { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Cyan { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush DarkBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush DarkCyan { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush DarkGoldenrod { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush DarkGray { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush DarkGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush DarkKhaki { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush DarkMagenta { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush DarkOliveGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush DarkOrange { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush DarkOrchid { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush DarkRed { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush DarkSalmon { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush DarkSeaGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush DarkSlateBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush DarkSlateGray { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush DarkTurquoise { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush DarkViolet { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush DeepPink { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush DeepSkyBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush DimGray { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush DodgerBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Firebrick { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush FloralWhite { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush ForestGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Fuchsia { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Gainsboro { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush GhostWhite { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Gold { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Goldenrod { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Gray { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Green { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush GreenYellow { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Honeydew { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush HotPink { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush IndianRed { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Indigo { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Ivory { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Khaki { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Lavender { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush LavenderBlush { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush LawnGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush LemonChiffon { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush LightBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush LightCoral { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush LightCyan { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush LightGoldenrodYellow { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush LightGray { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush LightGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush LightPink { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush LightSalmon { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush LightSeaGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush LightSkyBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush LightSlateGray { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush LightSteelBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush LightYellow { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Lime { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush LimeGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Linen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Magenta { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Maroon { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush MediumAquamarine { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush MediumBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush MediumOrchid { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush MediumPurple { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush MediumSeaGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush MediumSlateBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush MediumSpringGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush MediumTurquoise { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush MediumVioletRed { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush MidnightBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush MintCream { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush MistyRose { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Moccasin { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush NavajoWhite { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Navy { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush OldLace { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Olive { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush OliveDrab { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Orange { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush OrangeRed { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Orchid { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush PaleGoldenrod { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush PaleGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush PaleTurquoise { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush PaleVioletRed { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush PapayaWhip { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush PeachPuff { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Peru { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Pink { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Plum { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush PowderBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Purple { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Red { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush RosyBrown { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush RoyalBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush SaddleBrown { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Salmon { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush SandyBrown { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush SeaGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush SeaShell { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Sienna { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Silver { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush SkyBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush SlateBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush SlateGray { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Snow { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush SpringGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush SteelBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Tan { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Teal { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Thistle { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Tomato { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Transparent { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Turquoise { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Violet { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Wheat { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush White { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush WhiteSmoke { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Yellow { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush YellowGreen { get { throw new PlatformNotSupportedException(); } }
    }
    public sealed partial class BufferedGraphics : System.IDisposable
    {
        internal BufferedGraphics() => throw new PlatformNotSupportedException();
        public System.Drawing.Graphics Graphics { get { throw new PlatformNotSupportedException(); } }
        public void Dispose() => throw new PlatformNotSupportedException();
        ~BufferedGraphics() => throw new PlatformNotSupportedException();
        public void Render() => throw new PlatformNotSupportedException();
        public void Render(System.Drawing.Graphics target) => throw new PlatformNotSupportedException();
        public void Render(System.IntPtr targetDC) => throw new PlatformNotSupportedException();
    }
    public sealed partial class BufferedGraphicsContext : System.IDisposable
    {
        public BufferedGraphicsContext() => throw new PlatformNotSupportedException();
        public System.Drawing.Size MaximumBuffer { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.BufferedGraphics Allocate(System.Drawing.Graphics targetGraphics, System.Drawing.Rectangle targetRectangle) { throw new PlatformNotSupportedException(); }
        public System.Drawing.BufferedGraphics Allocate(System.IntPtr targetDC, System.Drawing.Rectangle targetRectangle) { throw new PlatformNotSupportedException(); }
        public void Dispose() => throw new PlatformNotSupportedException();
        ~BufferedGraphicsContext() => throw new PlatformNotSupportedException();
        public void Invalidate() => throw new PlatformNotSupportedException();
    }
    public static partial class BufferedGraphicsManager
    {
        public static System.Drawing.BufferedGraphicsContext Current { get { throw new PlatformNotSupportedException(); } }
    }
    public partial struct CharacterRange
    {
        private int _dummy;
        public CharacterRange(int First, int Length) { throw new PlatformNotSupportedException(); }
        public int First { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public int Length { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public override bool Equals(object obj) { throw new PlatformNotSupportedException(); }
        public override int GetHashCode() { throw new PlatformNotSupportedException(); }
        public static bool operator ==(System.Drawing.CharacterRange cr1, System.Drawing.CharacterRange cr2) { throw new PlatformNotSupportedException(); }
        public static bool operator !=(System.Drawing.CharacterRange cr1, System.Drawing.CharacterRange cr2) { throw new PlatformNotSupportedException(); }
    }
    public static partial class ColorTranslator
    {
        public static System.Drawing.Color FromHtml(string htmlColor) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Color FromOle(int oleColor) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Color FromWin32(int win32Color) { throw new PlatformNotSupportedException(); }
        public static string ToHtml(System.Drawing.Color c) { throw new PlatformNotSupportedException(); }
        public static int ToOle(System.Drawing.Color c) { throw new PlatformNotSupportedException(); }
        public static int ToWin32(System.Drawing.Color c) { throw new PlatformNotSupportedException(); }
    }
    public enum ContentAlignment
    {
        BottomCenter = 512,
        BottomLeft = 256,
        BottomRight = 1024,
        MiddleCenter = 32,
        MiddleLeft = 16,
        MiddleRight = 64,
        TopCenter = 2,
        TopLeft = 1,
        TopRight = 4,
    }
    public enum CopyPixelOperation
    {
        Blackness = 66,
        CaptureBlt = 1073741824,
        DestinationInvert = 5570569,
        MergeCopy = 12583114,
        MergePaint = 12255782,
        NoMirrorBitmap = -2147483648,
        NotSourceCopy = 3342344,
        NotSourceErase = 1114278,
        PatCopy = 15728673,
        PatInvert = 5898313,
        PatPaint = 16452105,
        SourceAnd = 8913094,
        SourceCopy = 13369376,
        SourceErase = 4457256,
        SourceInvert = 6684742,
        SourcePaint = 15597702,
        Whiteness = 16711778,
    }
#if netcoreapp
    [System.ComponentModel.TypeConverter("System.Drawing.FontConverter, System.Windows.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51")]
#endif
    public sealed partial class Font : System.MarshalByRefObject, System.ICloneable, System.IDisposable, System.Runtime.Serialization.ISerializable
    {
        public Font(System.Drawing.Font prototype, System.Drawing.FontStyle newStyle) => throw new PlatformNotSupportedException();
        public Font(System.Drawing.FontFamily family, float emSize) => throw new PlatformNotSupportedException();
        public Font(System.Drawing.FontFamily family, float emSize, System.Drawing.FontStyle style) => throw new PlatformNotSupportedException();
        public Font(System.Drawing.FontFamily family, float emSize, System.Drawing.FontStyle style, System.Drawing.GraphicsUnit unit) => throw new PlatformNotSupportedException();
        public Font(System.Drawing.FontFamily family, float emSize, System.Drawing.FontStyle style, System.Drawing.GraphicsUnit unit, byte gdiCharSet) => throw new PlatformNotSupportedException();
        public Font(System.Drawing.FontFamily family, float emSize, System.Drawing.FontStyle style, System.Drawing.GraphicsUnit unit, byte gdiCharSet, bool gdiVerticalFont) => throw new PlatformNotSupportedException();
        public Font(System.Drawing.FontFamily family, float emSize, System.Drawing.GraphicsUnit unit) => throw new PlatformNotSupportedException();
        public Font(string familyName, float emSize) => throw new PlatformNotSupportedException();
        public Font(string familyName, float emSize, System.Drawing.FontStyle style) => throw new PlatformNotSupportedException();
        public Font(string familyName, float emSize, System.Drawing.FontStyle style, System.Drawing.GraphicsUnit unit) => throw new PlatformNotSupportedException();
        public Font(string familyName, float emSize, System.Drawing.FontStyle style, System.Drawing.GraphicsUnit unit, byte gdiCharSet) => throw new PlatformNotSupportedException();
        public Font(string familyName, float emSize, System.Drawing.FontStyle style, System.Drawing.GraphicsUnit unit, byte gdiCharSet, bool gdiVerticalFont) => throw new PlatformNotSupportedException();
        public Font(string familyName, float emSize, System.Drawing.GraphicsUnit unit) => throw new PlatformNotSupportedException();
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public bool Bold { get { throw new PlatformNotSupportedException(); } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public System.Drawing.FontFamily FontFamily { get { throw new PlatformNotSupportedException(); } }
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public byte GdiCharSet { get { throw new PlatformNotSupportedException(); } }
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public bool GdiVerticalFont { get { throw new PlatformNotSupportedException(); } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public int Height { get { throw new PlatformNotSupportedException(); } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public bool IsSystemFont { get { throw new PlatformNotSupportedException(); } }
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public bool Italic { get { throw new PlatformNotSupportedException(); } }
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public string Name { get { throw new PlatformNotSupportedException(); } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public string OriginalFontName { get { throw new PlatformNotSupportedException(); } }
        public float Size { get { throw new PlatformNotSupportedException(); } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public float SizeInPoints { get { throw new PlatformNotSupportedException(); } }
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public bool Strikeout { get { throw new PlatformNotSupportedException(); } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public System.Drawing.FontStyle Style { get { throw new PlatformNotSupportedException(); } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public string SystemFontName { get { throw new PlatformNotSupportedException(); } }
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public bool Underline { get { throw new PlatformNotSupportedException(); } }
        public System.Drawing.GraphicsUnit Unit { get { throw new PlatformNotSupportedException(); } }
        public object Clone() { throw new PlatformNotSupportedException(); }
        public void Dispose() => throw new PlatformNotSupportedException();
        public override bool Equals(object obj) { throw new PlatformNotSupportedException(); }
        ~Font() => throw new PlatformNotSupportedException();
        public static System.Drawing.Font FromHdc(System.IntPtr hdc) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Font FromHfont(System.IntPtr hfont) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Font FromLogFont(object lf) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Font FromLogFont(object lf, System.IntPtr hdc) { throw new PlatformNotSupportedException(); }
        public override int GetHashCode() { throw new PlatformNotSupportedException(); }
        public float GetHeight() { throw new PlatformNotSupportedException(); }
        public float GetHeight(System.Drawing.Graphics graphics) { throw new PlatformNotSupportedException(); }
        public float GetHeight(float dpi) { throw new PlatformNotSupportedException(); }
        void System.Runtime.Serialization.ISerializable.GetObjectData(System.Runtime.Serialization.SerializationInfo si, System.Runtime.Serialization.StreamingContext context) => throw new PlatformNotSupportedException();
        public System.IntPtr ToHfont() { throw new PlatformNotSupportedException(); }
        public void ToLogFont(object logFont) => throw new PlatformNotSupportedException();
        public void ToLogFont(object logFont, System.Drawing.Graphics graphics) => throw new PlatformNotSupportedException();
        public override string ToString() { throw new PlatformNotSupportedException(); }
    }
    public sealed partial class FontFamily : System.MarshalByRefObject, System.IDisposable
    {
        public FontFamily(System.Drawing.Text.GenericFontFamilies genericFamily) => throw new PlatformNotSupportedException();
        public FontFamily(string name) => throw new PlatformNotSupportedException();
        public FontFamily(string name, System.Drawing.Text.FontCollection fontCollection) => throw new PlatformNotSupportedException();
        public static System.Drawing.FontFamily[] Families { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.FontFamily GenericMonospace { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.FontFamily GenericSansSerif { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.FontFamily GenericSerif { get { throw new PlatformNotSupportedException(); } }
        public string Name { get { throw new PlatformNotSupportedException(); } }
        public void Dispose() => throw new PlatformNotSupportedException();
        public override bool Equals(object obj) { throw new PlatformNotSupportedException(); }
        ~FontFamily() => throw new PlatformNotSupportedException();
        public int GetCellAscent(System.Drawing.FontStyle style) { throw new PlatformNotSupportedException(); }
        public int GetCellDescent(System.Drawing.FontStyle style) { throw new PlatformNotSupportedException(); }
        public int GetEmHeight(System.Drawing.FontStyle style) { throw new PlatformNotSupportedException(); }
        [System.ObsoleteAttribute("Do not use method GetFamilies, use property Families instead")]
        public static System.Drawing.FontFamily[] GetFamilies(System.Drawing.Graphics graphics) { throw new PlatformNotSupportedException(); }
        public override int GetHashCode() { throw new PlatformNotSupportedException(); }
        public int GetLineSpacing(System.Drawing.FontStyle style) { throw new PlatformNotSupportedException(); }
        public string GetName(int language) { throw new PlatformNotSupportedException(); }
        public bool IsStyleAvailable(System.Drawing.FontStyle style) { throw new PlatformNotSupportedException(); }
        public override string ToString() { throw new PlatformNotSupportedException(); }
    }
    [System.FlagsAttribute]
    public enum FontStyle
    {
        Bold = 1,
        Italic = 2,
        Regular = 0,
        Strikeout = 8,
        Underline = 4,
    }
    public sealed partial class Graphics : System.MarshalByRefObject, System.Drawing.IDeviceContext, System.IDisposable
    {
        internal Graphics() => throw new PlatformNotSupportedException();
        public System.Drawing.Region Clip { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.RectangleF ClipBounds { get { throw new PlatformNotSupportedException(); } }
        public System.Drawing.Drawing2D.CompositingMode CompositingMode { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Drawing2D.CompositingQuality CompositingQuality { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float DpiX { get { throw new PlatformNotSupportedException(); } }
        public float DpiY { get { throw new PlatformNotSupportedException(); } }
        public System.Drawing.Drawing2D.InterpolationMode InterpolationMode { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public bool IsClipEmpty { get { throw new PlatformNotSupportedException(); } }
        public bool IsVisibleClipEmpty { get { throw new PlatformNotSupportedException(); } }
        public float PageScale { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.GraphicsUnit PageUnit { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Drawing2D.PixelOffsetMode PixelOffsetMode { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Point RenderingOrigin { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Drawing2D.SmoothingMode SmoothingMode { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public int TextContrast { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Text.TextRenderingHint TextRenderingHint { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Drawing2D.Matrix Transform { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.RectangleF VisibleClipBounds { get { throw new PlatformNotSupportedException(); } }
        public void AddMetafileComment(byte[] data) => throw new PlatformNotSupportedException();
        public System.Drawing.Drawing2D.GraphicsContainer BeginContainer() { throw new PlatformNotSupportedException(); }
        public System.Drawing.Drawing2D.GraphicsContainer BeginContainer(System.Drawing.Rectangle dstrect, System.Drawing.Rectangle srcrect, System.Drawing.GraphicsUnit unit) { throw new PlatformNotSupportedException(); }
        public System.Drawing.Drawing2D.GraphicsContainer BeginContainer(System.Drawing.RectangleF dstrect, System.Drawing.RectangleF srcrect, System.Drawing.GraphicsUnit unit) { throw new PlatformNotSupportedException(); }
        public void Clear(System.Drawing.Color color) => throw new PlatformNotSupportedException();
        public void CopyFromScreen(System.Drawing.Point upperLeftSource, System.Drawing.Point upperLeftDestination, System.Drawing.Size blockRegionSize) => throw new PlatformNotSupportedException();
        public void CopyFromScreen(System.Drawing.Point upperLeftSource, System.Drawing.Point upperLeftDestination, System.Drawing.Size blockRegionSize, System.Drawing.CopyPixelOperation copyPixelOperation) => throw new PlatformNotSupportedException();
        public void CopyFromScreen(int sourceX, int sourceY, int destinationX, int destinationY, System.Drawing.Size blockRegionSize) => throw new PlatformNotSupportedException();
        public void CopyFromScreen(int sourceX, int sourceY, int destinationX, int destinationY, System.Drawing.Size blockRegionSize, System.Drawing.CopyPixelOperation copyPixelOperation) => throw new PlatformNotSupportedException();
        public void Dispose() => throw new PlatformNotSupportedException();
        public void DrawArc(System.Drawing.Pen pen, System.Drawing.Rectangle rect, float startAngle, float sweepAngle) => throw new PlatformNotSupportedException();
        public void DrawArc(System.Drawing.Pen pen, System.Drawing.RectangleF rect, float startAngle, float sweepAngle) => throw new PlatformNotSupportedException();
        public void DrawArc(System.Drawing.Pen pen, int x, int y, int width, int height, int startAngle, int sweepAngle) => throw new PlatformNotSupportedException();
        public void DrawArc(System.Drawing.Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle) => throw new PlatformNotSupportedException();
        public void DrawBezier(System.Drawing.Pen pen, System.Drawing.Point pt1, System.Drawing.Point pt2, System.Drawing.Point pt3, System.Drawing.Point pt4) => throw new PlatformNotSupportedException();
        public void DrawBezier(System.Drawing.Pen pen, System.Drawing.PointF pt1, System.Drawing.PointF pt2, System.Drawing.PointF pt3, System.Drawing.PointF pt4) => throw new PlatformNotSupportedException();
        public void DrawBezier(System.Drawing.Pen pen, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4) => throw new PlatformNotSupportedException();
        public void DrawBeziers(System.Drawing.Pen pen, System.Drawing.PointF[] points) => throw new PlatformNotSupportedException();
        public void DrawBeziers(System.Drawing.Pen pen, System.Drawing.Point[] points) => throw new PlatformNotSupportedException();
        public void DrawClosedCurve(System.Drawing.Pen pen, System.Drawing.PointF[] points) => throw new PlatformNotSupportedException();
        public void DrawClosedCurve(System.Drawing.Pen pen, System.Drawing.PointF[] points, float tension, System.Drawing.Drawing2D.FillMode fillmode) => throw new PlatformNotSupportedException();
        public void DrawClosedCurve(System.Drawing.Pen pen, System.Drawing.Point[] points) => throw new PlatformNotSupportedException();
        public void DrawClosedCurve(System.Drawing.Pen pen, System.Drawing.Point[] points, float tension, System.Drawing.Drawing2D.FillMode fillmode) => throw new PlatformNotSupportedException();
        public void DrawCurve(System.Drawing.Pen pen, System.Drawing.PointF[] points) => throw new PlatformNotSupportedException();
        public void DrawCurve(System.Drawing.Pen pen, System.Drawing.PointF[] points, int offset, int numberOfSegments) => throw new PlatformNotSupportedException();
        public void DrawCurve(System.Drawing.Pen pen, System.Drawing.PointF[] points, int offset, int numberOfSegments, float tension) => throw new PlatformNotSupportedException();
        public void DrawCurve(System.Drawing.Pen pen, System.Drawing.PointF[] points, float tension) => throw new PlatformNotSupportedException();
        public void DrawCurve(System.Drawing.Pen pen, System.Drawing.Point[] points) => throw new PlatformNotSupportedException();
        public void DrawCurve(System.Drawing.Pen pen, System.Drawing.Point[] points, int offset, int numberOfSegments, float tension) => throw new PlatformNotSupportedException();
        public void DrawCurve(System.Drawing.Pen pen, System.Drawing.Point[] points, float tension) => throw new PlatformNotSupportedException();
        public void DrawEllipse(System.Drawing.Pen pen, System.Drawing.Rectangle rect) => throw new PlatformNotSupportedException();
        public void DrawEllipse(System.Drawing.Pen pen, System.Drawing.RectangleF rect) => throw new PlatformNotSupportedException();
        public void DrawEllipse(System.Drawing.Pen pen, int x, int y, int width, int height) => throw new PlatformNotSupportedException();
        public void DrawEllipse(System.Drawing.Pen pen, float x, float y, float width, float height) => throw new PlatformNotSupportedException();
        public void DrawIcon(System.Drawing.Icon icon, System.Drawing.Rectangle targetRect) => throw new PlatformNotSupportedException();
        public void DrawIcon(System.Drawing.Icon icon, int x, int y) => throw new PlatformNotSupportedException();
        public void DrawIconUnstretched(System.Drawing.Icon icon, System.Drawing.Rectangle targetRect) => throw new PlatformNotSupportedException();
        public void DrawImage(System.Drawing.Image image, System.Drawing.Point point) => throw new PlatformNotSupportedException();
        public void DrawImage(System.Drawing.Image image, System.Drawing.PointF point) => throw new PlatformNotSupportedException();
        public void DrawImage(System.Drawing.Image image, System.Drawing.PointF[] destPoints) => throw new PlatformNotSupportedException();
        public void DrawImage(System.Drawing.Image image, System.Drawing.PointF[] destPoints, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit srcUnit) => throw new PlatformNotSupportedException();
        public void DrawImage(System.Drawing.Image image, System.Drawing.PointF[] destPoints, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes imageAttr) => throw new PlatformNotSupportedException();
        public void DrawImage(System.Drawing.Image image, System.Drawing.PointF[] destPoints, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes imageAttr, System.Drawing.Graphics.DrawImageAbort callback) => throw new PlatformNotSupportedException();
        public void DrawImage(System.Drawing.Image image, System.Drawing.PointF[] destPoints, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes imageAttr, System.Drawing.Graphics.DrawImageAbort callback, int callbackData) => throw new PlatformNotSupportedException();
        public void DrawImage(System.Drawing.Image image, System.Drawing.Point[] destPoints) => throw new PlatformNotSupportedException();
        public void DrawImage(System.Drawing.Image image, System.Drawing.Point[] destPoints, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit srcUnit) => throw new PlatformNotSupportedException();
        public void DrawImage(System.Drawing.Image image, System.Drawing.Point[] destPoints, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes imageAttr) => throw new PlatformNotSupportedException();
        public void DrawImage(System.Drawing.Image image, System.Drawing.Point[] destPoints, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes imageAttr, System.Drawing.Graphics.DrawImageAbort callback) => throw new PlatformNotSupportedException();
        public void DrawImage(System.Drawing.Image image, System.Drawing.Point[] destPoints, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes imageAttr, System.Drawing.Graphics.DrawImageAbort callback, int callbackData) => throw new PlatformNotSupportedException();
        public void DrawImage(System.Drawing.Image image, System.Drawing.Rectangle rect) => throw new PlatformNotSupportedException();
        public void DrawImage(System.Drawing.Image image, System.Drawing.Rectangle destRect, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit srcUnit) => throw new PlatformNotSupportedException();
        public void DrawImage(System.Drawing.Image image, System.Drawing.Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, System.Drawing.GraphicsUnit srcUnit) => throw new PlatformNotSupportedException();
        public void DrawImage(System.Drawing.Image image, System.Drawing.Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes imageAttr) => throw new PlatformNotSupportedException();
        public void DrawImage(System.Drawing.Image image, System.Drawing.Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes imageAttr, System.Drawing.Graphics.DrawImageAbort callback) => throw new PlatformNotSupportedException();
        public void DrawImage(System.Drawing.Image image, System.Drawing.Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes imageAttrs, System.Drawing.Graphics.DrawImageAbort callback, System.IntPtr callbackData) => throw new PlatformNotSupportedException();
        public void DrawImage(System.Drawing.Image image, System.Drawing.Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, System.Drawing.GraphicsUnit srcUnit) => throw new PlatformNotSupportedException();
        public void DrawImage(System.Drawing.Image image, System.Drawing.Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes imageAttrs) => throw new PlatformNotSupportedException();
        public void DrawImage(System.Drawing.Image image, System.Drawing.Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes imageAttrs, System.Drawing.Graphics.DrawImageAbort callback) => throw new PlatformNotSupportedException();
        public void DrawImage(System.Drawing.Image image, System.Drawing.Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes imageAttrs, System.Drawing.Graphics.DrawImageAbort callback, System.IntPtr callbackData) => throw new PlatformNotSupportedException();
        public void DrawImage(System.Drawing.Image image, System.Drawing.RectangleF rect) => throw new PlatformNotSupportedException();
        public void DrawImage(System.Drawing.Image image, System.Drawing.RectangleF destRect, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit srcUnit) => throw new PlatformNotSupportedException();
        public void DrawImage(System.Drawing.Image image, int x, int y) => throw new PlatformNotSupportedException();
        public void DrawImage(System.Drawing.Image image, int x, int y, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit srcUnit) => throw new PlatformNotSupportedException();
        public void DrawImage(System.Drawing.Image image, int x, int y, int width, int height) => throw new PlatformNotSupportedException();
        public void DrawImage(System.Drawing.Image image, float x, float y) => throw new PlatformNotSupportedException();
        public void DrawImage(System.Drawing.Image image, float x, float y, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit srcUnit) => throw new PlatformNotSupportedException();
        public void DrawImage(System.Drawing.Image image, float x, float y, float width, float height) => throw new PlatformNotSupportedException();
        public void DrawImageUnscaled(System.Drawing.Image image, System.Drawing.Point point) => throw new PlatformNotSupportedException();
        public void DrawImageUnscaled(System.Drawing.Image image, System.Drawing.Rectangle rect) => throw new PlatformNotSupportedException();
        public void DrawImageUnscaled(System.Drawing.Image image, int x, int y) => throw new PlatformNotSupportedException();
        public void DrawImageUnscaled(System.Drawing.Image image, int x, int y, int width, int height) => throw new PlatformNotSupportedException();
        public void DrawImageUnscaledAndClipped(System.Drawing.Image image, System.Drawing.Rectangle rect) => throw new PlatformNotSupportedException();
        public void DrawLine(System.Drawing.Pen pen, System.Drawing.Point pt1, System.Drawing.Point pt2) => throw new PlatformNotSupportedException();
        public void DrawLine(System.Drawing.Pen pen, System.Drawing.PointF pt1, System.Drawing.PointF pt2) => throw new PlatformNotSupportedException();
        public void DrawLine(System.Drawing.Pen pen, int x1, int y1, int x2, int y2) => throw new PlatformNotSupportedException();
        public void DrawLine(System.Drawing.Pen pen, float x1, float y1, float x2, float y2) => throw new PlatformNotSupportedException();
        public void DrawLines(System.Drawing.Pen pen, System.Drawing.PointF[] points) => throw new PlatformNotSupportedException();
        public void DrawLines(System.Drawing.Pen pen, System.Drawing.Point[] points) => throw new PlatformNotSupportedException();
        public void DrawPath(System.Drawing.Pen pen, System.Drawing.Drawing2D.GraphicsPath path) => throw new PlatformNotSupportedException();
        public void DrawPie(System.Drawing.Pen pen, System.Drawing.Rectangle rect, float startAngle, float sweepAngle) => throw new PlatformNotSupportedException();
        public void DrawPie(System.Drawing.Pen pen, System.Drawing.RectangleF rect, float startAngle, float sweepAngle) => throw new PlatformNotSupportedException();
        public void DrawPie(System.Drawing.Pen pen, int x, int y, int width, int height, int startAngle, int sweepAngle) => throw new PlatformNotSupportedException();
        public void DrawPie(System.Drawing.Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle) => throw new PlatformNotSupportedException();
        public void DrawPolygon(System.Drawing.Pen pen, System.Drawing.PointF[] points) => throw new PlatformNotSupportedException();
        public void DrawPolygon(System.Drawing.Pen pen, System.Drawing.Point[] points) => throw new PlatformNotSupportedException();
        public void DrawRectangle(System.Drawing.Pen pen, System.Drawing.Rectangle rect) => throw new PlatformNotSupportedException();
        public void DrawRectangle(System.Drawing.Pen pen, int x, int y, int width, int height) => throw new PlatformNotSupportedException();
        public void DrawRectangle(System.Drawing.Pen pen, float x, float y, float width, float height) => throw new PlatformNotSupportedException();
        public void DrawRectangles(System.Drawing.Pen pen, System.Drawing.RectangleF[] rects) => throw new PlatformNotSupportedException();
        public void DrawRectangles(System.Drawing.Pen pen, System.Drawing.Rectangle[] rects) => throw new PlatformNotSupportedException();
        public void DrawString(string s, System.Drawing.Font font, System.Drawing.Brush brush, System.Drawing.PointF point) => throw new PlatformNotSupportedException();
        public void DrawString(string s, System.Drawing.Font font, System.Drawing.Brush brush, System.Drawing.PointF point, System.Drawing.StringFormat format) => throw new PlatformNotSupportedException();
        public void DrawString(string s, System.Drawing.Font font, System.Drawing.Brush brush, System.Drawing.RectangleF layoutRectangle) => throw new PlatformNotSupportedException();
        public void DrawString(string s, System.Drawing.Font font, System.Drawing.Brush brush, System.Drawing.RectangleF layoutRectangle, System.Drawing.StringFormat format) => throw new PlatformNotSupportedException();
        public void DrawString(string s, System.Drawing.Font font, System.Drawing.Brush brush, float x, float y) => throw new PlatformNotSupportedException();
        public void DrawString(string s, System.Drawing.Font font, System.Drawing.Brush brush, float x, float y, System.Drawing.StringFormat format) => throw new PlatformNotSupportedException();
        public void EndContainer(System.Drawing.Drawing2D.GraphicsContainer container) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Point destPoint, System.Drawing.Graphics.EnumerateMetafileProc callback) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Point destPoint, System.Drawing.Graphics.EnumerateMetafileProc callback, System.IntPtr callbackData) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Point destPoint, System.Drawing.Graphics.EnumerateMetafileProc callback, System.IntPtr callbackData, System.Drawing.Imaging.ImageAttributes imageAttr) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Point destPoint, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Point destPoint, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback, System.IntPtr callbackData) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Point destPoint, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit unit, System.Drawing.Graphics.EnumerateMetafileProc callback, System.IntPtr callbackData, System.Drawing.Imaging.ImageAttributes imageAttr) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.PointF destPoint, System.Drawing.Graphics.EnumerateMetafileProc callback) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.PointF destPoint, System.Drawing.Graphics.EnumerateMetafileProc callback, System.IntPtr callbackData) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.PointF destPoint, System.Drawing.Graphics.EnumerateMetafileProc callback, System.IntPtr callbackData, System.Drawing.Imaging.ImageAttributes imageAttr) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.PointF destPoint, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.PointF destPoint, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback, System.IntPtr callbackData) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.PointF destPoint, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit unit, System.Drawing.Graphics.EnumerateMetafileProc callback, System.IntPtr callbackData, System.Drawing.Imaging.ImageAttributes imageAttr) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.PointF[] destPoints, System.Drawing.Graphics.EnumerateMetafileProc callback) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.PointF[] destPoints, System.Drawing.Graphics.EnumerateMetafileProc callback, System.IntPtr callbackData) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.PointF[] destPoints, System.Drawing.Graphics.EnumerateMetafileProc callback, System.IntPtr callbackData, System.Drawing.Imaging.ImageAttributes imageAttr) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.PointF[] destPoints, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.PointF[] destPoints, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback, System.IntPtr callbackData) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.PointF[] destPoints, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit unit, System.Drawing.Graphics.EnumerateMetafileProc callback, System.IntPtr callbackData, System.Drawing.Imaging.ImageAttributes imageAttr) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Point[] destPoints, System.Drawing.Graphics.EnumerateMetafileProc callback) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Point[] destPoints, System.Drawing.Graphics.EnumerateMetafileProc callback, System.IntPtr callbackData) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Point[] destPoints, System.Drawing.Graphics.EnumerateMetafileProc callback, System.IntPtr callbackData, System.Drawing.Imaging.ImageAttributes imageAttr) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Point[] destPoints, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Point[] destPoints, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback, System.IntPtr callbackData) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Point[] destPoints, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit unit, System.Drawing.Graphics.EnumerateMetafileProc callback, System.IntPtr callbackData, System.Drawing.Imaging.ImageAttributes imageAttr) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Rectangle destRect, System.Drawing.Graphics.EnumerateMetafileProc callback) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Rectangle destRect, System.Drawing.Graphics.EnumerateMetafileProc callback, System.IntPtr callbackData) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Rectangle destRect, System.Drawing.Graphics.EnumerateMetafileProc callback, System.IntPtr callbackData, System.Drawing.Imaging.ImageAttributes imageAttr) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Rectangle destRect, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Rectangle destRect, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback, System.IntPtr callbackData) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Rectangle destRect, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit unit, System.Drawing.Graphics.EnumerateMetafileProc callback, System.IntPtr callbackData, System.Drawing.Imaging.ImageAttributes imageAttr) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.RectangleF destRect, System.Drawing.Graphics.EnumerateMetafileProc callback) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.RectangleF destRect, System.Drawing.Graphics.EnumerateMetafileProc callback, System.IntPtr callbackData) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.RectangleF destRect, System.Drawing.Graphics.EnumerateMetafileProc callback, System.IntPtr callbackData, System.Drawing.Imaging.ImageAttributes imageAttr) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.RectangleF destRect, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.RectangleF destRect, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback, System.IntPtr callbackData) => throw new PlatformNotSupportedException();
        public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.RectangleF destRect, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit unit, System.Drawing.Graphics.EnumerateMetafileProc callback, System.IntPtr callbackData, System.Drawing.Imaging.ImageAttributes imageAttr) => throw new PlatformNotSupportedException();
        public void ExcludeClip(System.Drawing.Rectangle rect) => throw new PlatformNotSupportedException();
        public void ExcludeClip(System.Drawing.Region region) => throw new PlatformNotSupportedException();
        public void FillClosedCurve(System.Drawing.Brush brush, System.Drawing.PointF[] points) => throw new PlatformNotSupportedException();
        public void FillClosedCurve(System.Drawing.Brush brush, System.Drawing.PointF[] points, System.Drawing.Drawing2D.FillMode fillmode) => throw new PlatformNotSupportedException();
        public void FillClosedCurve(System.Drawing.Brush brush, System.Drawing.PointF[] points, System.Drawing.Drawing2D.FillMode fillmode, float tension) => throw new PlatformNotSupportedException();
        public void FillClosedCurve(System.Drawing.Brush brush, System.Drawing.Point[] points) => throw new PlatformNotSupportedException();
        public void FillClosedCurve(System.Drawing.Brush brush, System.Drawing.Point[] points, System.Drawing.Drawing2D.FillMode fillmode) => throw new PlatformNotSupportedException();
        public void FillClosedCurve(System.Drawing.Brush brush, System.Drawing.Point[] points, System.Drawing.Drawing2D.FillMode fillmode, float tension) => throw new PlatformNotSupportedException();
        public void FillEllipse(System.Drawing.Brush brush, System.Drawing.Rectangle rect) => throw new PlatformNotSupportedException();
        public void FillEllipse(System.Drawing.Brush brush, System.Drawing.RectangleF rect) => throw new PlatformNotSupportedException();
        public void FillEllipse(System.Drawing.Brush brush, int x, int y, int width, int height) => throw new PlatformNotSupportedException();
        public void FillEllipse(System.Drawing.Brush brush, float x, float y, float width, float height) => throw new PlatformNotSupportedException();
        public void FillPath(System.Drawing.Brush brush, System.Drawing.Drawing2D.GraphicsPath path) => throw new PlatformNotSupportedException();
        public void FillPie(System.Drawing.Brush brush, System.Drawing.Rectangle rect, float startAngle, float sweepAngle) => throw new PlatformNotSupportedException();
        public void FillPie(System.Drawing.Brush brush, int x, int y, int width, int height, int startAngle, int sweepAngle) => throw new PlatformNotSupportedException();
        public void FillPie(System.Drawing.Brush brush, float x, float y, float width, float height, float startAngle, float sweepAngle) => throw new PlatformNotSupportedException();
        public void FillPolygon(System.Drawing.Brush brush, System.Drawing.PointF[] points) => throw new PlatformNotSupportedException();
        public void FillPolygon(System.Drawing.Brush brush, System.Drawing.PointF[] points, System.Drawing.Drawing2D.FillMode fillMode) => throw new PlatformNotSupportedException();
        public void FillPolygon(System.Drawing.Brush brush, System.Drawing.Point[] points) => throw new PlatformNotSupportedException();
        public void FillPolygon(System.Drawing.Brush brush, System.Drawing.Point[] points, System.Drawing.Drawing2D.FillMode fillMode) => throw new PlatformNotSupportedException();
        public void FillRectangle(System.Drawing.Brush brush, System.Drawing.Rectangle rect) => throw new PlatformNotSupportedException();
        public void FillRectangle(System.Drawing.Brush brush, System.Drawing.RectangleF rect) => throw new PlatformNotSupportedException();
        public void FillRectangle(System.Drawing.Brush brush, int x, int y, int width, int height) => throw new PlatformNotSupportedException();
        public void FillRectangle(System.Drawing.Brush brush, float x, float y, float width, float height) => throw new PlatformNotSupportedException();
        public void FillRectangles(System.Drawing.Brush brush, System.Drawing.RectangleF[] rects) => throw new PlatformNotSupportedException();
        public void FillRectangles(System.Drawing.Brush brush, System.Drawing.Rectangle[] rects) => throw new PlatformNotSupportedException();
        public void FillRegion(System.Drawing.Brush brush, System.Drawing.Region region) => throw new PlatformNotSupportedException();
        ~Graphics() => throw new PlatformNotSupportedException();
        public void Flush() => throw new PlatformNotSupportedException();
        public void Flush(System.Drawing.Drawing2D.FlushIntention intention) => throw new PlatformNotSupportedException();
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(2))]
        public static System.Drawing.Graphics FromHdc(System.IntPtr hdc) { throw new PlatformNotSupportedException(); }
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(2))]
        public static System.Drawing.Graphics FromHdc(System.IntPtr hdc, System.IntPtr hdevice) { throw new PlatformNotSupportedException(); }
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(2))]
        public static System.Drawing.Graphics FromHdcInternal(System.IntPtr hdc) { throw new PlatformNotSupportedException(); }
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(2))]
        public static System.Drawing.Graphics FromHwnd(System.IntPtr hwnd) { throw new PlatformNotSupportedException(); }
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(2))]
        public static System.Drawing.Graphics FromHwndInternal(System.IntPtr hwnd) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Graphics FromImage(System.Drawing.Image image) { throw new PlatformNotSupportedException(); }
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
        public object GetContextInfo() { throw new PlatformNotSupportedException(); }
        public static System.IntPtr GetHalftonePalette() { throw new PlatformNotSupportedException(); }
        public System.IntPtr GetHdc() { throw new PlatformNotSupportedException(); }
        public System.Drawing.Color GetNearestColor(System.Drawing.Color color) { throw new PlatformNotSupportedException(); }
        public void IntersectClip(System.Drawing.Rectangle rect) => throw new PlatformNotSupportedException();
        public void IntersectClip(System.Drawing.RectangleF rect) => throw new PlatformNotSupportedException();
        public void IntersectClip(System.Drawing.Region region) => throw new PlatformNotSupportedException();
        public bool IsVisible(System.Drawing.Point point) { throw new PlatformNotSupportedException(); }
        public bool IsVisible(System.Drawing.PointF point) { throw new PlatformNotSupportedException(); }
        public bool IsVisible(System.Drawing.Rectangle rect) { throw new PlatformNotSupportedException(); }
        public bool IsVisible(System.Drawing.RectangleF rect) { throw new PlatformNotSupportedException(); }
        public bool IsVisible(int x, int y) { throw new PlatformNotSupportedException(); }
        public bool IsVisible(int x, int y, int width, int height) { throw new PlatformNotSupportedException(); }
        public bool IsVisible(float x, float y) { throw new PlatformNotSupportedException(); }
        public bool IsVisible(float x, float y, float width, float height) { throw new PlatformNotSupportedException(); }
        public System.Drawing.Region[] MeasureCharacterRanges(string text, System.Drawing.Font font, System.Drawing.RectangleF layoutRect, System.Drawing.StringFormat stringFormat) { throw new PlatformNotSupportedException(); }
        public System.Drawing.SizeF MeasureString(string text, System.Drawing.Font font) { throw new PlatformNotSupportedException(); }
        public System.Drawing.SizeF MeasureString(string text, System.Drawing.Font font, System.Drawing.PointF origin, System.Drawing.StringFormat stringFormat) { throw new PlatformNotSupportedException(); }
        public System.Drawing.SizeF MeasureString(string text, System.Drawing.Font font, System.Drawing.SizeF layoutArea) { throw new PlatformNotSupportedException(); }
        public System.Drawing.SizeF MeasureString(string text, System.Drawing.Font font, System.Drawing.SizeF layoutArea, System.Drawing.StringFormat stringFormat) { throw new PlatformNotSupportedException(); }
        public System.Drawing.SizeF MeasureString(string text, System.Drawing.Font font, System.Drawing.SizeF layoutArea, System.Drawing.StringFormat stringFormat, out int charactersFitted, out int linesFilled) { throw new PlatformNotSupportedException(); }
        public System.Drawing.SizeF MeasureString(string text, System.Drawing.Font font, int width) { throw new PlatformNotSupportedException(); }
        public System.Drawing.SizeF MeasureString(string text, System.Drawing.Font font, int width, System.Drawing.StringFormat format) { throw new PlatformNotSupportedException(); }
        public void MultiplyTransform(System.Drawing.Drawing2D.Matrix matrix) => throw new PlatformNotSupportedException();
        public void MultiplyTransform(System.Drawing.Drawing2D.Matrix matrix, System.Drawing.Drawing2D.MatrixOrder order) => throw new PlatformNotSupportedException();
        public void ReleaseHdc() => throw new PlatformNotSupportedException();
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(2))]
        public void ReleaseHdc(System.IntPtr hdc) => throw new PlatformNotSupportedException();
        [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
        public void ReleaseHdcInternal(System.IntPtr hdc) => throw new PlatformNotSupportedException();
        public void ResetClip() => throw new PlatformNotSupportedException();
        public void ResetTransform() => throw new PlatformNotSupportedException();
        public void Restore(System.Drawing.Drawing2D.GraphicsState gstate) => throw new PlatformNotSupportedException();
        public void RotateTransform(float angle) => throw new PlatformNotSupportedException();
        public void RotateTransform(float angle, System.Drawing.Drawing2D.MatrixOrder order) => throw new PlatformNotSupportedException();
        public System.Drawing.Drawing2D.GraphicsState Save() { throw new PlatformNotSupportedException(); }
        public void ScaleTransform(float sx, float sy) => throw new PlatformNotSupportedException();
        public void ScaleTransform(float sx, float sy, System.Drawing.Drawing2D.MatrixOrder order) => throw new PlatformNotSupportedException();
        public void SetClip(System.Drawing.Drawing2D.GraphicsPath path) => throw new PlatformNotSupportedException();
        public void SetClip(System.Drawing.Drawing2D.GraphicsPath path, System.Drawing.Drawing2D.CombineMode combineMode) => throw new PlatformNotSupportedException();
        public void SetClip(System.Drawing.Graphics g) => throw new PlatformNotSupportedException();
        public void SetClip(System.Drawing.Graphics g, System.Drawing.Drawing2D.CombineMode combineMode) => throw new PlatformNotSupportedException();
        public void SetClip(System.Drawing.Rectangle rect) => throw new PlatformNotSupportedException();
        public void SetClip(System.Drawing.Rectangle rect, System.Drawing.Drawing2D.CombineMode combineMode) => throw new PlatformNotSupportedException();
        public void SetClip(System.Drawing.RectangleF rect) => throw new PlatformNotSupportedException();
        public void SetClip(System.Drawing.RectangleF rect, System.Drawing.Drawing2D.CombineMode combineMode) => throw new PlatformNotSupportedException();
        public void SetClip(System.Drawing.Region region, System.Drawing.Drawing2D.CombineMode combineMode) => throw new PlatformNotSupportedException();
        public void TransformPoints(System.Drawing.Drawing2D.CoordinateSpace destSpace, System.Drawing.Drawing2D.CoordinateSpace srcSpace, System.Drawing.PointF[] pts) => throw new PlatformNotSupportedException();
        public void TransformPoints(System.Drawing.Drawing2D.CoordinateSpace destSpace, System.Drawing.Drawing2D.CoordinateSpace srcSpace, System.Drawing.Point[] pts) => throw new PlatformNotSupportedException();
        public void TranslateClip(int dx, int dy) => throw new PlatformNotSupportedException();
        public void TranslateClip(float dx, float dy) => throw new PlatformNotSupportedException();
        public void TranslateTransform(float dx, float dy) => throw new PlatformNotSupportedException();
        public void TranslateTransform(float dx, float dy, System.Drawing.Drawing2D.MatrixOrder order) => throw new PlatformNotSupportedException();
        public delegate bool DrawImageAbort(System.IntPtr callbackdata);
        public delegate bool EnumerateMetafileProc(System.Drawing.Imaging.EmfPlusRecordType recordType, int flags, int dataSize, System.IntPtr data, System.Drawing.Imaging.PlayRecordCallback callbackData);
    }
    public enum GraphicsUnit
    {
        Display = 1,
        Document = 5,
        Inch = 4,
        Millimeter = 6,
        Pixel = 2,
        Point = 3,
        World = 0,
    }
#if netcoreapp
    [System.ComponentModel.TypeConverter("System.Drawing.IconConverter, System.Windows.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51")]
#endif
    public sealed partial class Icon : System.MarshalByRefObject, System.ICloneable, System.IDisposable, System.Runtime.Serialization.ISerializable
    {
        public Icon(System.Drawing.Icon original, System.Drawing.Size size) => throw new PlatformNotSupportedException();
        public Icon(System.Drawing.Icon original, int width, int height) => throw new PlatformNotSupportedException();
        public Icon(System.IO.Stream stream) => throw new PlatformNotSupportedException();
        public Icon(System.IO.Stream stream, System.Drawing.Size size) => throw new PlatformNotSupportedException();
        public Icon(System.IO.Stream stream, int width, int height) => throw new PlatformNotSupportedException();
        public Icon(string fileName) => throw new PlatformNotSupportedException();
        public Icon(string fileName, System.Drawing.Size size) => throw new PlatformNotSupportedException();
        public Icon(string fileName, int width, int height) => throw new PlatformNotSupportedException();
        public Icon(System.Type type, string resource) => throw new PlatformNotSupportedException();
        [System.ComponentModel.BrowsableAttribute(false)]
        public System.IntPtr Handle { get { throw new PlatformNotSupportedException(); } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public int Height { get { throw new PlatformNotSupportedException(); } }
        public System.Drawing.Size Size { get { throw new PlatformNotSupportedException(); } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public int Width { get { throw new PlatformNotSupportedException(); } }
        public object Clone() { throw new PlatformNotSupportedException(); }
        public void Dispose() => throw new PlatformNotSupportedException();
        public static System.Drawing.Icon ExtractAssociatedIcon(string filePath) { throw new PlatformNotSupportedException(); }
        ~Icon() => throw new PlatformNotSupportedException();
        public static System.Drawing.Icon FromHandle(System.IntPtr handle) { throw new PlatformNotSupportedException(); }
        public void Save(System.IO.Stream outputStream) => throw new PlatformNotSupportedException();
        void System.Runtime.Serialization.ISerializable.GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) => throw new PlatformNotSupportedException();
        public System.Drawing.Bitmap ToBitmap() { throw new PlatformNotSupportedException(); }
        public override string ToString() { throw new PlatformNotSupportedException(); }
    }
    public partial interface IDeviceContext : System.IDisposable
    {
        System.IntPtr GetHdc();
        void ReleaseHdc();
    }
#if netcoreapp
    [System.ComponentModel.TypeConverter("System.Drawing.ImageConverter, System.Windows.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51")]
#endif
    [System.ComponentModel.ImmutableObjectAttribute(true)]
    public abstract partial class Image : System.MarshalByRefObject, System.ICloneable, System.IDisposable, System.Runtime.Serialization.ISerializable
    {
        internal Image() => throw new PlatformNotSupportedException();
        [System.ComponentModel.BrowsableAttribute(false)]
        public int Flags { get { throw new PlatformNotSupportedException(); } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public System.Guid[] FrameDimensionsList { get { throw new PlatformNotSupportedException(); } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DefaultValueAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public int Height { get { throw new PlatformNotSupportedException(); } }
        public float HorizontalResolution { get { throw new PlatformNotSupportedException(); } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public System.Drawing.Imaging.ColorPalette Palette { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.SizeF PhysicalDimension { get { throw new PlatformNotSupportedException(); } }
        public System.Drawing.Imaging.PixelFormat PixelFormat { get { throw new PlatformNotSupportedException(); } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public int[] PropertyIdList { get { throw new PlatformNotSupportedException(); } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public System.Drawing.Imaging.PropertyItem[] PropertyItems { get { throw new PlatformNotSupportedException(); } }
        public System.Drawing.Imaging.ImageFormat RawFormat { get { throw new PlatformNotSupportedException(); } }
        public System.Drawing.Size Size { get { throw new PlatformNotSupportedException(); } }
        [System.ComponentModel.DefaultValueAttribute(null)]
        [System.ComponentModel.LocalizableAttribute(false)]
        public object Tag { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float VerticalResolution { get { throw new PlatformNotSupportedException(); } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DefaultValueAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public int Width { get { throw new PlatformNotSupportedException(); } }
        public object Clone() { throw new PlatformNotSupportedException(); }
        public void Dispose() => throw new PlatformNotSupportedException();
        protected virtual void Dispose(bool disposing) => throw new PlatformNotSupportedException();
        ~Image() => throw new PlatformNotSupportedException();
        public static System.Drawing.Image FromFile(string filename) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Image FromFile(string filename, bool useEmbeddedColorManagement) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Bitmap FromHbitmap(System.IntPtr hbitmap) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Bitmap FromHbitmap(System.IntPtr hbitmap, System.IntPtr hpalette) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Image FromStream(System.IO.Stream stream) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Image FromStream(System.IO.Stream stream, bool useEmbeddedColorManagement) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Image FromStream(System.IO.Stream stream, bool useEmbeddedColorManagement, bool validateImageData) { throw new PlatformNotSupportedException(); }
        public System.Drawing.RectangleF GetBounds(ref System.Drawing.GraphicsUnit pageUnit) { throw new PlatformNotSupportedException(); }
        public System.Drawing.Imaging.EncoderParameters GetEncoderParameterList(System.Guid encoder) { throw new PlatformNotSupportedException(); }
        public int GetFrameCount(System.Drawing.Imaging.FrameDimension dimension) { throw new PlatformNotSupportedException(); }
        public static int GetPixelFormatSize(System.Drawing.Imaging.PixelFormat pixfmt) { throw new PlatformNotSupportedException(); }
        public System.Drawing.Imaging.PropertyItem GetPropertyItem(int propid) { throw new PlatformNotSupportedException(); }
        public System.Drawing.Image GetThumbnailImage(int thumbWidth, int thumbHeight, System.Drawing.Image.GetThumbnailImageAbort callback, System.IntPtr callbackData) { throw new PlatformNotSupportedException(); }
        public static bool IsAlphaPixelFormat(System.Drawing.Imaging.PixelFormat pixfmt) { throw new PlatformNotSupportedException(); }
        public static bool IsCanonicalPixelFormat(System.Drawing.Imaging.PixelFormat pixfmt) { throw new PlatformNotSupportedException(); }
        public static bool IsExtendedPixelFormat(System.Drawing.Imaging.PixelFormat pixfmt) { throw new PlatformNotSupportedException(); }
        public void RemovePropertyItem(int propid) => throw new PlatformNotSupportedException();
        public void RotateFlip(System.Drawing.RotateFlipType rotateFlipType) => throw new PlatformNotSupportedException();
        public void Save(System.IO.Stream stream, System.Drawing.Imaging.ImageCodecInfo encoder, System.Drawing.Imaging.EncoderParameters encoderParams) => throw new PlatformNotSupportedException();
        public void Save(System.IO.Stream stream, System.Drawing.Imaging.ImageFormat format) => throw new PlatformNotSupportedException();
        public void Save(string filename) => throw new PlatformNotSupportedException();
        public void Save(string filename, System.Drawing.Imaging.ImageCodecInfo encoder, System.Drawing.Imaging.EncoderParameters encoderParams) => throw new PlatformNotSupportedException();
        public void Save(string filename, System.Drawing.Imaging.ImageFormat format) => throw new PlatformNotSupportedException();
        public void SaveAdd(System.Drawing.Image image, System.Drawing.Imaging.EncoderParameters encoderParams) => throw new PlatformNotSupportedException();
        public void SaveAdd(System.Drawing.Imaging.EncoderParameters encoderParams) => throw new PlatformNotSupportedException();
        public int SelectActiveFrame(System.Drawing.Imaging.FrameDimension dimension, int frameIndex) { throw new PlatformNotSupportedException(); }
        public void SetPropertyItem(System.Drawing.Imaging.PropertyItem propitem) => throw new PlatformNotSupportedException();
        void System.Runtime.Serialization.ISerializable.GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) => throw new PlatformNotSupportedException();
        public delegate bool GetThumbnailImageAbort();
    }
    public sealed partial class ImageAnimator
    {
        internal ImageAnimator() => throw new PlatformNotSupportedException();
        public static void Animate(System.Drawing.Image image, System.EventHandler onFrameChangedHandler) => throw new PlatformNotSupportedException();
        public static bool CanAnimate(System.Drawing.Image image) { throw new PlatformNotSupportedException(); }
        public static void StopAnimate(System.Drawing.Image image, System.EventHandler onFrameChangedHandler) => throw new PlatformNotSupportedException();
        public static void UpdateFrames() => throw new PlatformNotSupportedException();
        public static void UpdateFrames(System.Drawing.Image image) => throw new PlatformNotSupportedException();
    }
    public sealed partial class Pen : System.MarshalByRefObject, System.ICloneable, System.IDisposable
    {
        public Pen(System.Drawing.Brush brush) => throw new PlatformNotSupportedException();
        public Pen(System.Drawing.Brush brush, float width) => throw new PlatformNotSupportedException();
        public Pen(System.Drawing.Color color) => throw new PlatformNotSupportedException();
        public Pen(System.Drawing.Color color, float width) => throw new PlatformNotSupportedException();
        public System.Drawing.Drawing2D.PenAlignment Alignment { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Brush Brush { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Color Color { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float[] CompoundArray { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Drawing2D.CustomLineCap CustomEndCap { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Drawing2D.CustomLineCap CustomStartCap { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Drawing2D.DashCap DashCap { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float DashOffset { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float[] DashPattern { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Drawing2D.DashStyle DashStyle { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Drawing2D.LineCap EndCap { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Drawing2D.LineJoin LineJoin { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float MiterLimit { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Drawing2D.PenType PenType { get { throw new PlatformNotSupportedException(); } }
        public System.Drawing.Drawing2D.LineCap StartCap { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Drawing2D.Matrix Transform { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float Width { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public object Clone() { throw new PlatformNotSupportedException(); }
        public void Dispose() => throw new PlatformNotSupportedException();
        ~Pen() => throw new PlatformNotSupportedException();
        public void MultiplyTransform(System.Drawing.Drawing2D.Matrix matrix) => throw new PlatformNotSupportedException();
        public void MultiplyTransform(System.Drawing.Drawing2D.Matrix matrix, System.Drawing.Drawing2D.MatrixOrder order) => throw new PlatformNotSupportedException();
        public void ResetTransform() => throw new PlatformNotSupportedException();
        public void RotateTransform(float angle) => throw new PlatformNotSupportedException();
        public void RotateTransform(float angle, System.Drawing.Drawing2D.MatrixOrder order) => throw new PlatformNotSupportedException();
        public void ScaleTransform(float sx, float sy) => throw new PlatformNotSupportedException();
        public void ScaleTransform(float sx, float sy, System.Drawing.Drawing2D.MatrixOrder order) => throw new PlatformNotSupportedException();
        public void SetLineCap(System.Drawing.Drawing2D.LineCap startCap, System.Drawing.Drawing2D.LineCap endCap, System.Drawing.Drawing2D.DashCap dashCap) => throw new PlatformNotSupportedException();
        public void TranslateTransform(float dx, float dy) => throw new PlatformNotSupportedException();
        public void TranslateTransform(float dx, float dy, System.Drawing.Drawing2D.MatrixOrder order) => throw new PlatformNotSupportedException();
    }
    public static partial class Pens
    {
        public static System.Drawing.Pen AliceBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen AntiqueWhite { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Aqua { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Aquamarine { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Azure { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Beige { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Bisque { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Black { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen BlanchedAlmond { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Blue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen BlueViolet { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Brown { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen BurlyWood { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen CadetBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Chartreuse { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Chocolate { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Coral { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen CornflowerBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Cornsilk { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Crimson { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Cyan { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen DarkBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen DarkCyan { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen DarkGoldenrod { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen DarkGray { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen DarkGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen DarkKhaki { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen DarkMagenta { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen DarkOliveGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen DarkOrange { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen DarkOrchid { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen DarkRed { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen DarkSalmon { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen DarkSeaGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen DarkSlateBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen DarkSlateGray { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen DarkTurquoise { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen DarkViolet { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen DeepPink { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen DeepSkyBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen DimGray { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen DodgerBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Firebrick { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen FloralWhite { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen ForestGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Fuchsia { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Gainsboro { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen GhostWhite { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Gold { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Goldenrod { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Gray { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Green { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen GreenYellow { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Honeydew { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen HotPink { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen IndianRed { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Indigo { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Ivory { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Khaki { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Lavender { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen LavenderBlush { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen LawnGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen LemonChiffon { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen LightBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen LightCoral { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen LightCyan { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen LightGoldenrodYellow { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen LightGray { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen LightGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen LightPink { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen LightSalmon { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen LightSeaGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen LightSkyBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen LightSlateGray { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen LightSteelBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen LightYellow { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Lime { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen LimeGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Linen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Magenta { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Maroon { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen MediumAquamarine { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen MediumBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen MediumOrchid { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen MediumPurple { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen MediumSeaGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen MediumSlateBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen MediumSpringGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen MediumTurquoise { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen MediumVioletRed { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen MidnightBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen MintCream { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen MistyRose { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Moccasin { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen NavajoWhite { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Navy { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen OldLace { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Olive { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen OliveDrab { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Orange { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen OrangeRed { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Orchid { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen PaleGoldenrod { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen PaleGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen PaleTurquoise { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen PaleVioletRed { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen PapayaWhip { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen PeachPuff { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Peru { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Pink { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Plum { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen PowderBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Purple { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Red { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen RosyBrown { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen RoyalBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen SaddleBrown { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Salmon { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen SandyBrown { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen SeaGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen SeaShell { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Sienna { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Silver { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen SkyBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen SlateBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen SlateGray { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Snow { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen SpringGreen { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen SteelBlue { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Tan { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Teal { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Thistle { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Tomato { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Transparent { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Turquoise { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Violet { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Wheat { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen White { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen WhiteSmoke { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Yellow { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen YellowGreen { get { throw new PlatformNotSupportedException(); } }
    }
    public sealed partial class Region : System.MarshalByRefObject, System.IDisposable
    {
        public Region() => throw new PlatformNotSupportedException();
        public Region(System.Drawing.Drawing2D.GraphicsPath path) => throw new PlatformNotSupportedException();
        public Region(System.Drawing.Drawing2D.RegionData rgnData) => throw new PlatformNotSupportedException();
        public Region(System.Drawing.Rectangle rect) => throw new PlatformNotSupportedException();
        public Region(System.Drawing.RectangleF rect) => throw new PlatformNotSupportedException();
        public System.Drawing.Region Clone() { throw new PlatformNotSupportedException(); }
        public void Complement(System.Drawing.Drawing2D.GraphicsPath path) => throw new PlatformNotSupportedException();
        public void Complement(System.Drawing.Rectangle rect) => throw new PlatformNotSupportedException();
        public void Complement(System.Drawing.RectangleF rect) => throw new PlatformNotSupportedException();
        public void Complement(System.Drawing.Region region) => throw new PlatformNotSupportedException();
        public void Dispose() => throw new PlatformNotSupportedException();
        public bool Equals(System.Drawing.Region region, System.Drawing.Graphics g) { throw new PlatformNotSupportedException(); }
        public void Exclude(System.Drawing.Drawing2D.GraphicsPath path) => throw new PlatformNotSupportedException();
        public void Exclude(System.Drawing.Rectangle rect) => throw new PlatformNotSupportedException();
        public void Exclude(System.Drawing.RectangleF rect) => throw new PlatformNotSupportedException();
        public void Exclude(System.Drawing.Region region) => throw new PlatformNotSupportedException();
        ~Region() => throw new PlatformNotSupportedException();
        public static System.Drawing.Region FromHrgn(System.IntPtr hrgn) { throw new PlatformNotSupportedException(); }
        public System.Drawing.RectangleF GetBounds(System.Drawing.Graphics g) { throw new PlatformNotSupportedException(); }
        public System.IntPtr GetHrgn(System.Drawing.Graphics g) { throw new PlatformNotSupportedException(); }
        public System.Drawing.Drawing2D.RegionData GetRegionData() { throw new PlatformNotSupportedException(); }
        public System.Drawing.RectangleF[] GetRegionScans(System.Drawing.Drawing2D.Matrix matrix) { throw new PlatformNotSupportedException(); }
        public void Intersect(System.Drawing.Drawing2D.GraphicsPath path) => throw new PlatformNotSupportedException();
        public void Intersect(System.Drawing.Rectangle rect) => throw new PlatformNotSupportedException();
        public void Intersect(System.Drawing.RectangleF rect) => throw new PlatformNotSupportedException();
        public void Intersect(System.Drawing.Region region) => throw new PlatformNotSupportedException();
        public bool IsEmpty(System.Drawing.Graphics g) { throw new PlatformNotSupportedException(); }
        public bool IsInfinite(System.Drawing.Graphics g) { throw new PlatformNotSupportedException(); }
        public bool IsVisible(System.Drawing.Point point) { throw new PlatformNotSupportedException(); }
        public bool IsVisible(System.Drawing.Point point, System.Drawing.Graphics g) { throw new PlatformNotSupportedException(); }
        public bool IsVisible(System.Drawing.PointF point) { throw new PlatformNotSupportedException(); }
        public bool IsVisible(System.Drawing.PointF point, System.Drawing.Graphics g) { throw new PlatformNotSupportedException(); }
        public bool IsVisible(System.Drawing.Rectangle rect) { throw new PlatformNotSupportedException(); }
        public bool IsVisible(System.Drawing.Rectangle rect, System.Drawing.Graphics g) { throw new PlatformNotSupportedException(); }
        public bool IsVisible(System.Drawing.RectangleF rect) { throw new PlatformNotSupportedException(); }
        public bool IsVisible(System.Drawing.RectangleF rect, System.Drawing.Graphics g) { throw new PlatformNotSupportedException(); }
        public bool IsVisible(int x, int y, System.Drawing.Graphics g) { throw new PlatformNotSupportedException(); }
        public bool IsVisible(int x, int y, int width, int height) { throw new PlatformNotSupportedException(); }
        public bool IsVisible(int x, int y, int width, int height, System.Drawing.Graphics g) { throw new PlatformNotSupportedException(); }
        public bool IsVisible(float x, float y) { throw new PlatformNotSupportedException(); }
        public bool IsVisible(float x, float y, System.Drawing.Graphics g) { throw new PlatformNotSupportedException(); }
        public bool IsVisible(float x, float y, float width, float height) { throw new PlatformNotSupportedException(); }
        public bool IsVisible(float x, float y, float width, float height, System.Drawing.Graphics g) { throw new PlatformNotSupportedException(); }
        public void MakeEmpty() => throw new PlatformNotSupportedException();
        public void MakeInfinite() => throw new PlatformNotSupportedException();
        public void ReleaseHrgn(System.IntPtr regionHandle) => throw new PlatformNotSupportedException();
        public void Transform(System.Drawing.Drawing2D.Matrix matrix) => throw new PlatformNotSupportedException();
        public void Translate(int dx, int dy) => throw new PlatformNotSupportedException();
        public void Translate(float dx, float dy) => throw new PlatformNotSupportedException();
        public void Union(System.Drawing.Drawing2D.GraphicsPath path) => throw new PlatformNotSupportedException();
        public void Union(System.Drawing.Rectangle rect) => throw new PlatformNotSupportedException();
        public void Union(System.Drawing.RectangleF rect) => throw new PlatformNotSupportedException();
        public void Union(System.Drawing.Region region) => throw new PlatformNotSupportedException();
        public void Xor(System.Drawing.Drawing2D.GraphicsPath path) => throw new PlatformNotSupportedException();
        public void Xor(System.Drawing.Rectangle rect) => throw new PlatformNotSupportedException();
        public void Xor(System.Drawing.RectangleF rect) => throw new PlatformNotSupportedException();
        public void Xor(System.Drawing.Region region) => throw new PlatformNotSupportedException();
    }
    public enum RotateFlipType
    {
        Rotate180FlipNone = 2,
        Rotate180FlipX = 6,
        Rotate180FlipXY = 0,
        Rotate180FlipY = 4,
        Rotate270FlipNone = 3,
        Rotate270FlipX = 7,
        Rotate270FlipXY = 1,
        Rotate270FlipY = 5,
        Rotate90FlipNone = 1,
        Rotate90FlipX = 5,
        Rotate90FlipXY = 3,
        Rotate90FlipY = 7,
        RotateNoneFlipNone = 0,
        RotateNoneFlipX = 4,
        RotateNoneFlipXY = 2,
        RotateNoneFlipY = 6,
    }
    public sealed partial class SolidBrush : System.Drawing.Brush
    {
        public SolidBrush(System.Drawing.Color color) => throw new PlatformNotSupportedException();
        public System.Drawing.Color Color { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public override object Clone() { throw new PlatformNotSupportedException(); }
        protected override void Dispose(bool disposing) => throw new PlatformNotSupportedException();
    }
    public enum StringAlignment
    {
        Center = 1,
        Far = 2,
        Near = 0,
    }
    public enum StringDigitSubstitute
    {
        National = 2,
        None = 1,
        Traditional = 3,
        User = 0,
    }
    public sealed partial class StringFormat : System.MarshalByRefObject, System.ICloneable, System.IDisposable
    {
        public StringFormat() => throw new PlatformNotSupportedException();
        public StringFormat(System.Drawing.StringFormat format) => throw new PlatformNotSupportedException();
        public StringFormat(System.Drawing.StringFormatFlags options) => throw new PlatformNotSupportedException();
        public StringFormat(System.Drawing.StringFormatFlags options, int language) => throw new PlatformNotSupportedException();
        public System.Drawing.StringAlignment Alignment { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public int DigitSubstitutionLanguage { get { throw new PlatformNotSupportedException(); } }
        public System.Drawing.StringDigitSubstitute DigitSubstitutionMethod { get { throw new PlatformNotSupportedException(); } }
        public System.Drawing.StringFormatFlags FormatFlags { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public static System.Drawing.StringFormat GenericDefault { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.StringFormat GenericTypographic { get { throw new PlatformNotSupportedException(); } }
        public System.Drawing.Text.HotkeyPrefix HotkeyPrefix { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.StringAlignment LineAlignment { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.StringTrimming Trimming { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public object Clone() { throw new PlatformNotSupportedException(); }
        public void Dispose() => throw new PlatformNotSupportedException();
        ~StringFormat() => throw new PlatformNotSupportedException();
        public float[] GetTabStops(out float firstTabOffset) { throw new PlatformNotSupportedException(); }
        public void SetDigitSubstitution(int language, System.Drawing.StringDigitSubstitute substitute) => throw new PlatformNotSupportedException();
        public void SetMeasurableCharacterRanges(System.Drawing.CharacterRange[] ranges) => throw new PlatformNotSupportedException();
        public void SetTabStops(float firstTabOffset, float[] tabStops) => throw new PlatformNotSupportedException();
        public override string ToString() { throw new PlatformNotSupportedException(); }
    }
    [System.FlagsAttribute]
    public enum StringFormatFlags
    {
        DirectionRightToLeft = 1,
        DirectionVertical = 2,
        DisplayFormatControl = 32,
        FitBlackBox = 4,
        LineLimit = 8192,
        MeasureTrailingSpaces = 2048,
        NoClip = 16384,
        NoFontFallback = 1024,
        NoWrap = 4096,
    }
    public enum StringTrimming
    {
        Character = 1,
        EllipsisCharacter = 3,
        EllipsisPath = 5,
        EllipsisWord = 4,
        None = 0,
        Word = 2,
    }
    public enum StringUnit
    {
        Display = 1,
        Document = 5,
        Em = 32,
        Inch = 4,
        Millimeter = 6,
        Pixel = 2,
        Point = 3,
        World = 0,
    }
    public static partial class SystemBrushes
    {
        public static System.Drawing.Brush ActiveBorder { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush ActiveCaption { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush ActiveCaptionText { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush AppWorkspace { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush ButtonFace { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush ButtonHighlight { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush ButtonShadow { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Control { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush ControlDark { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush ControlDarkDark { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush ControlLight { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush ControlLightLight { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush ControlText { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Desktop { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush GradientActiveCaption { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush GradientInactiveCaption { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush GrayText { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Highlight { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush HighlightText { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush HotTrack { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush InactiveBorder { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush InactiveCaption { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush InactiveCaptionText { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Info { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush InfoText { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Menu { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush MenuBar { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush MenuHighlight { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush MenuText { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush ScrollBar { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush Window { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush WindowFrame { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush WindowText { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Brush FromSystemColor(System.Drawing.Color c) { throw new PlatformNotSupportedException(); }
    }

#if !MONODROID

    public static partial class SystemColors
    {
        public static System.Drawing.Color ActiveBorder { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color ActiveCaption { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color ActiveCaptionText { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color AppWorkspace { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color ButtonFace { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color ButtonHighlight { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color ButtonShadow { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Control { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color ControlDark { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color ControlDarkDark { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color ControlLight { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color ControlLightLight { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color ControlText { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Desktop { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color GradientActiveCaption { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color GradientInactiveCaption { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color GrayText { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Highlight { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color HighlightText { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color HotTrack { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color InactiveBorder { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color InactiveCaption { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color InactiveCaptionText { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Info { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color InfoText { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Menu { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color MenuBar { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color MenuHighlight { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color MenuText { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color ScrollBar { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color Window { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color WindowFrame { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Color WindowText { get { throw new PlatformNotSupportedException(); } }
    }

#endif // !MONODROID

    public static partial class SystemFonts
    {
        public static System.Drawing.Font CaptionFont { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Font DefaultFont { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Font DialogFont { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Font IconTitleFont { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Font MenuFont { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Font MessageBoxFont { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Font SmallCaptionFont { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Font StatusFont { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Font GetFontByName(string systemFontName) { throw new PlatformNotSupportedException(); }
    }
    public static partial class SystemIcons
    {
        public static System.Drawing.Icon Application { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Icon Asterisk { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Icon Error { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Icon Exclamation { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Icon Hand { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Icon Information { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Icon Question { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Icon Shield { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Icon Warning { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Icon WinLogo { get { throw new PlatformNotSupportedException(); } }
    }
    public static partial class SystemPens
    {
        public static System.Drawing.Pen ActiveBorder { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen ActiveCaption { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen ActiveCaptionText { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen AppWorkspace { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen ButtonFace { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen ButtonHighlight { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen ButtonShadow { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Control { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen ControlDark { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen ControlDarkDark { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen ControlLight { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen ControlLightLight { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen ControlText { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Desktop { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen GradientActiveCaption { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen GradientInactiveCaption { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen GrayText { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Highlight { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen HighlightText { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen HotTrack { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen InactiveBorder { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen InactiveCaption { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen InactiveCaptionText { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Info { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen InfoText { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Menu { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen MenuBar { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen MenuHighlight { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen MenuText { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen ScrollBar { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen Window { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen WindowFrame { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen WindowText { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Pen FromSystemColor(System.Drawing.Color c) { throw new PlatformNotSupportedException(); }
    }
    public sealed partial class TextureBrush : System.Drawing.Brush
    {
        public TextureBrush(System.Drawing.Image bitmap) => throw new PlatformNotSupportedException();
        public TextureBrush(System.Drawing.Image image, System.Drawing.Drawing2D.WrapMode wrapMode) => throw new PlatformNotSupportedException();
        public TextureBrush(System.Drawing.Image image, System.Drawing.Drawing2D.WrapMode wrapMode, System.Drawing.Rectangle dstRect) => throw new PlatformNotSupportedException();
        public TextureBrush(System.Drawing.Image image, System.Drawing.Drawing2D.WrapMode wrapMode, System.Drawing.RectangleF dstRect) => throw new PlatformNotSupportedException();
        public TextureBrush(System.Drawing.Image image, System.Drawing.Rectangle dstRect) => throw new PlatformNotSupportedException();
        public TextureBrush(System.Drawing.Image image, System.Drawing.Rectangle dstRect, System.Drawing.Imaging.ImageAttributes imageAttr) => throw new PlatformNotSupportedException();
        public TextureBrush(System.Drawing.Image image, System.Drawing.RectangleF dstRect) => throw new PlatformNotSupportedException();
        public TextureBrush(System.Drawing.Image image, System.Drawing.RectangleF dstRect, System.Drawing.Imaging.ImageAttributes imageAttr) => throw new PlatformNotSupportedException();
        public System.Drawing.Image Image { get { throw new PlatformNotSupportedException(); } }
        public System.Drawing.Drawing2D.Matrix Transform { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Drawing2D.WrapMode WrapMode { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public override object Clone() { throw new PlatformNotSupportedException(); }
        public void MultiplyTransform(System.Drawing.Drawing2D.Matrix matrix) => throw new PlatformNotSupportedException();
        public void MultiplyTransform(System.Drawing.Drawing2D.Matrix matrix, System.Drawing.Drawing2D.MatrixOrder order) => throw new PlatformNotSupportedException();
        public void ResetTransform() => throw new PlatformNotSupportedException();
        public void RotateTransform(float angle) => throw new PlatformNotSupportedException();
        public void RotateTransform(float angle, System.Drawing.Drawing2D.MatrixOrder order) => throw new PlatformNotSupportedException();
        public void ScaleTransform(float sx, float sy) => throw new PlatformNotSupportedException();
        public void ScaleTransform(float sx, float sy, System.Drawing.Drawing2D.MatrixOrder order) => throw new PlatformNotSupportedException();
        public void TranslateTransform(float dx, float dy) => throw new PlatformNotSupportedException();
        public void TranslateTransform(float dx, float dy, System.Drawing.Drawing2D.MatrixOrder order) => throw new PlatformNotSupportedException();
    }
    [System.AttributeUsageAttribute((System.AttributeTargets)(4))]
    public partial class ToolboxBitmapAttribute : System.Attribute
    {
        public static readonly System.Drawing.ToolboxBitmapAttribute Default;
        public ToolboxBitmapAttribute(string imageFile) => throw new PlatformNotSupportedException();
        public ToolboxBitmapAttribute(System.Type t) => throw new PlatformNotSupportedException();
        public ToolboxBitmapAttribute(System.Type t, string name) => throw new PlatformNotSupportedException();
        public override bool Equals(object value) { throw new PlatformNotSupportedException(); }
        public override int GetHashCode() { throw new PlatformNotSupportedException(); }
        public System.Drawing.Image GetImage(object component) { throw new PlatformNotSupportedException(); }
        public System.Drawing.Image GetImage(object component, bool large) { throw new PlatformNotSupportedException(); }
        public System.Drawing.Image GetImage(System.Type type) { throw new PlatformNotSupportedException(); }
        public System.Drawing.Image GetImage(System.Type type, bool large) { throw new PlatformNotSupportedException(); }
        public System.Drawing.Image GetImage(System.Type type, string imgName, bool large) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Image GetImageFromResource(System.Type t, string imageName, bool large) { throw new PlatformNotSupportedException(); }
    }
}
namespace System.Drawing.Design
{
    public sealed partial class CategoryNameCollection : System.Collections.ReadOnlyCollectionBase
    {
        public CategoryNameCollection(System.Drawing.Design.CategoryNameCollection value) => throw new PlatformNotSupportedException();
        public CategoryNameCollection(string[] value) => throw new PlatformNotSupportedException();
        public string this[int index] { get { throw new PlatformNotSupportedException(); } }
        public bool Contains(string value) { throw new PlatformNotSupportedException(); }
        public void CopyTo(string[] array, int index) => throw new PlatformNotSupportedException();
        public int IndexOf(string value) { throw new PlatformNotSupportedException(); }
    }
}
namespace System.Drawing.Drawing2D
{
    public sealed partial class AdjustableArrowCap : System.Drawing.Drawing2D.CustomLineCap
    {
        public AdjustableArrowCap(float width, float height) : base (default(System.Drawing.Drawing2D.GraphicsPath), default(System.Drawing.Drawing2D.GraphicsPath)) => throw new PlatformNotSupportedException();
        public AdjustableArrowCap(float width, float height, bool isFilled) : base (default(System.Drawing.Drawing2D.GraphicsPath), default(System.Drawing.Drawing2D.GraphicsPath)) => throw new PlatformNotSupportedException();
        public bool Filled { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float Height { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float MiddleInset { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float Width { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
    }
    public sealed partial class Blend
    {
        public Blend() => throw new PlatformNotSupportedException();
        public Blend(int count) => throw new PlatformNotSupportedException();
        public float[] Factors { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float[] Positions { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
    }
    public sealed partial class ColorBlend
    {
        public ColorBlend() => throw new PlatformNotSupportedException();
        public ColorBlend(int count) => throw new PlatformNotSupportedException();
        public System.Drawing.Color[] Colors { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float[] Positions { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
    }
    public enum CombineMode
    {
        Complement = 5,
        Exclude = 4,
        Intersect = 1,
        Replace = 0,
        Union = 2,
        Xor = 3,
    }
    public enum CompositingMode
    {
        SourceCopy = 1,
        SourceOver = 0,
    }
    public enum CompositingQuality
    {
        AssumeLinear = 4,
        Default = 0,
        GammaCorrected = 3,
        HighQuality = 2,
        HighSpeed = 1,
        Invalid = -1,
    }
    public enum CoordinateSpace
    {
        Device = 2,
        Page = 1,
        World = 0,
    }
    public partial class CustomLineCap : System.MarshalByRefObject, System.ICloneable, System.IDisposable
    {
        public CustomLineCap(System.Drawing.Drawing2D.GraphicsPath fillPath, System.Drawing.Drawing2D.GraphicsPath strokePath) => throw new PlatformNotSupportedException();
        public CustomLineCap(System.Drawing.Drawing2D.GraphicsPath fillPath, System.Drawing.Drawing2D.GraphicsPath strokePath, System.Drawing.Drawing2D.LineCap baseCap) => throw new PlatformNotSupportedException();
        public CustomLineCap(System.Drawing.Drawing2D.GraphicsPath fillPath, System.Drawing.Drawing2D.GraphicsPath strokePath, System.Drawing.Drawing2D.LineCap baseCap, float baseInset) => throw new PlatformNotSupportedException();
        public System.Drawing.Drawing2D.LineCap BaseCap { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float BaseInset { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Drawing2D.LineJoin StrokeJoin { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float WidthScale { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public object Clone() { throw new PlatformNotSupportedException(); }
        public void Dispose() => throw new PlatformNotSupportedException();
        protected virtual void Dispose(bool disposing) => throw new PlatformNotSupportedException();
        ~CustomLineCap() => throw new PlatformNotSupportedException();
        public void GetStrokeCaps(out System.Drawing.Drawing2D.LineCap startCap, out System.Drawing.Drawing2D.LineCap endCap) { throw new PlatformNotSupportedException(); }
        public void SetStrokeCaps(System.Drawing.Drawing2D.LineCap startCap, System.Drawing.Drawing2D.LineCap endCap) => throw new PlatformNotSupportedException();
    }
    public enum DashCap
    {
        Flat = 0,
        Round = 2,
        Triangle = 3,
    }
    public enum DashStyle
    {
        Custom = 5,
        Dash = 1,
        DashDot = 3,
        DashDotDot = 4,
        Dot = 2,
        Solid = 0,
    }
    public enum FillMode
    {
        Alternate = 0,
        Winding = 1,
    }
    public enum FlushIntention
    {
        Flush = 0,
        Sync = 1,
    }
    public sealed partial class GraphicsContainer : System.MarshalByRefObject
    {
        internal GraphicsContainer() => throw new PlatformNotSupportedException();
    }
    public sealed partial class GraphicsPath : System.MarshalByRefObject, System.ICloneable, System.IDisposable
    {
        public GraphicsPath() => throw new PlatformNotSupportedException();
        public GraphicsPath(System.Drawing.Drawing2D.FillMode fillMode) => throw new PlatformNotSupportedException();
        public GraphicsPath(System.Drawing.PointF[] pts, byte[] types) => throw new PlatformNotSupportedException();
        public GraphicsPath(System.Drawing.PointF[] pts, byte[] types, System.Drawing.Drawing2D.FillMode fillMode) => throw new PlatformNotSupportedException();
        public GraphicsPath(System.Drawing.Point[] pts, byte[] types) => throw new PlatformNotSupportedException();
        public GraphicsPath(System.Drawing.Point[] pts, byte[] types, System.Drawing.Drawing2D.FillMode fillMode) => throw new PlatformNotSupportedException();
        public System.Drawing.Drawing2D.FillMode FillMode { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Drawing2D.PathData PathData { get { throw new PlatformNotSupportedException(); } }
        public System.Drawing.PointF[] PathPoints { get { throw new PlatformNotSupportedException(); } }
        public byte[] PathTypes { get { throw new PlatformNotSupportedException(); } }
        public int PointCount { get { throw new PlatformNotSupportedException(); } }
        public void AddArc(System.Drawing.Rectangle rect, float startAngle, float sweepAngle) => throw new PlatformNotSupportedException();
        public void AddArc(System.Drawing.RectangleF rect, float startAngle, float sweepAngle) => throw new PlatformNotSupportedException();
        public void AddArc(int x, int y, int width, int height, float startAngle, float sweepAngle) => throw new PlatformNotSupportedException();
        public void AddArc(float x, float y, float width, float height, float startAngle, float sweepAngle) => throw new PlatformNotSupportedException();
        public void AddBezier(System.Drawing.Point pt1, System.Drawing.Point pt2, System.Drawing.Point pt3, System.Drawing.Point pt4) => throw new PlatformNotSupportedException();
        public void AddBezier(System.Drawing.PointF pt1, System.Drawing.PointF pt2, System.Drawing.PointF pt3, System.Drawing.PointF pt4) => throw new PlatformNotSupportedException();
        public void AddBezier(int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4) => throw new PlatformNotSupportedException();
        public void AddBezier(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4) => throw new PlatformNotSupportedException();
        public void AddBeziers(System.Drawing.PointF[] points) => throw new PlatformNotSupportedException();
        public void AddBeziers(params System.Drawing.Point[] points) => throw new PlatformNotSupportedException();
        public void AddClosedCurve(System.Drawing.PointF[] points) => throw new PlatformNotSupportedException();
        public void AddClosedCurve(System.Drawing.PointF[] points, float tension) => throw new PlatformNotSupportedException();
        public void AddClosedCurve(System.Drawing.Point[] points) => throw new PlatformNotSupportedException();
        public void AddClosedCurve(System.Drawing.Point[] points, float tension) => throw new PlatformNotSupportedException();
        public void AddCurve(System.Drawing.PointF[] points) => throw new PlatformNotSupportedException();
        public void AddCurve(System.Drawing.PointF[] points, int offset, int numberOfSegments, float tension) => throw new PlatformNotSupportedException();
        public void AddCurve(System.Drawing.PointF[] points, float tension) => throw new PlatformNotSupportedException();
        public void AddCurve(System.Drawing.Point[] points) => throw new PlatformNotSupportedException();
        public void AddCurve(System.Drawing.Point[] points, int offset, int numberOfSegments, float tension) => throw new PlatformNotSupportedException();
        public void AddCurve(System.Drawing.Point[] points, float tension) => throw new PlatformNotSupportedException();
        public void AddEllipse(System.Drawing.Rectangle rect) => throw new PlatformNotSupportedException();
        public void AddEllipse(System.Drawing.RectangleF rect) => throw new PlatformNotSupportedException();
        public void AddEllipse(int x, int y, int width, int height) => throw new PlatformNotSupportedException();
        public void AddEllipse(float x, float y, float width, float height) => throw new PlatformNotSupportedException();
        public void AddLine(System.Drawing.Point pt1, System.Drawing.Point pt2) => throw new PlatformNotSupportedException();
        public void AddLine(System.Drawing.PointF pt1, System.Drawing.PointF pt2) => throw new PlatformNotSupportedException();
        public void AddLine(int x1, int y1, int x2, int y2) => throw new PlatformNotSupportedException();
        public void AddLine(float x1, float y1, float x2, float y2) => throw new PlatformNotSupportedException();
        public void AddLines(System.Drawing.PointF[] points) => throw new PlatformNotSupportedException();
        public void AddLines(System.Drawing.Point[] points) => throw new PlatformNotSupportedException();
        public void AddPath(System.Drawing.Drawing2D.GraphicsPath addingPath, bool connect) => throw new PlatformNotSupportedException();
        public void AddPie(System.Drawing.Rectangle rect, float startAngle, float sweepAngle) => throw new PlatformNotSupportedException();
        public void AddPie(int x, int y, int width, int height, float startAngle, float sweepAngle) => throw new PlatformNotSupportedException();
        public void AddPie(float x, float y, float width, float height, float startAngle, float sweepAngle) => throw new PlatformNotSupportedException();
        public void AddPolygon(System.Drawing.PointF[] points) => throw new PlatformNotSupportedException();
        public void AddPolygon(System.Drawing.Point[] points) => throw new PlatformNotSupportedException();
        public void AddRectangle(System.Drawing.Rectangle rect) => throw new PlatformNotSupportedException();
        public void AddRectangle(System.Drawing.RectangleF rect) => throw new PlatformNotSupportedException();
        public void AddRectangles(System.Drawing.RectangleF[] rects) => throw new PlatformNotSupportedException();
        public void AddRectangles(System.Drawing.Rectangle[] rects) => throw new PlatformNotSupportedException();
        public void AddString(string s, System.Drawing.FontFamily family, int style, float emSize, System.Drawing.Point origin, System.Drawing.StringFormat format) => throw new PlatformNotSupportedException();
        public void AddString(string s, System.Drawing.FontFamily family, int style, float emSize, System.Drawing.PointF origin, System.Drawing.StringFormat format) => throw new PlatformNotSupportedException();
        public void AddString(string s, System.Drawing.FontFamily family, int style, float emSize, System.Drawing.Rectangle layoutRect, System.Drawing.StringFormat format) => throw new PlatformNotSupportedException();
        public void AddString(string s, System.Drawing.FontFamily family, int style, float emSize, System.Drawing.RectangleF layoutRect, System.Drawing.StringFormat format) => throw new PlatformNotSupportedException();
        public void ClearMarkers() => throw new PlatformNotSupportedException();
        public object Clone() { throw new PlatformNotSupportedException(); }
        public void CloseAllFigures() => throw new PlatformNotSupportedException();
        public void CloseFigure() => throw new PlatformNotSupportedException();
        public void Dispose() => throw new PlatformNotSupportedException();
        ~GraphicsPath() => throw new PlatformNotSupportedException();
        public void Flatten() => throw new PlatformNotSupportedException();
        public void Flatten(System.Drawing.Drawing2D.Matrix matrix) => throw new PlatformNotSupportedException();
        public void Flatten(System.Drawing.Drawing2D.Matrix matrix, float flatness) => throw new PlatformNotSupportedException();
        public System.Drawing.RectangleF GetBounds() { throw new PlatformNotSupportedException(); }
        public System.Drawing.RectangleF GetBounds(System.Drawing.Drawing2D.Matrix matrix) { throw new PlatformNotSupportedException(); }
        public System.Drawing.RectangleF GetBounds(System.Drawing.Drawing2D.Matrix matrix, System.Drawing.Pen pen) { throw new PlatformNotSupportedException(); }
        public System.Drawing.PointF GetLastPoint() { throw new PlatformNotSupportedException(); }
        public bool IsOutlineVisible(System.Drawing.Point point, System.Drawing.Pen pen) { throw new PlatformNotSupportedException(); }
        public bool IsOutlineVisible(System.Drawing.Point pt, System.Drawing.Pen pen, System.Drawing.Graphics graphics) { throw new PlatformNotSupportedException(); }
        public bool IsOutlineVisible(System.Drawing.PointF point, System.Drawing.Pen pen) { throw new PlatformNotSupportedException(); }
        public bool IsOutlineVisible(System.Drawing.PointF pt, System.Drawing.Pen pen, System.Drawing.Graphics graphics) { throw new PlatformNotSupportedException(); }
        public bool IsOutlineVisible(int x, int y, System.Drawing.Pen pen) { throw new PlatformNotSupportedException(); }
        public bool IsOutlineVisible(int x, int y, System.Drawing.Pen pen, System.Drawing.Graphics graphics) { throw new PlatformNotSupportedException(); }
        public bool IsOutlineVisible(float x, float y, System.Drawing.Pen pen) { throw new PlatformNotSupportedException(); }
        public bool IsOutlineVisible(float x, float y, System.Drawing.Pen pen, System.Drawing.Graphics graphics) { throw new PlatformNotSupportedException(); }
        public bool IsVisible(System.Drawing.Point point) { throw new PlatformNotSupportedException(); }
        public bool IsVisible(System.Drawing.Point pt, System.Drawing.Graphics graphics) { throw new PlatformNotSupportedException(); }
        public bool IsVisible(System.Drawing.PointF point) { throw new PlatformNotSupportedException(); }
        public bool IsVisible(System.Drawing.PointF pt, System.Drawing.Graphics graphics) { throw new PlatformNotSupportedException(); }
        public bool IsVisible(int x, int y) { throw new PlatformNotSupportedException(); }
        public bool IsVisible(int x, int y, System.Drawing.Graphics graphics) { throw new PlatformNotSupportedException(); }
        public bool IsVisible(float x, float y) { throw new PlatformNotSupportedException(); }
        public bool IsVisible(float x, float y, System.Drawing.Graphics graphics) { throw new PlatformNotSupportedException(); }
        public void Reset() => throw new PlatformNotSupportedException();
        public void Reverse() => throw new PlatformNotSupportedException();
        public void SetMarkers() => throw new PlatformNotSupportedException();
        public void StartFigure() => throw new PlatformNotSupportedException();
        public void Transform(System.Drawing.Drawing2D.Matrix matrix) => throw new PlatformNotSupportedException();
        public void Warp(System.Drawing.PointF[] destPoints, System.Drawing.RectangleF srcRect) => throw new PlatformNotSupportedException();
        public void Warp(System.Drawing.PointF[] destPoints, System.Drawing.RectangleF srcRect, System.Drawing.Drawing2D.Matrix matrix) => throw new PlatformNotSupportedException();
        public void Warp(System.Drawing.PointF[] destPoints, System.Drawing.RectangleF srcRect, System.Drawing.Drawing2D.Matrix matrix, System.Drawing.Drawing2D.WarpMode warpMode) => throw new PlatformNotSupportedException();
        public void Warp(System.Drawing.PointF[] destPoints, System.Drawing.RectangleF srcRect, System.Drawing.Drawing2D.Matrix matrix, System.Drawing.Drawing2D.WarpMode warpMode, float flatness) => throw new PlatformNotSupportedException();
        public void Widen(System.Drawing.Pen pen) => throw new PlatformNotSupportedException();
        public void Widen(System.Drawing.Pen pen, System.Drawing.Drawing2D.Matrix matrix) => throw new PlatformNotSupportedException();
        public void Widen(System.Drawing.Pen pen, System.Drawing.Drawing2D.Matrix matrix, float flatness) => throw new PlatformNotSupportedException();
    }
    public sealed partial class GraphicsPathIterator : System.MarshalByRefObject, System.IDisposable
    {
        public GraphicsPathIterator(System.Drawing.Drawing2D.GraphicsPath path) => throw new PlatformNotSupportedException();
        public int Count { get { throw new PlatformNotSupportedException(); } }
        public int SubpathCount { get { throw new PlatformNotSupportedException(); } }
        public int CopyData(ref System.Drawing.PointF[] points, ref byte[] types, int startIndex, int endIndex) { throw new PlatformNotSupportedException(); }
        public void Dispose() => throw new PlatformNotSupportedException();
        public int Enumerate(ref System.Drawing.PointF[] points, ref byte[] types) { throw new PlatformNotSupportedException(); }
        ~GraphicsPathIterator() => throw new PlatformNotSupportedException();
        public bool HasCurve() { throw new PlatformNotSupportedException(); }
        public int NextMarker(System.Drawing.Drawing2D.GraphicsPath path) { throw new PlatformNotSupportedException(); }
        public int NextMarker(out int startIndex, out int endIndex) { throw new PlatformNotSupportedException(); }
        public int NextPathType(out byte pathType, out int startIndex, out int endIndex) { throw new PlatformNotSupportedException(); }
        public int NextSubpath(System.Drawing.Drawing2D.GraphicsPath path, out bool isClosed) { throw new PlatformNotSupportedException(); }
        public int NextSubpath(out int startIndex, out int endIndex, out bool isClosed) { throw new PlatformNotSupportedException(); }
        public void Rewind() => throw new PlatformNotSupportedException();
    }
    public sealed partial class GraphicsState : System.MarshalByRefObject
    {
        internal GraphicsState() => throw new PlatformNotSupportedException();
    }
    public sealed partial class HatchBrush : System.Drawing.Brush
    {
        public HatchBrush(System.Drawing.Drawing2D.HatchStyle hatchstyle, System.Drawing.Color foreColor) => throw new PlatformNotSupportedException();
        public HatchBrush(System.Drawing.Drawing2D.HatchStyle hatchstyle, System.Drawing.Color foreColor, System.Drawing.Color backColor) => throw new PlatformNotSupportedException();
        public System.Drawing.Color BackgroundColor { get { throw new PlatformNotSupportedException(); } }
        public System.Drawing.Color ForegroundColor { get { throw new PlatformNotSupportedException(); } }
        public System.Drawing.Drawing2D.HatchStyle HatchStyle { get { throw new PlatformNotSupportedException(); } }
        public override object Clone() { throw new PlatformNotSupportedException(); }
    }
    public enum HatchStyle
    {
        BackwardDiagonal = 3,
        Cross = 4,
        DarkDownwardDiagonal = 20,
        DarkHorizontal = 29,
        DarkUpwardDiagonal = 21,
        DarkVertical = 28,
        DashedDownwardDiagonal = 30,
        DashedHorizontal = 32,
        DashedUpwardDiagonal = 31,
        DashedVertical = 33,
        DiagonalBrick = 38,
        DiagonalCross = 5,
        Divot = 42,
        DottedDiamond = 44,
        DottedGrid = 43,
        ForwardDiagonal = 2,
        Horizontal = 0,
        HorizontalBrick = 39,
        LargeCheckerBoard = 50,
        LargeConfetti = 35,
        LargeGrid = 4,
        LightDownwardDiagonal = 18,
        LightHorizontal = 25,
        LightUpwardDiagonal = 19,
        LightVertical = 24,
        Max = 4,
        Min = 0,
        NarrowHorizontal = 27,
        NarrowVertical = 26,
        OutlinedDiamond = 51,
        Percent05 = 6,
        Percent10 = 7,
        Percent20 = 8,
        Percent25 = 9,
        Percent30 = 10,
        Percent40 = 11,
        Percent50 = 12,
        Percent60 = 13,
        Percent70 = 14,
        Percent75 = 15,
        Percent80 = 16,
        Percent90 = 17,
        Plaid = 41,
        Shingle = 45,
        SmallCheckerBoard = 49,
        SmallConfetti = 34,
        SmallGrid = 48,
        SolidDiamond = 52,
        Sphere = 47,
        Trellis = 46,
        Vertical = 1,
        Wave = 37,
        Weave = 40,
        WideDownwardDiagonal = 22,
        WideUpwardDiagonal = 23,
        ZigZag = 36,
    }
    public enum InterpolationMode
    {
        Bicubic = 4,
        Bilinear = 3,
        Default = 0,
        High = 2,
        HighQualityBicubic = 7,
        HighQualityBilinear = 6,
        Invalid = -1,
        Low = 1,
        NearestNeighbor = 5,
    }
    public sealed partial class LinearGradientBrush : System.Drawing.Brush
    {
        public LinearGradientBrush(System.Drawing.Point point1, System.Drawing.Point point2, System.Drawing.Color color1, System.Drawing.Color color2) => throw new PlatformNotSupportedException();
        public LinearGradientBrush(System.Drawing.PointF point1, System.Drawing.PointF point2, System.Drawing.Color color1, System.Drawing.Color color2) => throw new PlatformNotSupportedException();
        public LinearGradientBrush(System.Drawing.Rectangle rect, System.Drawing.Color color1, System.Drawing.Color color2, System.Drawing.Drawing2D.LinearGradientMode linearGradientMode) => throw new PlatformNotSupportedException();
        public LinearGradientBrush(System.Drawing.Rectangle rect, System.Drawing.Color color1, System.Drawing.Color color2, float angle) => throw new PlatformNotSupportedException();
        public LinearGradientBrush(System.Drawing.Rectangle rect, System.Drawing.Color color1, System.Drawing.Color color2, float angle, bool isAngleScaleable) => throw new PlatformNotSupportedException();
        public LinearGradientBrush(System.Drawing.RectangleF rect, System.Drawing.Color color1, System.Drawing.Color color2, System.Drawing.Drawing2D.LinearGradientMode linearGradientMode) => throw new PlatformNotSupportedException();
        public LinearGradientBrush(System.Drawing.RectangleF rect, System.Drawing.Color color1, System.Drawing.Color color2, float angle) => throw new PlatformNotSupportedException();
        public LinearGradientBrush(System.Drawing.RectangleF rect, System.Drawing.Color color1, System.Drawing.Color color2, float angle, bool isAngleScaleable) => throw new PlatformNotSupportedException();
        public System.Drawing.Drawing2D.Blend Blend { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public bool GammaCorrection { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Drawing2D.ColorBlend InterpolationColors { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Color[] LinearColors { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.RectangleF Rectangle { get { throw new PlatformNotSupportedException(); } }
        public System.Drawing.Drawing2D.Matrix Transform { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Drawing2D.WrapMode WrapMode { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public override object Clone() { throw new PlatformNotSupportedException(); }
        public void MultiplyTransform(System.Drawing.Drawing2D.Matrix matrix) => throw new PlatformNotSupportedException();
        public void MultiplyTransform(System.Drawing.Drawing2D.Matrix matrix, System.Drawing.Drawing2D.MatrixOrder order) => throw new PlatformNotSupportedException();
        public void ResetTransform() => throw new PlatformNotSupportedException();
        public void RotateTransform(float angle) => throw new PlatformNotSupportedException();
        public void RotateTransform(float angle, System.Drawing.Drawing2D.MatrixOrder order) => throw new PlatformNotSupportedException();
        public void ScaleTransform(float sx, float sy) => throw new PlatformNotSupportedException();
        public void ScaleTransform(float sx, float sy, System.Drawing.Drawing2D.MatrixOrder order) => throw new PlatformNotSupportedException();
        public void SetBlendTriangularShape(float focus) => throw new PlatformNotSupportedException();
        public void SetBlendTriangularShape(float focus, float scale) => throw new PlatformNotSupportedException();
        public void SetSigmaBellShape(float focus) => throw new PlatformNotSupportedException();
        public void SetSigmaBellShape(float focus, float scale) => throw new PlatformNotSupportedException();
        public void TranslateTransform(float dx, float dy) => throw new PlatformNotSupportedException();
        public void TranslateTransform(float dx, float dy, System.Drawing.Drawing2D.MatrixOrder order) => throw new PlatformNotSupportedException();
    }
    public enum LinearGradientMode
    {
        BackwardDiagonal = 3,
        ForwardDiagonal = 2,
        Horizontal = 0,
        Vertical = 1,
    }
    public enum LineCap
    {
        AnchorMask = 240,
        ArrowAnchor = 20,
        Custom = 255,
        DiamondAnchor = 19,
        Flat = 0,
        NoAnchor = 16,
        Round = 2,
        RoundAnchor = 18,
        Square = 1,
        SquareAnchor = 17,
        Triangle = 3,
    }
    public enum LineJoin
    {
        Bevel = 1,
        Miter = 0,
        MiterClipped = 3,
        Round = 2,
    }
    public sealed partial class Matrix : System.MarshalByRefObject, System.IDisposable
    {
        public Matrix() => throw new PlatformNotSupportedException();
        public Matrix(System.Drawing.Rectangle rect, System.Drawing.Point[] plgpts) => throw new PlatformNotSupportedException();
        public Matrix(System.Drawing.RectangleF rect, System.Drawing.PointF[] plgpts) => throw new PlatformNotSupportedException();
        public Matrix(float m11, float m12, float m21, float m22, float dx, float dy) => throw new PlatformNotSupportedException();
        public float[] Elements { get { throw new PlatformNotSupportedException(); } }
        public bool IsIdentity { get { throw new PlatformNotSupportedException(); } }
        public bool IsInvertible { get { throw new PlatformNotSupportedException(); } }
        public float OffsetX { get { throw new PlatformNotSupportedException(); } }
        public float OffsetY { get { throw new PlatformNotSupportedException(); } }
        public System.Drawing.Drawing2D.Matrix Clone() { throw new PlatformNotSupportedException(); }
        public void Dispose() => throw new PlatformNotSupportedException();
        public override bool Equals(object obj) { throw new PlatformNotSupportedException(); }
        ~Matrix() => throw new PlatformNotSupportedException();
        public override int GetHashCode() { throw new PlatformNotSupportedException(); }
        public void Invert() => throw new PlatformNotSupportedException();
        public void Multiply(System.Drawing.Drawing2D.Matrix matrix) => throw new PlatformNotSupportedException();
        public void Multiply(System.Drawing.Drawing2D.Matrix matrix, System.Drawing.Drawing2D.MatrixOrder order) => throw new PlatformNotSupportedException();
        public void Reset() => throw new PlatformNotSupportedException();
        public void Rotate(float angle) => throw new PlatformNotSupportedException();
        public void Rotate(float angle, System.Drawing.Drawing2D.MatrixOrder order) => throw new PlatformNotSupportedException();
        public void RotateAt(float angle, System.Drawing.PointF point) => throw new PlatformNotSupportedException();
        public void RotateAt(float angle, System.Drawing.PointF point, System.Drawing.Drawing2D.MatrixOrder order) => throw new PlatformNotSupportedException();
        public void Scale(float scaleX, float scaleY) => throw new PlatformNotSupportedException();
        public void Scale(float scaleX, float scaleY, System.Drawing.Drawing2D.MatrixOrder order) => throw new PlatformNotSupportedException();
        public void Shear(float shearX, float shearY) => throw new PlatformNotSupportedException();
        public void Shear(float shearX, float shearY, System.Drawing.Drawing2D.MatrixOrder order) => throw new PlatformNotSupportedException();
        public void TransformPoints(System.Drawing.PointF[] pts) => throw new PlatformNotSupportedException();
        public void TransformPoints(System.Drawing.Point[] pts) => throw new PlatformNotSupportedException();
        public void TransformVectors(System.Drawing.PointF[] pts) => throw new PlatformNotSupportedException();
        public void TransformVectors(System.Drawing.Point[] pts) => throw new PlatformNotSupportedException();
        public void Translate(float offsetX, float offsetY) => throw new PlatformNotSupportedException();
        public void Translate(float offsetX, float offsetY, System.Drawing.Drawing2D.MatrixOrder order) => throw new PlatformNotSupportedException();
        public void VectorTransformPoints(System.Drawing.Point[] pts) => throw new PlatformNotSupportedException();
    }
    public enum MatrixOrder
    {
        Append = 1,
        Prepend = 0,
    }
    public sealed partial class PathData
    {
        public PathData() => throw new PlatformNotSupportedException();
        public System.Drawing.PointF[] Points { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public byte[] Types { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
    }
    public sealed partial class PathGradientBrush : System.Drawing.Brush
    {
        public PathGradientBrush(System.Drawing.Drawing2D.GraphicsPath path) => throw new PlatformNotSupportedException();
        public PathGradientBrush(System.Drawing.PointF[] points) => throw new PlatformNotSupportedException();
        public PathGradientBrush(System.Drawing.PointF[] points, System.Drawing.Drawing2D.WrapMode wrapMode) => throw new PlatformNotSupportedException();
        public PathGradientBrush(System.Drawing.Point[] points) => throw new PlatformNotSupportedException();
        public PathGradientBrush(System.Drawing.Point[] points, System.Drawing.Drawing2D.WrapMode wrapMode) => throw new PlatformNotSupportedException();
        public System.Drawing.Drawing2D.Blend Blend { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Color CenterColor { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.PointF CenterPoint { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.PointF FocusScales { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Drawing2D.ColorBlend InterpolationColors { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.RectangleF Rectangle { get { throw new PlatformNotSupportedException(); } }
        public System.Drawing.Color[] SurroundColors { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Drawing2D.Matrix Transform { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Drawing2D.WrapMode WrapMode { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public override object Clone() { throw new PlatformNotSupportedException(); }
        public void MultiplyTransform(System.Drawing.Drawing2D.Matrix matrix) => throw new PlatformNotSupportedException();
        public void MultiplyTransform(System.Drawing.Drawing2D.Matrix matrix, System.Drawing.Drawing2D.MatrixOrder order) => throw new PlatformNotSupportedException();
        public void ResetTransform() => throw new PlatformNotSupportedException();
        public void RotateTransform(float angle) => throw new PlatformNotSupportedException();
        public void RotateTransform(float angle, System.Drawing.Drawing2D.MatrixOrder order) => throw new PlatformNotSupportedException();
        public void ScaleTransform(float sx, float sy) => throw new PlatformNotSupportedException();
        public void ScaleTransform(float sx, float sy, System.Drawing.Drawing2D.MatrixOrder order) => throw new PlatformNotSupportedException();
        public void SetBlendTriangularShape(float focus) => throw new PlatformNotSupportedException();
        public void SetBlendTriangularShape(float focus, float scale) => throw new PlatformNotSupportedException();
        public void SetSigmaBellShape(float focus) => throw new PlatformNotSupportedException();
        public void SetSigmaBellShape(float focus, float scale) => throw new PlatformNotSupportedException();
        public void TranslateTransform(float dx, float dy) => throw new PlatformNotSupportedException();
        public void TranslateTransform(float dx, float dy, System.Drawing.Drawing2D.MatrixOrder order) => throw new PlatformNotSupportedException();
    }
    public enum PathPointType
    {
        Bezier = 3,
        Bezier3 = 3,
        CloseSubpath = 128,
        DashMode = 16,
        Line = 1,
        PathMarker = 32,
        PathTypeMask = 7,
        Start = 0,
    }
    public enum PenAlignment
    {
        Center = 0,
        Inset = 1,
        Left = 3,
        Outset = 2,
        Right = 4,
    }
    public enum PenType
    {
        HatchFill = 1,
        LinearGradient = 4,
        PathGradient = 3,
        SolidColor = 0,
        TextureFill = 2,
    }
    public enum PixelOffsetMode
    {
        Default = 0,
        Half = 4,
        HighQuality = 2,
        HighSpeed = 1,
        Invalid = -1,
        None = 3,
    }
    public enum QualityMode
    {
        Default = 0,
        High = 2,
        Invalid = -1,
        Low = 1,
    }
    public sealed partial class RegionData
    {
        internal RegionData() => throw new PlatformNotSupportedException();
        public byte[] Data { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
    }
    public enum SmoothingMode
    {
        AntiAlias = 4,
        Default = 0,
        HighQuality = 2,
        HighSpeed = 1,
        Invalid = -1,
        None = 3,
    }
    public enum WarpMode
    {
        Bilinear = 1,
        Perspective = 0,
    }
    public enum WrapMode
    {
        Clamp = 4,
        Tile = 0,
        TileFlipX = 1,
        TileFlipXY = 3,
        TileFlipY = 2,
    }
}
namespace System.Drawing.Imaging
{
    public sealed partial class BitmapData
    {
        public BitmapData() => throw new PlatformNotSupportedException();
        public int Height { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Imaging.PixelFormat PixelFormat { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public int Reserved { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.IntPtr Scan0 { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public int Stride { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public int Width { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
    }
    public enum ColorAdjustType
    {
        Any = 6,
        Bitmap = 1,
        Brush = 2,
        Count = 5,
        Default = 0,
        Pen = 3,
        Text = 4,
    }
    public enum ColorChannelFlag
    {
        ColorChannelC = 0,
        ColorChannelK = 3,
        ColorChannelLast = 4,
        ColorChannelM = 1,
        ColorChannelY = 2,
    }
    public sealed partial class ColorMap
    {
        public ColorMap() => throw new PlatformNotSupportedException();
        public System.Drawing.Color NewColor { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Color OldColor { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
    }
    public enum ColorMapType
    {
        Brush = 1,
        Default = 0,
    }
    public sealed partial class ColorMatrix
    {
        public ColorMatrix() => throw new PlatformNotSupportedException();
        [System.CLSCompliantAttribute(false)]
        public ColorMatrix(float[][] newColorMatrix) => throw new PlatformNotSupportedException();
        public float this[int row, int column] { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float Matrix00 { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float Matrix01 { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float Matrix02 { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float Matrix03 { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float Matrix04 { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float Matrix10 { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float Matrix11 { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float Matrix12 { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float Matrix13 { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float Matrix14 { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float Matrix20 { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float Matrix21 { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float Matrix22 { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float Matrix23 { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float Matrix24 { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float Matrix30 { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float Matrix31 { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float Matrix32 { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float Matrix33 { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float Matrix34 { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float Matrix40 { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float Matrix41 { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float Matrix42 { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float Matrix43 { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float Matrix44 { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
    }
    public enum ColorMatrixFlag
    {
        AltGrays = 2,
        Default = 0,
        SkipGrays = 1,
    }
    public enum ColorMode
    {
        Argb32Mode = 0,
        Argb64Mode = 1,
    }
    public sealed partial class ColorPalette
    {
        internal ColorPalette() => throw new PlatformNotSupportedException();
        public System.Drawing.Color[] Entries { get { throw new PlatformNotSupportedException(); } }
        public int Flags { get { throw new PlatformNotSupportedException(); } }
    }
    public enum EmfPlusRecordType
    {
        BeginContainer = 16423,
        BeginContainerNoParams = 16424,
        Clear = 16393,
        Comment = 16387,
        DrawArc = 16402,
        DrawBeziers = 16409,
        DrawClosedCurve = 16407,
        DrawCurve = 16408,
        DrawDriverString = 16438,
        DrawEllipse = 16399,
        DrawImage = 16410,
        DrawImagePoints = 16411,
        DrawLines = 16397,
        DrawPath = 16405,
        DrawPie = 16401,
        DrawRects = 16395,
        DrawString = 16412,
        EmfAbortPath = 68,
        EmfAlphaBlend = 114,
        EmfAngleArc = 41,
        EmfArcTo = 55,
        EmfBeginPath = 59,
        EmfBitBlt = 76,
        EmfChord = 46,
        EmfCloseFigure = 61,
        EmfColorCorrectPalette = 111,
        EmfColorMatchToTargetW = 121,
        EmfCreateBrushIndirect = 39,
        EmfCreateColorSpace = 99,
        EmfCreateColorSpaceW = 122,
        EmfCreateDibPatternBrushPt = 94,
        EmfCreateMonoBrush = 93,
        EmfCreatePalette = 49,
        EmfCreatePen = 38,
        EmfDeleteColorSpace = 101,
        EmfDeleteObject = 40,
        EmfDrawEscape = 105,
        EmfEllipse = 42,
        EmfEndPath = 60,
        EmfEof = 14,
        EmfExcludeClipRect = 29,
        EmfExtCreateFontIndirect = 82,
        EmfExtCreatePen = 95,
        EmfExtEscape = 106,
        EmfExtFloodFill = 53,
        EmfExtSelectClipRgn = 75,
        EmfExtTextOutA = 83,
        EmfExtTextOutW = 84,
        EmfFillPath = 62,
        EmfFillRgn = 71,
        EmfFlattenPath = 65,
        EmfForceUfiMapping = 109,
        EmfFrameRgn = 72,
        EmfGdiComment = 70,
        EmfGlsBoundedRecord = 103,
        EmfGlsRecord = 102,
        EmfGradientFill = 118,
        EmfHeader = 1,
        EmfIntersectClipRect = 30,
        EmfInvertRgn = 73,
        EmfLineTo = 54,
        EmfMaskBlt = 78,
        EmfMax = 122,
        EmfMin = 1,
        EmfModifyWorldTransform = 36,
        EmfMoveToEx = 27,
        EmfNamedEscpae = 110,
        EmfOffsetClipRgn = 26,
        EmfPaintRgn = 74,
        EmfPie = 47,
        EmfPixelFormat = 104,
        EmfPlgBlt = 79,
        EmfPlusRecordBase = 16384,
        EmfPolyBezier = 2,
        EmfPolyBezier16 = 85,
        EmfPolyBezierTo = 5,
        EmfPolyBezierTo16 = 88,
        EmfPolyDraw = 56,
        EmfPolyDraw16 = 92,
        EmfPolygon = 3,
        EmfPolygon16 = 86,
        EmfPolyline = 4,
        EmfPolyline16 = 87,
        EmfPolyLineTo = 6,
        EmfPolylineTo16 = 89,
        EmfPolyPolygon = 8,
        EmfPolyPolygon16 = 91,
        EmfPolyPolyline = 7,
        EmfPolyPolyline16 = 90,
        EmfPolyTextOutA = 96,
        EmfPolyTextOutW = 97,
        EmfRealizePalette = 52,
        EmfRectangle = 43,
        EmfReserved069 = 69,
        EmfReserved117 = 117,
        EmfResizePalette = 51,
        EmfRestoreDC = 34,
        EmfRoundArc = 45,
        EmfRoundRect = 44,
        EmfSaveDC = 33,
        EmfScaleViewportExtEx = 31,
        EmfScaleWindowExtEx = 32,
        EmfSelectClipPath = 67,
        EmfSelectObject = 37,
        EmfSelectPalette = 48,
        EmfSetArcDirection = 57,
        EmfSetBkColor = 25,
        EmfSetBkMode = 18,
        EmfSetBrushOrgEx = 13,
        EmfSetColorAdjustment = 23,
        EmfSetColorSpace = 100,
        EmfSetDIBitsToDevice = 80,
        EmfSetIcmMode = 98,
        EmfSetIcmProfileA = 112,
        EmfSetIcmProfileW = 113,
        EmfSetLayout = 115,
        EmfSetLinkedUfis = 119,
        EmfSetMapMode = 17,
        EmfSetMapperFlags = 16,
        EmfSetMetaRgn = 28,
        EmfSetMiterLimit = 58,
        EmfSetPaletteEntries = 50,
        EmfSetPixelV = 15,
        EmfSetPolyFillMode = 19,
        EmfSetROP2 = 20,
        EmfSetStretchBltMode = 21,
        EmfSetTextAlign = 22,
        EmfSetTextColor = 24,
        EmfSetTextJustification = 120,
        EmfSetViewportExtEx = 11,
        EmfSetViewportOrgEx = 12,
        EmfSetWindowExtEx = 9,
        EmfSetWindowOrgEx = 10,
        EmfSetWorldTransform = 35,
        EmfSmallTextOut = 108,
        EmfStartDoc = 107,
        EmfStretchBlt = 77,
        EmfStretchDIBits = 81,
        EmfStrokeAndFillPath = 63,
        EmfStrokePath = 64,
        EmfTransparentBlt = 116,
        EmfWidenPath = 66,
        EndContainer = 16425,
        EndOfFile = 16386,
        FillClosedCurve = 16406,
        FillEllipse = 16398,
        FillPath = 16404,
        FillPie = 16400,
        FillPolygon = 16396,
        FillRects = 16394,
        FillRegion = 16403,
        GetDC = 16388,
        Header = 16385,
        Invalid = 16384,
        Max = 16438,
        Min = 16385,
        MultiFormatEnd = 16391,
        MultiFormatSection = 16390,
        MultiFormatStart = 16389,
        MultiplyWorldTransform = 16428,
        Object = 16392,
        OffsetClip = 16437,
        ResetClip = 16433,
        ResetWorldTransform = 16427,
        Restore = 16422,
        RotateWorldTransform = 16431,
        Save = 16421,
        ScaleWorldTransform = 16430,
        SetAntiAliasMode = 16414,
        SetClipPath = 16435,
        SetClipRect = 16434,
        SetClipRegion = 16436,
        SetCompositingMode = 16419,
        SetCompositingQuality = 16420,
        SetInterpolationMode = 16417,
        SetPageTransform = 16432,
        SetPixelOffsetMode = 16418,
        SetRenderingOrigin = 16413,
        SetTextContrast = 16416,
        SetTextRenderingHint = 16415,
        SetWorldTransform = 16426,
        Total = 16439,
        TranslateWorldTransform = 16429,
        WmfAnimatePalette = 66614,
        WmfArc = 67607,
        WmfBitBlt = 67874,
        WmfChord = 67632,
        WmfCreateBrushIndirect = 66300,
        WmfCreateFontIndirect = 66299,
        WmfCreatePalette = 65783,
        WmfCreatePatternBrush = 66041,
        WmfCreatePenIndirect = 66298,
        WmfCreateRegion = 67327,
        WmfDeleteObject = 66032,
        WmfDibBitBlt = 67904,
        WmfDibCreatePatternBrush = 65858,
        WmfDibStretchBlt = 68417,
        WmfEllipse = 66584,
        WmfEscape = 67110,
        WmfExcludeClipRect = 66581,
        WmfExtFloodFill = 66888,
        WmfExtTextOut = 68146,
        WmfFillRegion = 66088,
        WmfFloodFill = 66585,
        WmfFrameRegion = 66601,
        WmfIntersectClipRect = 66582,
        WmfInvertRegion = 65834,
        WmfLineTo = 66067,
        WmfMoveTo = 66068,
        WmfOffsetCilpRgn = 66080,
        WmfOffsetViewportOrg = 66065,
        WmfOffsetWindowOrg = 66063,
        WmfPaintRegion = 65835,
        WmfPatBlt = 67101,
        WmfPie = 67610,
        WmfPolygon = 66340,
        WmfPolyline = 66341,
        WmfPolyPolygon = 66872,
        WmfRealizePalette = 65589,
        WmfRecordBase = 65536,
        WmfRectangle = 66587,
        WmfResizePalette = 65849,
        WmfRestoreDC = 65831,
        WmfRoundRect = 67100,
        WmfSaveDC = 65566,
        WmfScaleViewportExt = 66578,
        WmfScaleWindowExt = 66576,
        WmfSelectClipRegion = 65836,
        WmfSelectObject = 65837,
        WmfSelectPalette = 66100,
        WmfSetBkColor = 66049,
        WmfSetBkMode = 65794,
        WmfSetDibToDev = 68915,
        WmfSetLayout = 65865,
        WmfSetMapMode = 65795,
        WmfSetMapperFlags = 66097,
        WmfSetPalEntries = 65591,
        WmfSetPixel = 66591,
        WmfSetPolyFillMode = 65798,
        WmfSetRelAbs = 65797,
        WmfSetROP2 = 65796,
        WmfSetStretchBltMode = 65799,
        WmfSetTextAlign = 65838,
        WmfSetTextCharExtra = 65800,
        WmfSetTextColor = 66057,
        WmfSetTextJustification = 66058,
        WmfSetViewportExt = 66062,
        WmfSetViewportOrg = 66061,
        WmfSetWindowExt = 66060,
        WmfSetWindowOrg = 66059,
        WmfStretchBlt = 68387,
        WmfStretchDib = 69443,
        WmfTextOut = 66849,
    }
    public enum EmfType
    {
        EmfOnly = 3,
        EmfPlusDual = 5,
        EmfPlusOnly = 4,
    }
    public sealed partial class Encoder
    {
        public static readonly System.Drawing.Imaging.Encoder ChrominanceTable;
        public static readonly System.Drawing.Imaging.Encoder ColorDepth;
        public static readonly System.Drawing.Imaging.Encoder Compression;
        public static readonly System.Drawing.Imaging.Encoder LuminanceTable;
        public static readonly System.Drawing.Imaging.Encoder Quality;
        public static readonly System.Drawing.Imaging.Encoder RenderMethod;
        public static readonly System.Drawing.Imaging.Encoder SaveFlag;
        public static readonly System.Drawing.Imaging.Encoder ScanMethod;
        public static readonly System.Drawing.Imaging.Encoder Transformation;
        public static readonly System.Drawing.Imaging.Encoder Version;
        public Encoder(System.Guid guid) => throw new PlatformNotSupportedException();
        public System.Guid Guid { get { throw new PlatformNotSupportedException(); } }
    }
    public sealed partial class EncoderParameter : System.IDisposable
    {
        public EncoderParameter(System.Drawing.Imaging.Encoder encoder, byte value) => throw new PlatformNotSupportedException();
        public EncoderParameter(System.Drawing.Imaging.Encoder encoder, byte value, bool undefined) => throw new PlatformNotSupportedException();
        public EncoderParameter(System.Drawing.Imaging.Encoder encoder, byte[] value) => throw new PlatformNotSupportedException();
        public EncoderParameter(System.Drawing.Imaging.Encoder encoder, byte[] value, bool undefined) => throw new PlatformNotSupportedException();
        public EncoderParameter(System.Drawing.Imaging.Encoder encoder, short value) => throw new PlatformNotSupportedException();
        public EncoderParameter(System.Drawing.Imaging.Encoder encoder, short[] value) => throw new PlatformNotSupportedException();
        public EncoderParameter(System.Drawing.Imaging.Encoder encoder, int numberValues, System.Drawing.Imaging.EncoderParameterValueType type, System.IntPtr value) => throw new PlatformNotSupportedException();
        public EncoderParameter(System.Drawing.Imaging.Encoder encoder, int numerator, int denominator) => throw new PlatformNotSupportedException();
        [System.ObsoleteAttribute("This constructor has been deprecated. Use EncoderParameter(Encoder encoder, int numberValues, EncoderParameterValueType type, IntPtr value) instead.  https://go.microsoft.com/fwlink/?linkid=14202")]
        public EncoderParameter(System.Drawing.Imaging.Encoder encoder, int NumberOfValues, int Type, int Value) => throw new PlatformNotSupportedException();
        public EncoderParameter(System.Drawing.Imaging.Encoder encoder, int numerator1, int demoninator1, int numerator2, int demoninator2) => throw new PlatformNotSupportedException();
        public EncoderParameter(System.Drawing.Imaging.Encoder encoder, int[] numerator, int[] denominator) => throw new PlatformNotSupportedException();
        public EncoderParameter(System.Drawing.Imaging.Encoder encoder, int[] numerator1, int[] denominator1, int[] numerator2, int[] denominator2) => throw new PlatformNotSupportedException();
        public EncoderParameter(System.Drawing.Imaging.Encoder encoder, long value) => throw new PlatformNotSupportedException();
        public EncoderParameter(System.Drawing.Imaging.Encoder encoder, long rangebegin, long rangeend) => throw new PlatformNotSupportedException();
        public EncoderParameter(System.Drawing.Imaging.Encoder encoder, long[] value) => throw new PlatformNotSupportedException();
        public EncoderParameter(System.Drawing.Imaging.Encoder encoder, long[] rangebegin, long[] rangeend) => throw new PlatformNotSupportedException();
        public EncoderParameter(System.Drawing.Imaging.Encoder encoder, string value) => throw new PlatformNotSupportedException();
        public System.Drawing.Imaging.Encoder Encoder { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public int NumberOfValues { get { throw new PlatformNotSupportedException(); } }
        public System.Drawing.Imaging.EncoderParameterValueType Type { get { throw new PlatformNotSupportedException(); } }
        public System.Drawing.Imaging.EncoderParameterValueType ValueType { get { throw new PlatformNotSupportedException(); } }
        public void Dispose() => throw new PlatformNotSupportedException();
        ~EncoderParameter() => throw new PlatformNotSupportedException();
    }
    public sealed partial class EncoderParameters : System.IDisposable
    {
        public EncoderParameters() => throw new PlatformNotSupportedException();
        public EncoderParameters(int count) => throw new PlatformNotSupportedException();
        public System.Drawing.Imaging.EncoderParameter[] Param { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public void Dispose() => throw new PlatformNotSupportedException();
    }
    public enum EncoderParameterValueType
    {
        ValueTypeAscii = 2,
        ValueTypeByte = 1,
        ValueTypeLong = 4,
        ValueTypeLongRange = 6,
        ValueTypeRational = 5,
        ValueTypeRationalRange = 8,
        ValueTypeShort = 3,
        ValueTypeUndefined = 7,
    }
    public enum EncoderValue
    {
        ColorTypeCMYK = 0,
        ColorTypeYCCK = 1,
        CompressionCCITT3 = 3,
        CompressionCCITT4 = 4,
        CompressionLZW = 2,
        CompressionNone = 6,
        CompressionRle = 5,
        Flush = 20,
        FrameDimensionPage = 23,
        FrameDimensionResolution = 22,
        FrameDimensionTime = 21,
        LastFrame = 19,
        MultiFrame = 18,
        RenderNonProgressive = 12,
        RenderProgressive = 11,
        ScanMethodInterlaced = 7,
        ScanMethodNonInterlaced = 8,
        TransformFlipHorizontal = 16,
        TransformFlipVertical = 17,
        TransformRotate180 = 14,
        TransformRotate270 = 15,
        TransformRotate90 = 13,
        VersionGif87 = 9,
        VersionGif89 = 10,
    }
    public sealed partial class FrameDimension
    {
        public FrameDimension(System.Guid guid) => throw new PlatformNotSupportedException();
        public System.Guid Guid { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Imaging.FrameDimension Page { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Imaging.FrameDimension Resolution { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Imaging.FrameDimension Time { get { throw new PlatformNotSupportedException(); } }
        public override bool Equals(object o) { throw new PlatformNotSupportedException(); }
        public override int GetHashCode() { throw new PlatformNotSupportedException(); }
        public override string ToString() { throw new PlatformNotSupportedException(); }
    }
    public sealed partial class ImageAttributes : System.ICloneable, System.IDisposable
    {
        public ImageAttributes() => throw new PlatformNotSupportedException();
        public void ClearBrushRemapTable() => throw new PlatformNotSupportedException();
        public void ClearColorKey() => throw new PlatformNotSupportedException();
        public void ClearColorKey(System.Drawing.Imaging.ColorAdjustType type) => throw new PlatformNotSupportedException();
        public void ClearColorMatrix() => throw new PlatformNotSupportedException();
        public void ClearColorMatrix(System.Drawing.Imaging.ColorAdjustType type) => throw new PlatformNotSupportedException();
        public void ClearGamma() => throw new PlatformNotSupportedException();
        public void ClearGamma(System.Drawing.Imaging.ColorAdjustType type) => throw new PlatformNotSupportedException();
        public void ClearNoOp() => throw new PlatformNotSupportedException();
        public void ClearNoOp(System.Drawing.Imaging.ColorAdjustType type) => throw new PlatformNotSupportedException();
        public void ClearOutputChannel() => throw new PlatformNotSupportedException();
        public void ClearOutputChannel(System.Drawing.Imaging.ColorAdjustType type) => throw new PlatformNotSupportedException();
        public void ClearOutputChannelColorProfile() => throw new PlatformNotSupportedException();
        public void ClearOutputChannelColorProfile(System.Drawing.Imaging.ColorAdjustType type) => throw new PlatformNotSupportedException();
        public void ClearRemapTable() => throw new PlatformNotSupportedException();
        public void ClearRemapTable(System.Drawing.Imaging.ColorAdjustType type) => throw new PlatformNotSupportedException();
        public void ClearThreshold() => throw new PlatformNotSupportedException();
        public void ClearThreshold(System.Drawing.Imaging.ColorAdjustType type) => throw new PlatformNotSupportedException();
        public object Clone() { throw new PlatformNotSupportedException(); }
        public void Dispose() => throw new PlatformNotSupportedException();
        ~ImageAttributes() => throw new PlatformNotSupportedException();
        public void GetAdjustedPalette(System.Drawing.Imaging.ColorPalette palette, System.Drawing.Imaging.ColorAdjustType type) => throw new PlatformNotSupportedException();
        public void SetBrushRemapTable(System.Drawing.Imaging.ColorMap[] map) => throw new PlatformNotSupportedException();
        public void SetColorKey(System.Drawing.Color colorLow, System.Drawing.Color colorHigh) => throw new PlatformNotSupportedException();
        public void SetColorKey(System.Drawing.Color colorLow, System.Drawing.Color colorHigh, System.Drawing.Imaging.ColorAdjustType type) => throw new PlatformNotSupportedException();
        public void SetColorMatrices(System.Drawing.Imaging.ColorMatrix newColorMatrix, System.Drawing.Imaging.ColorMatrix grayMatrix) => throw new PlatformNotSupportedException();
        public void SetColorMatrices(System.Drawing.Imaging.ColorMatrix newColorMatrix, System.Drawing.Imaging.ColorMatrix grayMatrix, System.Drawing.Imaging.ColorMatrixFlag flags) => throw new PlatformNotSupportedException();
        public void SetColorMatrices(System.Drawing.Imaging.ColorMatrix newColorMatrix, System.Drawing.Imaging.ColorMatrix grayMatrix, System.Drawing.Imaging.ColorMatrixFlag mode, System.Drawing.Imaging.ColorAdjustType type) => throw new PlatformNotSupportedException();
        public void SetColorMatrix(System.Drawing.Imaging.ColorMatrix newColorMatrix) => throw new PlatformNotSupportedException();
        public void SetColorMatrix(System.Drawing.Imaging.ColorMatrix newColorMatrix, System.Drawing.Imaging.ColorMatrixFlag flags) => throw new PlatformNotSupportedException();
        public void SetColorMatrix(System.Drawing.Imaging.ColorMatrix newColorMatrix, System.Drawing.Imaging.ColorMatrixFlag mode, System.Drawing.Imaging.ColorAdjustType type) => throw new PlatformNotSupportedException();
        public void SetGamma(float gamma) => throw new PlatformNotSupportedException();
        public void SetGamma(float gamma, System.Drawing.Imaging.ColorAdjustType type) => throw new PlatformNotSupportedException();
        public void SetNoOp() => throw new PlatformNotSupportedException();
        public void SetNoOp(System.Drawing.Imaging.ColorAdjustType type) => throw new PlatformNotSupportedException();
        public void SetOutputChannel(System.Drawing.Imaging.ColorChannelFlag flags) => throw new PlatformNotSupportedException();
        public void SetOutputChannel(System.Drawing.Imaging.ColorChannelFlag flags, System.Drawing.Imaging.ColorAdjustType type) => throw new PlatformNotSupportedException();
        public void SetOutputChannelColorProfile(string colorProfileFilename) => throw new PlatformNotSupportedException();
        public void SetOutputChannelColorProfile(string colorProfileFilename, System.Drawing.Imaging.ColorAdjustType type) => throw new PlatformNotSupportedException();
        public void SetRemapTable(System.Drawing.Imaging.ColorMap[] map) => throw new PlatformNotSupportedException();
        public void SetRemapTable(System.Drawing.Imaging.ColorMap[] map, System.Drawing.Imaging.ColorAdjustType type) => throw new PlatformNotSupportedException();
        public void SetThreshold(float threshold) => throw new PlatformNotSupportedException();
        public void SetThreshold(float threshold, System.Drawing.Imaging.ColorAdjustType type) => throw new PlatformNotSupportedException();
        public void SetWrapMode(System.Drawing.Drawing2D.WrapMode mode) => throw new PlatformNotSupportedException();
        public void SetWrapMode(System.Drawing.Drawing2D.WrapMode mode, System.Drawing.Color color) => throw new PlatformNotSupportedException();
        public void SetWrapMode(System.Drawing.Drawing2D.WrapMode mode, System.Drawing.Color color, bool clamp) => throw new PlatformNotSupportedException();
    }
    [System.FlagsAttribute]
    public enum ImageCodecFlags
    {
        BlockingDecode = 32,
        Builtin = 65536,
        Decoder = 2,
        Encoder = 1,
        SeekableEncode = 16,
        SupportBitmap = 4,
        SupportVector = 8,
        System = 131072,
        User = 262144,
    }
    public sealed partial class ImageCodecInfo
    {
        internal ImageCodecInfo() => throw new PlatformNotSupportedException();
        public System.Guid Clsid { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public string CodecName { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public string DllName { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public string FilenameExtension { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Imaging.ImageCodecFlags Flags { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public string FormatDescription { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Guid FormatID { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public string MimeType { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        [System.CLSCompliantAttribute(false)]
        public byte[][] SignatureMasks { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        [System.CLSCompliantAttribute(false)]
        public byte[][] SignaturePatterns { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public int Version { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public static System.Drawing.Imaging.ImageCodecInfo[] GetImageDecoders() { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Imaging.ImageCodecInfo[] GetImageEncoders() { throw new PlatformNotSupportedException(); }
    }
    [System.FlagsAttribute]
    public enum ImageFlags
    {
        Caching = 131072,
        ColorSpaceCmyk = 32,
        ColorSpaceGray = 64,
        ColorSpaceRgb = 16,
        ColorSpaceYcbcr = 128,
        ColorSpaceYcck = 256,
        HasAlpha = 2,
        HasRealDpi = 4096,
        HasRealPixelSize = 8192,
        HasTranslucent = 4,
        None = 0,
        PartiallyScalable = 8,
        ReadOnly = 65536,
        Scalable = 1,
    }
#if netcoreapp
    [System.ComponentModel.TypeConverter("System.Drawing.ImageFormatConverter, System.Windows.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51")]
#endif
    public sealed partial class ImageFormat
    {
        public ImageFormat(System.Guid guid) => throw new PlatformNotSupportedException();
        public static System.Drawing.Imaging.ImageFormat Bmp { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Imaging.ImageFormat Emf { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Imaging.ImageFormat Exif { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Imaging.ImageFormat Gif { get { throw new PlatformNotSupportedException(); } }
        public System.Guid Guid { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Imaging.ImageFormat Icon { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Imaging.ImageFormat Jpeg { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Imaging.ImageFormat MemoryBmp { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Imaging.ImageFormat Png { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Imaging.ImageFormat Tiff { get { throw new PlatformNotSupportedException(); } }
        public static System.Drawing.Imaging.ImageFormat Wmf { get { throw new PlatformNotSupportedException(); } }
        public override bool Equals(object o) { throw new PlatformNotSupportedException(); }
        public override int GetHashCode() { throw new PlatformNotSupportedException(); }
        public override string ToString() { throw new PlatformNotSupportedException(); }
    }
    public enum ImageLockMode
    {
        ReadOnly = 1,
        ReadWrite = 3,
        UserInputBuffer = 4,
        WriteOnly = 2,
    }
    public sealed partial class Metafile : System.Drawing.Image
    {
        public Metafile(System.IntPtr henhmetafile, bool deleteEmf) => throw new PlatformNotSupportedException();
        public Metafile(System.IntPtr referenceHdc, System.Drawing.Imaging.EmfType emfType) => throw new PlatformNotSupportedException();
        public Metafile(System.IntPtr referenceHdc, System.Drawing.Imaging.EmfType emfType, string description) => throw new PlatformNotSupportedException();
        public Metafile(System.IntPtr hmetafile, System.Drawing.Imaging.WmfPlaceableFileHeader wmfHeader) => throw new PlatformNotSupportedException();
        public Metafile(System.IntPtr hmetafile, System.Drawing.Imaging.WmfPlaceableFileHeader wmfHeader, bool deleteWmf) => throw new PlatformNotSupportedException();
        public Metafile(System.IntPtr referenceHdc, System.Drawing.Rectangle frameRect) => throw new PlatformNotSupportedException();
        public Metafile(System.IntPtr referenceHdc, System.Drawing.Rectangle frameRect, System.Drawing.Imaging.MetafileFrameUnit frameUnit) => throw new PlatformNotSupportedException();
        public Metafile(System.IntPtr referenceHdc, System.Drawing.Rectangle frameRect, System.Drawing.Imaging.MetafileFrameUnit frameUnit, System.Drawing.Imaging.EmfType type) => throw new PlatformNotSupportedException();
        public Metafile(System.IntPtr referenceHdc, System.Drawing.Rectangle frameRect, System.Drawing.Imaging.MetafileFrameUnit frameUnit, System.Drawing.Imaging.EmfType type, string desc) => throw new PlatformNotSupportedException();
        public Metafile(System.IntPtr referenceHdc, System.Drawing.RectangleF frameRect) => throw new PlatformNotSupportedException();
        public Metafile(System.IntPtr referenceHdc, System.Drawing.RectangleF frameRect, System.Drawing.Imaging.MetafileFrameUnit frameUnit) => throw new PlatformNotSupportedException();
        public Metafile(System.IntPtr referenceHdc, System.Drawing.RectangleF frameRect, System.Drawing.Imaging.MetafileFrameUnit frameUnit, System.Drawing.Imaging.EmfType type) => throw new PlatformNotSupportedException();
        public Metafile(System.IntPtr referenceHdc, System.Drawing.RectangleF frameRect, System.Drawing.Imaging.MetafileFrameUnit frameUnit, System.Drawing.Imaging.EmfType type, string description) => throw new PlatformNotSupportedException();
        public Metafile(System.IO.Stream stream) => throw new PlatformNotSupportedException();
        public Metafile(System.IO.Stream stream, System.IntPtr referenceHdc) => throw new PlatformNotSupportedException();
        public Metafile(System.IO.Stream stream, System.IntPtr referenceHdc, System.Drawing.Imaging.EmfType type) => throw new PlatformNotSupportedException();
        public Metafile(System.IO.Stream stream, System.IntPtr referenceHdc, System.Drawing.Imaging.EmfType type, string description) => throw new PlatformNotSupportedException();
        public Metafile(System.IO.Stream stream, System.IntPtr referenceHdc, System.Drawing.Rectangle frameRect) => throw new PlatformNotSupportedException();
        public Metafile(System.IO.Stream stream, System.IntPtr referenceHdc, System.Drawing.Rectangle frameRect, System.Drawing.Imaging.MetafileFrameUnit frameUnit) => throw new PlatformNotSupportedException();
        public Metafile(System.IO.Stream stream, System.IntPtr referenceHdc, System.Drawing.Rectangle frameRect, System.Drawing.Imaging.MetafileFrameUnit frameUnit, System.Drawing.Imaging.EmfType type) => throw new PlatformNotSupportedException();
        public Metafile(System.IO.Stream stream, System.IntPtr referenceHdc, System.Drawing.Rectangle frameRect, System.Drawing.Imaging.MetafileFrameUnit frameUnit, System.Drawing.Imaging.EmfType type, string description) => throw new PlatformNotSupportedException();
        public Metafile(System.IO.Stream stream, System.IntPtr referenceHdc, System.Drawing.RectangleF frameRect) => throw new PlatformNotSupportedException();
        public Metafile(System.IO.Stream stream, System.IntPtr referenceHdc, System.Drawing.RectangleF frameRect, System.Drawing.Imaging.MetafileFrameUnit frameUnit) => throw new PlatformNotSupportedException();
        public Metafile(System.IO.Stream stream, System.IntPtr referenceHdc, System.Drawing.RectangleF frameRect, System.Drawing.Imaging.MetafileFrameUnit frameUnit, System.Drawing.Imaging.EmfType type) => throw new PlatformNotSupportedException();
        public Metafile(System.IO.Stream stream, System.IntPtr referenceHdc, System.Drawing.RectangleF frameRect, System.Drawing.Imaging.MetafileFrameUnit frameUnit, System.Drawing.Imaging.EmfType type, string description) => throw new PlatformNotSupportedException();
        public Metafile(string filename) => throw new PlatformNotSupportedException();
        public Metafile(string fileName, System.IntPtr referenceHdc) => throw new PlatformNotSupportedException();
        public Metafile(string fileName, System.IntPtr referenceHdc, System.Drawing.Imaging.EmfType type) => throw new PlatformNotSupportedException();
        public Metafile(string fileName, System.IntPtr referenceHdc, System.Drawing.Imaging.EmfType type, string description) => throw new PlatformNotSupportedException();
        public Metafile(string fileName, System.IntPtr referenceHdc, System.Drawing.Rectangle frameRect) => throw new PlatformNotSupportedException();
        public Metafile(string fileName, System.IntPtr referenceHdc, System.Drawing.Rectangle frameRect, System.Drawing.Imaging.MetafileFrameUnit frameUnit) => throw new PlatformNotSupportedException();
        public Metafile(string fileName, System.IntPtr referenceHdc, System.Drawing.Rectangle frameRect, System.Drawing.Imaging.MetafileFrameUnit frameUnit, System.Drawing.Imaging.EmfType type) => throw new PlatformNotSupportedException();
        public Metafile(string fileName, System.IntPtr referenceHdc, System.Drawing.Rectangle frameRect, System.Drawing.Imaging.MetafileFrameUnit frameUnit, System.Drawing.Imaging.EmfType type, string description) => throw new PlatformNotSupportedException();
        public Metafile(string fileName, System.IntPtr referenceHdc, System.Drawing.Rectangle frameRect, System.Drawing.Imaging.MetafileFrameUnit frameUnit, string description) => throw new PlatformNotSupportedException();
        public Metafile(string fileName, System.IntPtr referenceHdc, System.Drawing.RectangleF frameRect) => throw new PlatformNotSupportedException();
        public Metafile(string fileName, System.IntPtr referenceHdc, System.Drawing.RectangleF frameRect, System.Drawing.Imaging.MetafileFrameUnit frameUnit) => throw new PlatformNotSupportedException();
        public Metafile(string fileName, System.IntPtr referenceHdc, System.Drawing.RectangleF frameRect, System.Drawing.Imaging.MetafileFrameUnit frameUnit, System.Drawing.Imaging.EmfType type) => throw new PlatformNotSupportedException();
        public Metafile(string fileName, System.IntPtr referenceHdc, System.Drawing.RectangleF frameRect, System.Drawing.Imaging.MetafileFrameUnit frameUnit, System.Drawing.Imaging.EmfType type, string description) => throw new PlatformNotSupportedException();
        public Metafile(string fileName, System.IntPtr referenceHdc, System.Drawing.RectangleF frameRect, System.Drawing.Imaging.MetafileFrameUnit frameUnit, string desc) => throw new PlatformNotSupportedException();
        public System.IntPtr GetHenhmetafile() { throw new PlatformNotSupportedException(); }
        public System.Drawing.Imaging.MetafileHeader GetMetafileHeader() { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Imaging.MetafileHeader GetMetafileHeader(System.IntPtr henhmetafile) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Imaging.MetafileHeader GetMetafileHeader(System.IntPtr hmetafile, System.Drawing.Imaging.WmfPlaceableFileHeader wmfHeader) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Imaging.MetafileHeader GetMetafileHeader(System.IO.Stream stream) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Imaging.MetafileHeader GetMetafileHeader(string fileName) { throw new PlatformNotSupportedException(); }
        public void PlayRecord(System.Drawing.Imaging.EmfPlusRecordType recordType, int flags, int dataSize, byte[] data) => throw new PlatformNotSupportedException();
    }
    public enum MetafileFrameUnit
    {
        Document = 5,
        GdiCompatible = 7,
        Inch = 4,
        Millimeter = 6,
        Pixel = 2,
        Point = 3,
    }
    public sealed partial class MetafileHeader
    {
        internal MetafileHeader() => throw new PlatformNotSupportedException();
        public System.Drawing.Rectangle Bounds { get { throw new PlatformNotSupportedException(); } }
        public float DpiX { get { throw new PlatformNotSupportedException(); } }
        public float DpiY { get { throw new PlatformNotSupportedException(); } }
        public int EmfPlusHeaderSize { get { throw new PlatformNotSupportedException(); } }
        public int LogicalDpiX { get { throw new PlatformNotSupportedException(); } }
        public int LogicalDpiY { get { throw new PlatformNotSupportedException(); } }
        public int MetafileSize { get { throw new PlatformNotSupportedException(); } }
        public System.Drawing.Imaging.MetafileType Type { get { throw new PlatformNotSupportedException(); } }
        public int Version { get { throw new PlatformNotSupportedException(); } }
        public System.Drawing.Imaging.MetaHeader WmfHeader { get { throw new PlatformNotSupportedException(); } }
        public bool IsDisplay() { throw new PlatformNotSupportedException(); }
        public bool IsEmf() { throw new PlatformNotSupportedException(); }
        public bool IsEmfOrEmfPlus() { throw new PlatformNotSupportedException(); }
        public bool IsEmfPlus() { throw new PlatformNotSupportedException(); }
        public bool IsEmfPlusDual() { throw new PlatformNotSupportedException(); }
        public bool IsEmfPlusOnly() { throw new PlatformNotSupportedException(); }
        public bool IsWmf() { throw new PlatformNotSupportedException(); }
        public bool IsWmfPlaceable() { throw new PlatformNotSupportedException(); }
    }
    public enum MetafileType
    {
        Emf = 3,
        EmfPlusDual = 5,
        EmfPlusOnly = 4,
        Invalid = 0,
        Wmf = 1,
        WmfPlaceable = 2,
    }
    public sealed partial class MetaHeader
    {
        public MetaHeader() => throw new PlatformNotSupportedException();
        public short HeaderSize { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public int MaxRecord { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public short NoObjects { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public short NoParameters { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public int Size { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public short Type { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public short Version { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
    }
    [System.FlagsAttribute]
    public enum PaletteFlags
    {
        GrayScale = 2,
        Halftone = 4,
        HasAlpha = 1,
    }
    public enum PixelFormat
    {
        Alpha = 262144,
        Canonical = 2097152,
        DontCare = 0,
        Extended = 1048576,
        Format16bppArgb1555 = 397319,
        Format16bppGrayScale = 1052676,
        Format16bppRgb555 = 135173,
        Format16bppRgb565 = 135174,
        Format1bppIndexed = 196865,
        Format24bppRgb = 137224,
        Format32bppArgb = 2498570,
        Format32bppPArgb = 925707,
        Format32bppRgb = 139273,
        Format48bppRgb = 1060876,
        Format4bppIndexed = 197634,
        Format64bppArgb = 3424269,
        Format64bppPArgb = 1851406,
        Format8bppIndexed = 198659,
        Gdi = 131072,
        Indexed = 65536,
        Max = 15,
        PAlpha = 524288,
        Undefined = 0,
    }
    public delegate void PlayRecordCallback(System.Drawing.Imaging.EmfPlusRecordType recordType, int flags, int dataSize, System.IntPtr recordData);
    public sealed partial class PropertyItem
    {
        internal PropertyItem() => throw new PlatformNotSupportedException();
        public int Id { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public int Len { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public short Type { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public byte[] Value { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
    }
    public sealed partial class WmfPlaceableFileHeader
    {
        public WmfPlaceableFileHeader() => throw new PlatformNotSupportedException();
        public short BboxBottom { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public short BboxLeft { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public short BboxRight { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public short BboxTop { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public short Checksum { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public short Hmf { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public short Inch { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public int Key { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public int Reserved { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
    }
}
namespace System.Drawing.Printing
{
    public enum Duplex
    {
        Default = -1,
        Horizontal = 3,
        Simplex = 1,
        Vertical = 2,
    }
    public partial class InvalidPrinterException : System.SystemException
    {
        public InvalidPrinterException(System.Drawing.Printing.PrinterSettings settings) => throw new PlatformNotSupportedException();
        protected InvalidPrinterException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) => throw new PlatformNotSupportedException();
        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) => throw new PlatformNotSupportedException();
    }
#if netcoreapp
    [System.ComponentModel.TypeConverter("System.Drawing.Printing.MarginsConverter, System.Windows.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51")]
#endif
    public partial class Margins : System.ICloneable
    {
        public Margins() => throw new PlatformNotSupportedException();
        public Margins(int left, int right, int top, int bottom) => throw new PlatformNotSupportedException();
        public int Bottom { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public int Left { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public int Right { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public int Top { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public object Clone() { throw new PlatformNotSupportedException(); }
        public override bool Equals(object obj) { throw new PlatformNotSupportedException(); }
        public override int GetHashCode() { throw new PlatformNotSupportedException(); }
        public static bool operator ==(System.Drawing.Printing.Margins m1, System.Drawing.Printing.Margins m2) { throw new PlatformNotSupportedException(); }
        public static bool operator !=(System.Drawing.Printing.Margins m1, System.Drawing.Printing.Margins m2) { throw new PlatformNotSupportedException(); }
        public override string ToString() { throw new PlatformNotSupportedException(); }
    }
    public partial class PageSettings : System.ICloneable
    {
        public PageSettings() => throw new PlatformNotSupportedException();
        public PageSettings(System.Drawing.Printing.PrinterSettings printerSettings) => throw new PlatformNotSupportedException();
        public System.Drawing.Rectangle Bounds { get { throw new PlatformNotSupportedException(); } }
        public bool Color { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public float HardMarginX { get { throw new PlatformNotSupportedException(); } }
        public float HardMarginY { get { throw new PlatformNotSupportedException(); } }
        public bool Landscape { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Printing.Margins Margins { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Printing.PaperSize PaperSize { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Printing.PaperSource PaperSource { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.RectangleF PrintableArea { get { throw new PlatformNotSupportedException(); } }
        public System.Drawing.Printing.PrinterResolution PrinterResolution { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Printing.PrinterSettings PrinterSettings { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public object Clone() { throw new PlatformNotSupportedException(); }
        public void CopyToHdevmode(System.IntPtr hdevmode) => throw new PlatformNotSupportedException();
        public void SetHdevmode(System.IntPtr hdevmode) => throw new PlatformNotSupportedException();
        public override string ToString() { throw new PlatformNotSupportedException(); }
    }
    public enum PaperKind
    {
        A2 = 66,
        A3 = 8,
        A3Extra = 63,
        A3ExtraTransverse = 68,
        A3Rotated = 76,
        A3Transverse = 67,
        A4 = 9,
        A4Extra = 53,
        A4Plus = 60,
        A4Rotated = 77,
        A4Small = 10,
        A4Transverse = 55,
        A5 = 11,
        A5Extra = 64,
        A5Rotated = 78,
        A5Transverse = 61,
        A6 = 70,
        A6Rotated = 83,
        APlus = 57,
        B4 = 12,
        B4Envelope = 33,
        B4JisRotated = 79,
        B5 = 13,
        B5Envelope = 34,
        B5Extra = 65,
        B5JisRotated = 80,
        B5Transverse = 62,
        B6Envelope = 35,
        B6Jis = 88,
        B6JisRotated = 89,
        BPlus = 58,
        C3Envelope = 29,
        C4Envelope = 30,
        C5Envelope = 28,
        C65Envelope = 32,
        C6Envelope = 31,
        CSheet = 24,
        Custom = 0,
        DLEnvelope = 27,
        DSheet = 25,
        ESheet = 26,
        Executive = 7,
        Folio = 14,
        GermanLegalFanfold = 41,
        GermanStandardFanfold = 40,
        InviteEnvelope = 47,
        IsoB4 = 42,
        ItalyEnvelope = 36,
        JapaneseDoublePostcard = 69,
        JapaneseDoublePostcardRotated = 82,
        JapaneseEnvelopeChouNumber3 = 73,
        JapaneseEnvelopeChouNumber3Rotated = 86,
        JapaneseEnvelopeChouNumber4 = 74,
        JapaneseEnvelopeChouNumber4Rotated = 87,
        JapaneseEnvelopeKakuNumber2 = 71,
        JapaneseEnvelopeKakuNumber2Rotated = 84,
        JapaneseEnvelopeKakuNumber3 = 72,
        JapaneseEnvelopeKakuNumber3Rotated = 85,
        JapaneseEnvelopeYouNumber4 = 91,
        JapaneseEnvelopeYouNumber4Rotated = 92,
        JapanesePostcard = 43,
        JapanesePostcardRotated = 81,
        Ledger = 4,
        Legal = 5,
        LegalExtra = 51,
        Letter = 1,
        LetterExtra = 50,
        LetterExtraTransverse = 56,
        LetterPlus = 59,
        LetterRotated = 75,
        LetterSmall = 2,
        LetterTransverse = 54,
        MonarchEnvelope = 37,
        Note = 18,
        Number10Envelope = 20,
        Number11Envelope = 21,
        Number12Envelope = 22,
        Number14Envelope = 23,
        Number9Envelope = 19,
        PersonalEnvelope = 38,
        Prc16K = 93,
        Prc16KRotated = 106,
        Prc32K = 94,
        Prc32KBig = 95,
        Prc32KBigRotated = 108,
        Prc32KRotated = 107,
        PrcEnvelopeNumber1 = 96,
        PrcEnvelopeNumber10 = 105,
        PrcEnvelopeNumber10Rotated = 118,
        PrcEnvelopeNumber1Rotated = 109,
        PrcEnvelopeNumber2 = 97,
        PrcEnvelopeNumber2Rotated = 110,
        PrcEnvelopeNumber3 = 98,
        PrcEnvelopeNumber3Rotated = 111,
        PrcEnvelopeNumber4 = 99,
        PrcEnvelopeNumber4Rotated = 112,
        PrcEnvelopeNumber5 = 100,
        PrcEnvelopeNumber5Rotated = 113,
        PrcEnvelopeNumber6 = 101,
        PrcEnvelopeNumber6Rotated = 114,
        PrcEnvelopeNumber7 = 102,
        PrcEnvelopeNumber7Rotated = 115,
        PrcEnvelopeNumber8 = 103,
        PrcEnvelopeNumber8Rotated = 116,
        PrcEnvelopeNumber9 = 104,
        PrcEnvelopeNumber9Rotated = 117,
        Quarto = 15,
        Standard10x11 = 45,
        Standard10x14 = 16,
        Standard11x17 = 17,
        Standard12x11 = 90,
        Standard15x11 = 46,
        Standard9x11 = 44,
        Statement = 6,
        Tabloid = 3,
        TabloidExtra = 52,
        USStandardFanfold = 39,
    }
    public partial class PaperSize
    {
        public PaperSize() => throw new PlatformNotSupportedException();
        public PaperSize(string name, int width, int height) => throw new PlatformNotSupportedException();
        public int Height { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Printing.PaperKind Kind { get { throw new PlatformNotSupportedException(); } }
        public string PaperName { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public int RawKind { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public int Width { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public override string ToString() { throw new PlatformNotSupportedException(); }
    }
    public partial class PaperSource
    {
        public PaperSource() => throw new PlatformNotSupportedException();
        public System.Drawing.Printing.PaperSourceKind Kind { get { throw new PlatformNotSupportedException(); } }
        public int RawKind { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public string SourceName { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public override string ToString() { throw new PlatformNotSupportedException(); }
    }
    public enum PaperSourceKind
    {
        AutomaticFeed = 7,
        Cassette = 14,
        Custom = 257,
        Envelope = 5,
        FormSource = 15,
        LargeCapacity = 11,
        LargeFormat = 10,
        Lower = 2,
        Manual = 4,
        ManualFeed = 6,
        Middle = 3,
        SmallFormat = 9,
        TractorFeed = 8,
        Upper = 1,
    }
    public sealed partial class PreviewPageInfo
    {
        public PreviewPageInfo(System.Drawing.Image image, System.Drawing.Size physicalSize) => throw new PlatformNotSupportedException();
        public System.Drawing.Image Image { get { throw new PlatformNotSupportedException(); } }
        public System.Drawing.Size PhysicalSize { get { throw new PlatformNotSupportedException(); } }
    }
    public partial class PreviewPrintController : System.Drawing.Printing.PrintController
    {
        public PreviewPrintController() => throw new PlatformNotSupportedException();
        public override bool IsPreview { get { throw new PlatformNotSupportedException(); } }
        public virtual bool UseAntiAlias { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Printing.PreviewPageInfo[] GetPreviewPageInfo() { throw new PlatformNotSupportedException(); }
        public override void OnEndPage(System.Drawing.Printing.PrintDocument document, System.Drawing.Printing.PrintPageEventArgs e) => throw new PlatformNotSupportedException();
        public override void OnEndPrint(System.Drawing.Printing.PrintDocument document, System.Drawing.Printing.PrintEventArgs e) => throw new PlatformNotSupportedException();
        public override System.Drawing.Graphics OnStartPage(System.Drawing.Printing.PrintDocument document, System.Drawing.Printing.PrintPageEventArgs e) { throw new PlatformNotSupportedException(); }
        public override void OnStartPrint(System.Drawing.Printing.PrintDocument document, System.Drawing.Printing.PrintEventArgs e) => throw new PlatformNotSupportedException();
    }
    public enum PrintAction
    {
        PrintToFile = 0,
        PrintToPreview = 1,
        PrintToPrinter = 2,
    }
    public abstract partial class PrintController
    {
        protected PrintController() => throw new PlatformNotSupportedException();
        public virtual bool IsPreview { get { throw new PlatformNotSupportedException(); } }
        public virtual void OnEndPage(System.Drawing.Printing.PrintDocument document, System.Drawing.Printing.PrintPageEventArgs e) => throw new PlatformNotSupportedException();
        public virtual void OnEndPrint(System.Drawing.Printing.PrintDocument document, System.Drawing.Printing.PrintEventArgs e) => throw new PlatformNotSupportedException();
        public virtual System.Drawing.Graphics OnStartPage(System.Drawing.Printing.PrintDocument document, System.Drawing.Printing.PrintPageEventArgs e) { throw new PlatformNotSupportedException(); }
        public virtual void OnStartPrint(System.Drawing.Printing.PrintDocument document, System.Drawing.Printing.PrintEventArgs e) => throw new PlatformNotSupportedException();
    }
    public partial class PrintDocument : System.ComponentModel.Component
    {
        public PrintDocument() => throw new PlatformNotSupportedException();
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public System.Drawing.Printing.PageSettings DefaultPageSettings { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        [System.ComponentModel.DefaultValueAttribute("document")]
        public string DocumentName { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool OriginAtMargins { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public System.Drawing.Printing.PrintController PrintController { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute((System.ComponentModel.DesignerSerializationVisibility)(0))]
        public System.Drawing.Printing.PrinterSettings PrinterSettings { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public event System.Drawing.Printing.PrintEventHandler BeginPrint { add => throw new PlatformNotSupportedException(); remove => throw new PlatformNotSupportedException(); }
        public event System.Drawing.Printing.PrintEventHandler EndPrint { add => throw new PlatformNotSupportedException(); remove => throw new PlatformNotSupportedException(); }
        public event System.Drawing.Printing.PrintPageEventHandler PrintPage { add => throw new PlatformNotSupportedException(); remove => throw new PlatformNotSupportedException(); }
        public event System.Drawing.Printing.QueryPageSettingsEventHandler QueryPageSettings { add => throw new PlatformNotSupportedException(); remove => throw new PlatformNotSupportedException(); }
        protected virtual void OnBeginPrint(System.Drawing.Printing.PrintEventArgs e) => throw new PlatformNotSupportedException();
        protected virtual void OnEndPrint(System.Drawing.Printing.PrintEventArgs e) => throw new PlatformNotSupportedException();
        protected virtual void OnPrintPage(System.Drawing.Printing.PrintPageEventArgs e) => throw new PlatformNotSupportedException();
        protected virtual void OnQueryPageSettings(System.Drawing.Printing.QueryPageSettingsEventArgs e) => throw new PlatformNotSupportedException();
        public void Print() => throw new PlatformNotSupportedException();
        public override string ToString() { throw new PlatformNotSupportedException(); }
    }
    public partial class PrinterResolution
    {
        public PrinterResolution() => throw new PlatformNotSupportedException();
        public System.Drawing.Printing.PrinterResolutionKind Kind { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public int X { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public int Y { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public override string ToString() { throw new PlatformNotSupportedException(); }
    }
    public enum PrinterResolutionKind
    {
        Custom = 0,
        Draft = -1,
        High = -4,
        Low = -2,
        Medium = -3,
    }
    public partial class PrinterSettings : System.ICloneable
    {
        public PrinterSettings() => throw new PlatformNotSupportedException();
        public bool CanDuplex { get { throw new PlatformNotSupportedException(); } }
        public bool Collate { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public short Copies { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Printing.PageSettings DefaultPageSettings { get { throw new PlatformNotSupportedException(); } }
        public System.Drawing.Printing.Duplex Duplex { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public int FromPage { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public static System.Drawing.Printing.PrinterSettings.StringCollection InstalledPrinters { get { throw new PlatformNotSupportedException(); } }
        public bool IsDefaultPrinter { get { throw new PlatformNotSupportedException(); } }
        public bool IsPlotter { get { throw new PlatformNotSupportedException(); } }
        public bool IsValid { get { throw new PlatformNotSupportedException(); } }
        public int LandscapeAngle { get { throw new PlatformNotSupportedException(); } }
        public int MaximumCopies { get { throw new PlatformNotSupportedException(); } }
        public int MaximumPage { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public int MinimumPage { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Printing.PrinterSettings.PaperSizeCollection PaperSizes { get { throw new PlatformNotSupportedException(); } }
        public System.Drawing.Printing.PrinterSettings.PaperSourceCollection PaperSources { get { throw new PlatformNotSupportedException(); } }
        public string PrinterName { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Printing.PrinterSettings.PrinterResolutionCollection PrinterResolutions { get { throw new PlatformNotSupportedException(); } }
        public string PrintFileName { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Printing.PrintRange PrintRange { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public bool PrintToFile { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public bool SupportsColor { get { throw new PlatformNotSupportedException(); } }
        public int ToPage { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public object Clone() { throw new PlatformNotSupportedException(); }
        public System.Drawing.Graphics CreateMeasurementGraphics() { throw new PlatformNotSupportedException(); }
        public System.Drawing.Graphics CreateMeasurementGraphics(bool honorOriginAtMargins) { throw new PlatformNotSupportedException(); }
        public System.Drawing.Graphics CreateMeasurementGraphics(System.Drawing.Printing.PageSettings pageSettings) { throw new PlatformNotSupportedException(); }
        public System.Drawing.Graphics CreateMeasurementGraphics(System.Drawing.Printing.PageSettings pageSettings, bool honorOriginAtMargins) { throw new PlatformNotSupportedException(); }
        public System.IntPtr GetHdevmode() { throw new PlatformNotSupportedException(); }
        public System.IntPtr GetHdevmode(System.Drawing.Printing.PageSettings pageSettings) { throw new PlatformNotSupportedException(); }
        public System.IntPtr GetHdevnames() { throw new PlatformNotSupportedException(); }
        public bool IsDirectPrintingSupported(System.Drawing.Image image) { throw new PlatformNotSupportedException(); }
        public bool IsDirectPrintingSupported(System.Drawing.Imaging.ImageFormat imageFormat) { throw new PlatformNotSupportedException(); }
        public void SetHdevmode(System.IntPtr hdevmode) => throw new PlatformNotSupportedException();
        public void SetHdevnames(System.IntPtr hdevnames) => throw new PlatformNotSupportedException();
        public override string ToString() { throw new PlatformNotSupportedException(); }
        public partial class PaperSizeCollection : System.Collections.ICollection, System.Collections.IEnumerable
        {
            public PaperSizeCollection(System.Drawing.Printing.PaperSize[] array) => throw new PlatformNotSupportedException();
            public int Count { get { throw new PlatformNotSupportedException(); } }
            public virtual System.Drawing.Printing.PaperSize this[int index] { get { throw new PlatformNotSupportedException(); } }
            int System.Collections.ICollection.Count { get { throw new PlatformNotSupportedException(); } }
            bool System.Collections.ICollection.IsSynchronized { get { throw new PlatformNotSupportedException(); } }
            object System.Collections.ICollection.SyncRoot { get { throw new PlatformNotSupportedException(); } }
            [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
            public int Add(System.Drawing.Printing.PaperSize paperSize) { throw new PlatformNotSupportedException(); }
            public void CopyTo(System.Drawing.Printing.PaperSize[] paperSizes, int index) => throw new PlatformNotSupportedException();
            public System.Collections.IEnumerator GetEnumerator() { throw new PlatformNotSupportedException(); }
            void System.Collections.ICollection.CopyTo(System.Array array, int index) => throw new PlatformNotSupportedException();
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { throw new PlatformNotSupportedException(); }
        }
        public partial class PaperSourceCollection : System.Collections.ICollection, System.Collections.IEnumerable
        {
            public PaperSourceCollection(System.Drawing.Printing.PaperSource[] array) => throw new PlatformNotSupportedException();
            public int Count { get { throw new PlatformNotSupportedException(); } }
            public virtual System.Drawing.Printing.PaperSource this[int index] { get { throw new PlatformNotSupportedException(); } }
            int System.Collections.ICollection.Count { get { throw new PlatformNotSupportedException(); } }
            bool System.Collections.ICollection.IsSynchronized { get { throw new PlatformNotSupportedException(); } }
            object System.Collections.ICollection.SyncRoot { get { throw new PlatformNotSupportedException(); } }
            [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
            public int Add(System.Drawing.Printing.PaperSource paperSource) { throw new PlatformNotSupportedException(); }
            public void CopyTo(System.Drawing.Printing.PaperSource[] paperSources, int index) => throw new PlatformNotSupportedException();
            public System.Collections.IEnumerator GetEnumerator() { throw new PlatformNotSupportedException(); }
            void System.Collections.ICollection.CopyTo(System.Array array, int index) => throw new PlatformNotSupportedException();
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { throw new PlatformNotSupportedException(); }
        }
        public partial class PrinterResolutionCollection : System.Collections.ICollection, System.Collections.IEnumerable
        {
            public PrinterResolutionCollection(System.Drawing.Printing.PrinterResolution[] array) => throw new PlatformNotSupportedException();
            public int Count { get { throw new PlatformNotSupportedException(); } }
            public virtual System.Drawing.Printing.PrinterResolution this[int index] { get { throw new PlatformNotSupportedException(); } }
            int System.Collections.ICollection.Count { get { throw new PlatformNotSupportedException(); } }
            bool System.Collections.ICollection.IsSynchronized { get { throw new PlatformNotSupportedException(); } }
            object System.Collections.ICollection.SyncRoot { get { throw new PlatformNotSupportedException(); } }
            [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
            public int Add(System.Drawing.Printing.PrinterResolution printerResolution) { throw new PlatformNotSupportedException(); }
            public void CopyTo(System.Drawing.Printing.PrinterResolution[] printerResolutions, int index) => throw new PlatformNotSupportedException();
            public System.Collections.IEnumerator GetEnumerator() { throw new PlatformNotSupportedException(); }
            void System.Collections.ICollection.CopyTo(System.Array array, int index) => throw new PlatformNotSupportedException();
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { throw new PlatformNotSupportedException(); }
        }
        public partial class StringCollection : System.Collections.ICollection, System.Collections.IEnumerable
        {
            public StringCollection(string[] array) => throw new PlatformNotSupportedException();
            public int Count { get { throw new PlatformNotSupportedException(); } }
            public virtual string this[int index] { get { throw new PlatformNotSupportedException(); } }
            int System.Collections.ICollection.Count { get { throw new PlatformNotSupportedException(); } }
            bool System.Collections.ICollection.IsSynchronized { get { throw new PlatformNotSupportedException(); } }
            object System.Collections.ICollection.SyncRoot { get { throw new PlatformNotSupportedException(); } }
            [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
            public int Add(string value) { throw new PlatformNotSupportedException(); }
            public void CopyTo(string[] strings, int index) => throw new PlatformNotSupportedException();
            public System.Collections.IEnumerator GetEnumerator() { throw new PlatformNotSupportedException(); }
            void System.Collections.ICollection.CopyTo(System.Array array, int index) => throw new PlatformNotSupportedException();
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { throw new PlatformNotSupportedException(); }
        }
    }
    public enum PrinterUnit
    {
        Display = 0,
        HundredthsOfAMillimeter = 2,
        TenthsOfAMillimeter = 3,
        ThousandthsOfAnInch = 1,
    }
    public sealed partial class PrinterUnitConvert
    {
        internal PrinterUnitConvert() => throw new PlatformNotSupportedException();
        public static double Convert(double value, System.Drawing.Printing.PrinterUnit fromUnit, System.Drawing.Printing.PrinterUnit toUnit) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Point Convert(System.Drawing.Point value, System.Drawing.Printing.PrinterUnit fromUnit, System.Drawing.Printing.PrinterUnit toUnit) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Printing.Margins Convert(System.Drawing.Printing.Margins value, System.Drawing.Printing.PrinterUnit fromUnit, System.Drawing.Printing.PrinterUnit toUnit) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Rectangle Convert(System.Drawing.Rectangle value, System.Drawing.Printing.PrinterUnit fromUnit, System.Drawing.Printing.PrinterUnit toUnit) { throw new PlatformNotSupportedException(); }
        public static System.Drawing.Size Convert(System.Drawing.Size value, System.Drawing.Printing.PrinterUnit fromUnit, System.Drawing.Printing.PrinterUnit toUnit) { throw new PlatformNotSupportedException(); }
        public static int Convert(int value, System.Drawing.Printing.PrinterUnit fromUnit, System.Drawing.Printing.PrinterUnit toUnit) { throw new PlatformNotSupportedException(); }
    }
    public partial class PrintEventArgs : System.ComponentModel.CancelEventArgs
    {
        public PrintEventArgs() => throw new PlatformNotSupportedException();
        public System.Drawing.Printing.PrintAction PrintAction { get { throw new PlatformNotSupportedException(); } }
    }
    public delegate void PrintEventHandler(object sender, System.Drawing.Printing.PrintEventArgs e);
    public partial class PrintPageEventArgs : System.EventArgs
    {
        public PrintPageEventArgs(System.Drawing.Graphics graphics, System.Drawing.Rectangle marginBounds, System.Drawing.Rectangle pageBounds, System.Drawing.Printing.PageSettings pageSettings) => throw new PlatformNotSupportedException();
        public bool Cancel { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Graphics Graphics { get { throw new PlatformNotSupportedException(); } }
        public bool HasMorePages { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
        public System.Drawing.Rectangle MarginBounds { get { throw new PlatformNotSupportedException(); } }
        public System.Drawing.Rectangle PageBounds { get { throw new PlatformNotSupportedException(); } }
        public System.Drawing.Printing.PageSettings PageSettings { get { throw new PlatformNotSupportedException(); } }
    }
    public delegate void PrintPageEventHandler(object sender, System.Drawing.Printing.PrintPageEventArgs e);
    public enum PrintRange
    {
        AllPages = 0,
        CurrentPage = 4194304,
        Selection = 1,
        SomePages = 2,
    }
    public partial class QueryPageSettingsEventArgs : System.Drawing.Printing.PrintEventArgs
    {
        public QueryPageSettingsEventArgs(System.Drawing.Printing.PageSettings pageSettings) => throw new PlatformNotSupportedException();
        public System.Drawing.Printing.PageSettings PageSettings { get { throw new PlatformNotSupportedException(); } set => throw new PlatformNotSupportedException(); }
    }
    public delegate void QueryPageSettingsEventHandler(object sender, System.Drawing.Printing.QueryPageSettingsEventArgs e);
    public partial class StandardPrintController : System.Drawing.Printing.PrintController
    {
        public StandardPrintController() => throw new PlatformNotSupportedException();
        public override void OnEndPage(System.Drawing.Printing.PrintDocument document, System.Drawing.Printing.PrintPageEventArgs e) => throw new PlatformNotSupportedException();
        public override void OnEndPrint(System.Drawing.Printing.PrintDocument document, System.Drawing.Printing.PrintEventArgs e) => throw new PlatformNotSupportedException();
        public override System.Drawing.Graphics OnStartPage(System.Drawing.Printing.PrintDocument document, System.Drawing.Printing.PrintPageEventArgs e) { throw new PlatformNotSupportedException(); }
        public override void OnStartPrint(System.Drawing.Printing.PrintDocument document, System.Drawing.Printing.PrintEventArgs e) => throw new PlatformNotSupportedException();
    }
}
namespace System.Drawing.Text
{
    public abstract partial class FontCollection : System.IDisposable
    {
        internal FontCollection() => throw new PlatformNotSupportedException();
        public System.Drawing.FontFamily[] Families { get { throw new PlatformNotSupportedException(); } }
        public void Dispose() => throw new PlatformNotSupportedException();
        protected virtual void Dispose(bool disposing) => throw new PlatformNotSupportedException();
        ~FontCollection() => throw new PlatformNotSupportedException();
    }
    public enum GenericFontFamilies
    {
        Monospace = 2,
        SansSerif = 1,
        Serif = 0,
    }
    public enum HotkeyPrefix
    {
        Hide = 2,
        None = 0,
        Show = 1,
    }
    public sealed partial class InstalledFontCollection : System.Drawing.Text.FontCollection
    {
        public InstalledFontCollection() => throw new PlatformNotSupportedException();
    }
    public sealed partial class PrivateFontCollection : System.Drawing.Text.FontCollection
    {
        public PrivateFontCollection() => throw new PlatformNotSupportedException();
        public void AddFontFile(string filename) => throw new PlatformNotSupportedException();
        public void AddMemoryFont(System.IntPtr memory, int length) => throw new PlatformNotSupportedException();
        protected override void Dispose(bool disposing) => throw new PlatformNotSupportedException();
    }
    public enum TextRenderingHint
    {
        AntiAlias = 4,
        AntiAliasGridFit = 3,
        ClearTypeGridFit = 5,
        SingleBitPerPixel = 2,
        SingleBitPerPixelGridFit = 1,
        SystemDefault = 0,
    }
}
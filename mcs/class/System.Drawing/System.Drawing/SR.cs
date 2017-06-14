//
// ExternDll.cs
//
// Author:
//   Frederik Carlier (frederik.carlier@quamotion.mobi)
//
// Copyright (C) 2017 Quamotion bvba http://quamotion.mobi
//

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

namespace System.Drawing
{
	internal class SR
	{ 
		public const string GdiplusGenericError = "A generic error occurred in GDI+.";
		public const string GdiplusInvalidParameter = "Parameter is not valid.";
		public const string GdiplusOutOfMemory = "Out of memory.";
		public const string GdiplusObjectBusy = "Object is currently in use elsewhere.";
		public const string GdiplusInsufficientBuffer = "Buffer is too small (internal GDI+ error).";
		public const string GdiplusNotImplemented = "Not implemented.";
		public const string GdiplusWrongState = "Bitmap region is already locked.";
		public const string GdiplusAborted = "Function was ended.";
		public const string GdiplusFileNotFound = "File not found.";
		public const string GdiplusOverflow = "Overflow error.";
		public const string GdiplusAccessDenied = "File access is denied.";
		public const string GdiplusUnknownImageFormat = "Image format is unknown.";
		public const string GdiplusPropertyNotFoundError = "Property cannot be found.";
		public const string GdiplusPropertyNotSupportedError = "Property is not supported.";
		public const string GdiplusFontFamilyNotFound = "Font '{0}' cannot be found.";
		public const string GdiplusFontStyleNotFound = "Font '{0}' does not support style '{1}'.";
		public const string GdiplusNotTrueTypeFont_NoName = "Only TrueType fonts are supported. This is not a TrueType font.";
		public const string GdiplusUnsupportedGdiplusVersion = "Current version of GDI+ does not support this feature.";
		public const string GdiplusNotInitialized = "GDI+ is not properly initialized (internal GDI+ error).";
		public const string GdiplusUnknown = "Unknown GDI+ error occurred.";
		public const string NotImplemented = "Not implemented.";
		public const string GdiplusInvalidRectangle = "Rectangle '{0}' cannot have a width or height equal to 0.";
		public const string InterpolationColorsCommon = "{0}{1} ColorBlend objects must be constructed with the same number of positions and color values. Positions must be between 0.0 and 1.0, 1.0 indicating the last element in the array.";
		public const string InterpolationColorsColorBlendNotSet = "Property must be set to a valid ColorBlend object to use interpolation colors.";
		public const string InterpolationColorsInvalidColorBlendObject = "ColorBlend object that was set is not valid.";
		public const string InterpolationColorsLength = "Array of colors and positions must contain at least two elements.";
		public const string InterpolationColorsLengthsDiffer = "Colors and positions do not have the same number of elements.";
		public const string InterpolationColorsInvalidStartPosition = "Position's first element must be equal to 0.";
		public const string InterpolationColorsInvalidEndPosition = "Position's last element must be equal to 1.0.";
		public const string CantChangeImmutableObjects = "Changes cannot be made to {0} because permissions are not valid.";
		public const string ColorNotSystemColor = "The color {0} is not a system color.";

		public static string Format (string format, params object[] args)
		{
			return string.Format (format, args);
		}
	}
}

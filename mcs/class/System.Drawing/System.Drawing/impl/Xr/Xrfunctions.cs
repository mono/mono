//
// System.Drawing.XrImpl.Xrfunctions.cs
//
// (C) 2002/3 Ximian, Inc.  http://www.ximian.com
// Authors:
//    Alexandre Pigolkine <pigolkine@gmx.de>
//    Miguel de Icaza (miguel@gnome.org)
//
// TODO:
//   Need a program to generate a C program with the enumerations, run
//   it to make sure all the constants remain the same.
//
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Drawing.XrImpl {
	internal class Xr {
	
		internal enum Format {
			ARGB32 = 0,
			RGB24 = 1,
			A8 = 2,
			A1 = 4
		}

		internal enum Operator {
			Clear = 0,
			Src = 1,
			Dst = 2,
			Over = 3,
			OverReverse = 4,
			In = 5,
			InReverse = 6,
			Out = 7,
			OutReverse = 8,
			Atop = 9,
			AtopReverse = 10,
			Xor = 11,
			Add = 12,
			Saturate = 13,
			DisjointClear = 16,
			DisjointSrc = 17,
			DisjointDst = 18,
			DisjointOver = 19,
			DisjointOverReverse = 20,
			DisjointIn = 21,
			DisjointInReverse = 22,
			DisjointOut = 23,
			DisjointOutReverse = 24,
			DisjointAtop = 25,
			DisjointAtopReverse = 26,
			DisjointXor = 27,
			ConjointClear = 32,
			ConjointSrc = 33,
			ConjointDst = 34,
			ConjointOver = 35,
			ConjointOverReverse = 36,
			ConjointIn = 37,
			ConjointInReverse = 38,
			ConjointOut = 39,
			ConjointOutReverse = 40,
			ConjointAtop = 41,
			ConjointAtopReverse = 42,
			ConjointXor = 43
		}

		internal enum FillRule {
			Winding,
			EvenOdd
		}

		internal enum LineCap {
			Butt, Round, Square
		}

		internal enum LineJoin {
			Miter, Round, Bevel
		}

		internal enum Status {
			Success = 0,
			NoMemory,
			InvalidRestore,
			InvalidPopGroup,
			NoCurrentPoint,
			InvalidMatrix
		}

		internal enum Filter {
			Fast,
			Good,
			Best,
			Nearest,
			Bilinear
		}

		#region Xr
		/// <summary>
		// Xr interface
		/// </summary>
		
		const string Xrimp = "Xr";
		
		[DllImport (Xrimp)]
		internal static extern IntPtr XrCreate ();

		[DllImport (Xrimp)]
		internal static extern IntPtr XrDestroy (IntPtr xr_state);
		
		[DllImport (Xrimp)]
		internal static extern void XrSave (IntPtr xr_state);

		[DllImport (Xrimp)]
		internal static extern void XrRestore (IntPtr xr_state);
		
		[DllImport (Xrimp)]
		internal static extern void XrSetTargetSurface (IntPtr xr_state, IntPtr xr_surface);

		[DllImport (Xrimp)]
		internal static extern void XrSetTargetDrawable (IntPtr xr_state, IntPtr display, IntPtr drawable);
		
		[DllImport (Xrimp)]
		internal static extern void XrSetTargetImage (IntPtr xr_state, IntPtr bytes, Format format, int width, int height, int stride);

		[DllImport (Xrimp)]
		internal static extern void XrSetOperator (IntPtr xr_state, Operator op);

		[DllImport (Xrimp)]
		internal static extern void XrSetRGBColor (IntPtr xr_state, double red, double green, double blue);

		[DllImport (Xrimp)]
		internal static extern void XrSetPattern (IntPtr xr_state, IntPtr xr_surface_pattern);

		[DllImport (Xrimp)]
		internal static extern void XrSetTolerance (IntPtr xr_state, double tolerance);

		[DllImport (Xrimp)]
		internal static extern void XrSetAlpha (IntPtr xr_state, double alpha);

		[DllImport (Xrimp)]
		internal static extern void XrSetFillRule (IntPtr xr_state, FillRule fill_rule);
		
		[DllImport (Xrimp)]
		internal static extern void XrSetLineWidth (IntPtr xr_state, double width);

		[DllImport (Xrimp)]
		internal static extern void XrSetLineCap (IntPtr xr_state, LineCap line_cap);

		[DllImport (Xrimp)]
		internal static extern void XrSetLineJoin (IntPtr xr_state, LineJoin line_cap);

		[DllImport (Xrimp)]
		internal static extern void XrSetDash (IntPtr xr_state, double [] dashes, int ndash, double offset);

		[DllImport (Xrimp)]
		internal static extern void XrSetMiterLimit (IntPtr xr_state, double limit);
		
		[DllImport (Xrimp)]
		internal static extern void XrTranslate (IntPtr xr_state, double tx, double ty);

		[DllImport (Xrimp)]
		internal static extern void XrScale (IntPtr xr_state, double sx, double sy);
		
		[DllImport (Xrimp)]
		internal static extern void XrRotate (IntPtr xr_state, double angle);
		
		[DllImport (Xrimp)]
		internal static extern void XrConcatMatrix (IntPtr xr_state, IntPtr xr_matrix);

		[DllImport (Xrimp)]
		internal static extern void XrSetMatrix (IntPtr xr_state, IntPtr xr_matrix);

		[DllImport (Xrimp)]
		internal static extern void XrDefaultMatrix (IntPtr xr_state);

		[DllImport (Xrimp)]
		internal static extern void XrIdentityMatrix (IntPtr xr_state);
		
		[DllImport (Xrimp)]
		internal static extern void XrTransformPoint (IntPtr xr_state, ref double x, ref double y);

		[DllImport (Xrimp)]
		internal static extern void XrTransformDistance (IntPtr xr_state, ref double x, ref double y);
		
		[DllImport (Xrimp)]
		internal static extern void XrInverseTransformPoint (IntPtr xr_state, ref double x, ref double y);
		
		[DllImport (Xrimp)]
		internal static extern void XrInverseTransformDistance (IntPtr xr_state, ref double x, ref double y);

		//
		// Path functions
		//
		[DllImport (Xrimp)]
		internal static extern void XrNewPath (IntPtr xr_state);
		
		[DllImport (Xrimp)]
		internal static extern void XrMoveTo (IntPtr xr_state, double x, double y);
		
		[DllImport (Xrimp)]
		internal static extern void XrLineTo (IntPtr xr_state, double x, double y);

		[DllImport (Xrimp)]
		internal static extern void XrCurveTo (IntPtr xr_state, double x1, double y1, double x2, double y2, double x3, double y3);

		[DllImport (Xrimp)]
		internal static extern void XrRelMoveTo (IntPtr xr_state, double dx, double dy);

		[DllImport (Xrimp)]
		internal static extern void XrRelLineTo (IntPtr xr_state, double dx, double dy);

		[DllImport (Xrimp)]
		internal static extern void XrRelCurveTo (IntPtr xr_state, double dx1, double dy1, double dx2, double dy2, double dx3, double dy3);

		[DllImport (Xrimp)]
		internal static extern void XrRectangle (IntPtr xr_state, double x, double y, double width, double height);
		
		[DllImport (Xrimp)]
		internal static extern void XrClosePath (IntPtr xr_state);

		//
		// Paiting functions
		//

		[DllImport (Xrimp)]
		internal static extern void XrStroke (IntPtr xr_state);

		[DllImport (Xrimp)]
		internal static extern void XrFill (IntPtr xr_state);

		//
		// Clipping functions
		//

		[DllImport (Xrimp)]
		internal static extern void XrClip (IntPtr xr_state);

		//
		// Font/text functions
		//
		[DllImport (Xrimp, CharSet=CharSet.Ansi)]
		internal static extern void XrSelectFont (IntPtr xr_state, string key);

		[DllImport (Xrimp)]
		internal static extern void XrScaleFont (IntPtr xr_state, double scale);

		// Missing: XrTransformFont, because there is a FIXME on the header file.
		
		[DllImport (Xrimp, CharSet=CharSet.Ansi)]
		internal static extern void XrTextExtents(IntPtr xr_state, string utf8,
							  out double x, out double y,
							  out double width, out double height,
							  out double dx, out double dy);

		[DllImport (Xrimp, CharSet=CharSet.Ansi)]
		internal static extern void XrShowText (IntPtr xr_state, string utf8);		
		
		//
		// Image functions
		//
		[DllImport (Xrimp)]
		internal static extern void XrShowSurface (IntPtr xr_state, IntPtr xr_surface, int width, int height);

		//
		// Query functions
		//
		[DllImport (Xrimp)]
		internal static extern Operator XrGetOperator (IntPtr xr_state);

		[DllImport (Xrimp)]
		internal static extern double XrGetTolerance (IntPtr xr_state);

		[DllImport (Xrimp)]
		internal static extern void XrGetCurrentPoint (IntPtr xr_state, out double x, out double y);

		[DllImport (Xrimp)]
		internal static extern double XrGetLineWidth (IntPtr xr_state);

		[DllImport (Xrimp)]
		internal static extern LineCap XrGetLineCap (IntPtr xr_state);

		[DllImport (Xrimp)]
		internal static extern LineJoin XrGetLineJoin (IntPtr xr_state);

		[DllImport (Xrimp)]
		internal static extern double XrGetMiterLimit (IntPtr xr_state);

#if false
		//
		// This is not implemented in Xr
		//
		[DllImport (Xrimp)]
		internal static extern void XrGetMatrix (IntPtr xr_state,
							 out double a, out double b, out double c, out double d,
							 out double tx, out double ty);
#endif

		[DllImport (Xrimp)]
		internal static extern double XrGetTargetSurface (IntPtr xr_state);

		[DllImport (Xrimp)]
		internal static extern Status XrGetStatus (IntPtr xr_state);

		[DllImport (Xrimp)]  // IntPtr, because it is a const string
		internal static extern IntPtr XrGetStatusString (IntPtr xr_state);


		//
		// Surface
		//
		[DllImport (Xrimp)]
		internal static extern IntPtr XrSurfaceCreateForDrawable (
			IntPtr display, IntPtr drawable, IntPtr visual, Format format, IntPtr colormap);

		[DllImport (Xrimp)]
		internal static extern IntPtr XrSurfaceCreateForImage (IntPtr data, Format format, int width, int height, int stride);

		[DllImport (Xrimp)]
		internal static extern IntPtr XrSurfaceCreateNextTo (IntPtr xr_surface_neighbor, Format format, int width, int height);

		[DllImport (Xrimp)]
		internal static extern IntPtr XrSurfaceCreateNextToSolid (
			IntPtr xr_surface_neighbor, Format format, int width, int height,
                            double      red, double green, double blue, double alpha);

		[DllImport (Xrimp)]
		internal static extern void XrSurfaceDestroy (IntPtr xr_surface);

		[DllImport (Xrimp)]
		internal static extern Status XrSurfaceSetRepeat (IntPtr xr_surface, int repeat);

		[DllImport (Xrimp)]
		internal static extern Status XrSurfaceSetMatrix(IntPtr xr_surface, IntPtr xr_matrix);

		[DllImport (Xrimp)]
		internal static extern Status XrSurfaceGetMatrix (IntPtr xr_surface, IntPtr xr_matrix);
		
		[DllImport (Xrimp)]
		internal static extern Status XrSurfaceSetFilter (IntPtr xr_surface, Filter filter);

		//
		// Matrix functions
		//
		[DllImport (Xrimp)]
		internal static extern Status XrMatrixCreate ();

		[DllImport (Xrimp)]
		internal static extern Status XrMatrixDestroy (IntPtr xr_matrix);

		[DllImport (Xrimp)]
		internal static extern Status XrMatrixCopy (IntPtr xr_matrix, IntPtr xr_matrix_other);

		[DllImport (Xrimp)]
		internal static extern Status XrMatrixSetIdentity (IntPtr xr_matrix);

		[DllImport (Xrimp)]
		internal static extern Status XrMatrixSetAffine (IntPtr xr_matrix, 
								 double a, double b,
								 double c, double d,
								 double tx, double ty);
		
		[DllImport (Xrimp)]
		internal static extern Status XrMatrixTranslate (IntPtr xr_matrix, double tx, double ty);

		[DllImport (Xrimp)]
		internal static extern Status XrMatrixScale (IntPtr xr_matrix, double sx, double sy);

		[DllImport (Xrimp)]
		internal static extern Status XrMatrixRotate (IntPtr xr_matrix, double radians);

		[DllImport (Xrimp)]
		internal static extern Status XrMatrixInvert(IntPtr xr_matrix);

		[DllImport (Xrimp)]
		internal static extern Status XrMatrixMultiply (IntPtr xr_matrix_result, IntPtr matrix_a, IntPtr matrix_b);

		[DllImport (Xrimp)]
		internal static extern Status XrMatrixTransformDistance (IntPtr xr_matrix, ref double dx, ref double dy);

		[DllImport (Xrimp)]
		internal static extern Status XrMatrixTransformPoint (IntPtr xr_matrix, ref double x, ref double y);

		#endregion
	}
}

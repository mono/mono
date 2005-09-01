//
// Mono.Cairo.Matrix.cs
//
// Author: Duncan Mak
//         Hisham Mardam Bey (hisham.mardambey@gmail.com)
// (C) Ximian Inc, 2003 - 2005.
//
// This is an OO wrapper API for the Cairo API
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
using System.Runtime.InteropServices;

namespace Cairo {
   
                  
   [StructLayout(LayoutKind.Sequential)]
   internal struct Matrix_T
   {
	   public double xx; 
	   public double yx;
	   public double xy; 
	   public double yy;
	   public double x0; 
	   public double y0;	   
   }
   
   
   
        public class Matrix
        {		
		internal Matrix_T matrix;
		
                public Matrix ()       
                {               
			//CreateIdentify();
                }
		
                internal Matrix (Matrix_T ptr)
                {
                        //if (ptr == null)
			//  CreateIdentify ();
			
                        matrix = ptr;
                }
		
                public void CreateIdentify ()
                {			
			CairoAPI.cairo_matrix_init_identity (ref matrix);
                }
		
		public void Init (double xx, double yx, double xy, double yy,
				  double x0, double y0)
		{
			matrix.xx = xx; matrix.yx = yx; matrix.xy = xy;
			matrix.yy = yy; matrix.x0 = x0; matrix.y0 = y0;
		}
		
		public void InitTranslate (double tx, double ty)
		{		
			CairoAPI.cairo_matrix_init_translate (ref matrix, tx, ty);
		}		
		  			       
		public void Translate (double tx, double ty)
		{
			CairoAPI.cairo_matrix_translate (ref matrix, tx, ty);
		}
		
                public void InitScale (double sx, double sy)
                {
			CairoAPI.cairo_matrix_init_scale (ref matrix, sx, sy);
                }		
		
                public void Scale (double sx, double sy)
                {
			CairoAPI.cairo_matrix_scale (ref matrix, sx, sy);
                }

                public void InitRotate (double radians)
                {
			CairoAPI.cairo_matrix_init_rotate (ref matrix, radians);
                }		
		
                public void Rotate (double radians)
                {
			CairoAPI.cairo_matrix_rotate (ref matrix, radians);
                }

                public Cairo.Status Invert ()
                {
			return  CairoAPI.cairo_matrix_invert (ref matrix);
                }


                public static void Multiply (ref Cairo.Matrix res, 
					 ref Cairo.Matrix a, ref Cairo.Matrix b)
                {	
			if (res == null)
			  res = new Matrix ();
						
                        CairoAPI.cairo_matrix_multiply (ref res.matrix, 
							ref a.matrix, 
							ref b.matrix);
                }
		
                public void TransformDistance (ref double dx, ref double dy)
                {
                        CairoAPI.cairo_matrix_transform_distance (ref matrix, ref dx, ref dy);
                }

                public void TransformPoint (ref double x, ref double y)
                {
                        CairoAPI.cairo_matrix_transform_point (ref matrix, ref x, ref y);
		}
		
                internal Matrix_T Pointer {
                        get { return matrix; }
			set { matrix = value; }
                }
		
		public IntPtr Raw {
			get {
				IntPtr p = Marshal.AllocCoTaskMem ( Marshal.SizeOf (matrix));
				Marshal.StructureToPtr (matrix, p, true);
				return p;
			}
		}
				
        }
}

/*
 * BSD Licence:
 * Copyright (c) 2001, 2002 Ben Houston [ ben@exocortex.org ]
 * Exocortex Technologies [ www.exocortex.org ]
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without 
 * modification, are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright 
 * notice, this list of conditions and the following disclaimer in the 
 * documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the <ORGANIZATION> nor the names of its contributors
 * may be used to endorse or promote products derived from this software
 * without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE REGENTS OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
 * DAMAGE.
 */

using System;
using System.Diagnostics;


namespace Exocortex.DSP {

	// Comments? Questions? Bugs? Tell Ben Houston at ben@exocortex.org
	// Version: May 4, 2002

	/// <summary>
	/// <p>Various mathematical functions for complex numbers.</p>
	/// </summary>
	public class ComplexMath {
		
		//---------------------------------------------------------------------------------------------------

		private ComplexMath() {
		}

		//---------------------------------------------------------------------------------------------------

		/// <summary>
		/// Swap two complex numbers
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		static public void Swap( ref Complex a, ref Complex b ) {
			Complex temp = a;
			a = b;
			b = temp;
		}

		/// <summary>
		/// Swap two complex numbers
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		static public void Swap( ref ComplexF a, ref ComplexF b ) {
			ComplexF temp = a;
			a = b;
			b = temp;
		}
		
		//---------------------------------------------------------------------------------------------------

		static private double	_halfOfRoot2	= 0.5 * Math.Sqrt( 2 );

		/// <summary>
		/// Calculate the square root of a complex number
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		static public ComplexF	Sqrt( ComplexF c ) {
			double	x	= c.Re;
			double	y	= c.Im;

			double	modulus	= Math.Sqrt( x*x + y*y );
			int		sign	= ( y < 0 ) ? -1 : 1;

			c.Re		= (float)( _halfOfRoot2 * Math.Sqrt( modulus + x ) );
			c.Im	= (float)( _halfOfRoot2 * sign * Math.Sqrt( modulus - x ) );

			return	c;
		}

		/// <summary>
		/// Calculate the square root of a complex number
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		static public Complex	Sqrt( Complex c ) {
			double	x	= c.Re;
			double	y	= c.Im;

			double	modulus	= Math.Sqrt( x*x + y*y );
			int		sign	= ( y < 0 ) ? -1 : 1;

			c.Re		= (double)( _halfOfRoot2 * Math.Sqrt( modulus + x ) );
			c.Im	= (double)( _halfOfRoot2 * sign * Math.Sqrt( modulus - x ) );

			return	c;
		}

		//---------------------------------------------------------------------------------------------------

		/// <summary>
		/// Calculate the power of a complex number
		/// </summary>
		/// <param name="c"></param>
		/// <param name="exponent"></param>
		/// <returns></returns>
		static public ComplexF	Pow( ComplexF c, double exponent ) {
			double	x	= c.Re;
			double	y	= c.Im;
			
			double	modulus		= Math.Pow( x*x + y*y, exponent * 0.5 );
			double	argument	= Math.Atan2( y, x ) * exponent;

			c.Re		= (float)( modulus * System.Math.Cos( argument ) );
			c.Im = (float)( modulus * System.Math.Sin( argument ) );

			return	c;
		}

		/// <summary>
		/// Calculate the power of a complex number
		/// </summary>
		/// <param name="c"></param>
		/// <param name="exponent"></param>
		/// <returns></returns>
		static public Complex	Pow( Complex c, double exponent ) {
			double	x	= c.Re;
			double	y	= c.Im;
			
			double	modulus		= Math.Pow( x*x + y*y, exponent * 0.5 );
			double	argument	= Math.Atan2( y, x ) * exponent;

			c.Re		= (double)( modulus * System.Math.Cos( argument ) );
			c.Im = (double)( modulus * System.Math.Sin( argument ) );

			return	c;
		}
		
		//---------------------------------------------------------------------------------------------------

	}
}

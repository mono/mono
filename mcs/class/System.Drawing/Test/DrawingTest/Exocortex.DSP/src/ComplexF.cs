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
using System.Runtime.InteropServices;


namespace Exocortex.DSP {

	// Comments? Questions? Bugs? Tell Ben Houston at ben@exocortex.org
	// Version: May 4, 2002

	/// <summary>
	/// <p>A single-precision complex number representation.</p>
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct ComplexF : IComparable, ICloneable {

		//-----------------------------------------------------------------------------------
		//-----------------------------------------------------------------------------------

		/// <summary>
		/// The real component of the complex number
		/// </summary>
		public float Re;

		/// <summary>
		/// The imaginary component of the complex number
		/// </summary>
		public float Im;

		//-----------------------------------------------------------------------------------
		//-----------------------------------------------------------------------------------

		/// <summary>
		/// Create a complex number from a real and an imaginary component
		/// </summary>
		/// <param name="real"></param>
		/// <param name="imaginary"></param>
		public ComplexF( float real, float imaginary ) {
			this.Re		= (float) real;
			this.Im	= (float) imaginary;
		}

		/// <summary>
		/// Create a complex number based on an existing complex number
		/// </summary>
		/// <param name="c"></param>
		public ComplexF( ComplexF c ) {
			this.Re		= c.Re;
			this.Im	= c.Im;
		}

		/// <summary>
		/// Create a complex number from a real and an imaginary component
		/// </summary>
		/// <param name="real"></param>
		/// <param name="imaginary"></param>
		/// <returns></returns>
		static public ComplexF	FromRealImaginary( float real, float imaginary ) {
			ComplexF c;
			c.Re		= (float) real;
			c.Im = (float) imaginary;
			return c;
		}

		/// <summary>
		/// Create a complex number from a modulus (length) and an argument (radian)
		/// </summary>
		/// <param name="modulus"></param>
		/// <param name="argument"></param>
		/// <returns></returns>
		static public ComplexF	FromModulusArgument( float modulus, float argument ) {
			ComplexF c;
			c.Re		= (float)( modulus * System.Math.Cos( argument ) );
			c.Im	= (float)( modulus * System.Math.Sin( argument ) );
			return c;
		}
		
		//-----------------------------------------------------------------------------------
		//-----------------------------------------------------------------------------------

		object	ICloneable.Clone() {
			return	new ComplexF( this );
		}
		/// <summary>
		/// Clone the complex number
		/// </summary>
		/// <returns></returns>
		public ComplexF	Clone() {
			return	new ComplexF( this );
		}
		
		//-----------------------------------------------------------------------------------
		//-----------------------------------------------------------------------------------

		/// <summary>
		/// The modulus (length) of the complex number
		/// </summary>
		/// <returns></returns>
		public float	GetModulus() {
			float	x	= this.Re;
			float	y	= this.Im;
			return	(float) Math.Sqrt( x*x + y*y );
		}

		/// <summary>
		/// The squared modulus (length^2) of the complex number
		/// </summary>
		/// <returns></returns>
		public float	GetModulusSquared() {
			float	x	= this.Re;
			float	y	= this.Im;
			return	(float) x*x + y*y;
		}

		/// <summary>
		/// The argument (radians) of the complex number
		/// </summary>
		/// <returns></returns>
		public float	GetArgument() {
			return (float) Math.Atan2( this.Im, this.Re );
		}

		//-----------------------------------------------------------------------------------

		/// <summary>
		/// Get the conjugate of the complex number
		/// </summary>
		/// <returns></returns>
		public ComplexF GetConjugate() {
			return FromRealImaginary( this.Re, -this.Im );
		}

		//-----------------------------------------------------------------------------------

		/// <summary>
		/// Scale the complex number to 1.
		/// </summary>
		public void Normalize() {
			double	modulus = this.GetModulus();
			if( modulus == 0 ) {
				throw new DivideByZeroException( "Can not normalize a complex number that is zero." );
			}
			this.Re	= (float)( this.Re / modulus );
			this.Im	= (float)( this.Im / modulus );
		}

		//-----------------------------------------------------------------------------------
		//-----------------------------------------------------------------------------------

		/// <summary>
		/// Convert to a from double precision complex number to a single precison complex number
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public static explicit operator ComplexF ( Complex c ) {
			ComplexF cF;
			cF.Re	= (float) c.Re;
			cF.Im	= (float) c.Im;
			return cF;
		}
		
		/// <summary>
		/// Convert from a single precision real number to a complex number
		/// </summary>
		/// <param name="f"></param>
		/// <returns></returns>
		public static explicit operator ComplexF ( float f ) {
			ComplexF c;
			c.Re		= (float) f;
			c.Im	= (float) 0;
			return c;
		}

		/// <summary>
		/// Convert from a single precision complex to a real number
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public static explicit operator float ( ComplexF c ) {
			return (float) c.Re;
		}
		
		//-----------------------------------------------------------------------------------
		//-----------------------------------------------------------------------------------

		/// <summary>
		/// Are these two complex numbers equivalent?
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool	operator==( ComplexF a, ComplexF b ) {
			return	( a.Re == b.Re ) && ( a.Im == b.Im );
		}

		/// <summary>
		/// Are these two complex numbers different?
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool	operator!=( ComplexF a, ComplexF b ) {
			return	( a.Re != b.Re ) || ( a.Im != b.Im );
		}

		/// <summary>
		/// Get the hash code of the complex number
		/// </summary>
		/// <returns></returns>
		public override int		GetHashCode() {
			return	( this.Re.GetHashCode() ^ this.Im.GetHashCode() );
		}

		/// <summary>
		/// Is this complex number equivalent to another object?
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>
		public override bool	Equals( object o ) {
			if( o is ComplexF ) {
				ComplexF c = (ComplexF) o;
				return   ( this == c );
			}
			return	false;
		}

		//-----------------------------------------------------------------------------------
		//-----------------------------------------------------------------------------------

		/// <summary>
		/// Compare to other complex numbers or real numbers
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>
		public int	CompareTo( object o ) {
			if( o == null ) {
				return 1;  // null sorts before current
			}
			if( o is ComplexF ) {
				return	this.GetModulus().CompareTo( ((ComplexF)o).GetModulus() );
			}
			if( o is float ) {
				return	this.GetModulus().CompareTo( (float)o );
			}
			if( o is Complex ) {
				return	this.GetModulus().CompareTo( ((Complex)o).GetModulus() );
			}
			if( o is double ) {
				return	this.GetModulus().CompareTo( (double)o );
			}
			throw new ArgumentException();
		}

		//-----------------------------------------------------------------------------------
		//-----------------------------------------------------------------------------------

		/// <summary>
		/// This operator doesn't do much. :-)
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public static ComplexF operator+( ComplexF a ) {
			return a;
		}

		/// <summary>
		/// Negate the complex number
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public static ComplexF operator-( ComplexF a ) {
			a.Re	= -a.Re;
			a.Im	= -a.Im;
			return a;
		}

		/// <summary>
		/// Add a complex number to a real
		/// </summary>
		/// <param name="a"></param>
		/// <param name="f"></param>
		/// <returns></returns>
		public static ComplexF operator+( ComplexF a, float f ) {
			a.Re	= (float)( a.Re + f );
			return a;
		}

		/// <summary>
		/// Add a real to a complex number
		/// </summary>
		/// <param name="f"></param>
		/// <param name="a"></param>
		/// <returns></returns>
		public static ComplexF operator+( float f, ComplexF a ) {
			a.Re	= (float)( a.Re + f );
			return a;
		}

		/// <summary>
		/// Add to complex numbers
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static ComplexF operator+( ComplexF a, ComplexF b ) {
			a.Re	= a.Re + b.Re;
			a.Im	= a.Im + b.Im;
			return a;
		}

		/// <summary>
		/// Subtract a real from a complex number
		/// </summary>
		/// <param name="a"></param>
		/// <param name="f"></param>
		/// <returns></returns>
		public static ComplexF operator-( ComplexF a, float f ) {
			a.Re	= (float)( a.Re - f );
			return a;
		}

		/// <summary>
		/// Subtract a complex number from a real
		/// </summary>
		/// <param name="f"></param>
		/// <param name="a"></param>
		/// <returns></returns>
		public static ComplexF operator-( float f, ComplexF a ) {
			a.Re	= (float)( f - a.Re );
			a.Im	= (float)( 0 - a.Im );
			return a;
		}

		/// <summary>
		/// Subtract two complex numbers
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static ComplexF operator-( ComplexF a, ComplexF b ) {
			a.Re	= a.Re - b.Re;
			a.Im	= a.Im - b.Im;
			return a;
		}

		/// <summary>
		/// Multiply a complex number by a real
		/// </summary>
		/// <param name="a"></param>
		/// <param name="f"></param>
		/// <returns></returns>
		public static ComplexF operator*( ComplexF a, float f ) {
			a.Re	= (float)( a.Re * f );
			a.Im	= (float)( a.Im * f );
			return a;
		}
		
		/// <summary>
		/// Multiply a real by a complex number
		/// </summary>
		/// <param name="f"></param>
		/// <param name="a"></param>
		/// <returns></returns>
		public static ComplexF operator*( float f, ComplexF a ) {
			a.Re	= (float)( a.Re * f );
			a.Im	= (float)( a.Im * f );
			return a;
		}
		
		/// <summary>
		/// Multiply two complex numbers together
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static ComplexF operator*( ComplexF a, ComplexF b ) {
			// (x + yi)(u + vi) = (xu – yv) + (xv + yu)i. 
			double	x = a.Re, y = a.Im;
			double	u = b.Re, v = b.Im;
			a.Re	= (float)( x*u - y*v );
			a.Im	= (float)( x*v + y*u );
			return a;
		}

		/// <summary>
		/// Divide a complex number by a real number
		/// </summary>
		/// <param name="a"></param>
		/// <param name="f"></param>
		/// <returns></returns>
		public static ComplexF operator/( ComplexF a, float f ) {
			if( f == 0 ) {
				throw new DivideByZeroException();
			}
			a.Re	= (float)( a.Re / f );
			a.Im	= (float)( a.Im / f );
			return a;
		}
		
		/// <summary>
		/// Divide a complex number by a complex number
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static ComplexF operator/( ComplexF a, ComplexF b ) {
			double	x = a.Re,	y = a.Im;
			double	u = b.Re,	v = b.Im;
			double	denom = u*u + v*v;

			if( denom == 0 ) {
				throw new DivideByZeroException();
			}
			a.Re	= (float)( ( x*u + y*v ) / denom );
			a.Im	= (float)( ( y*u - x*v ) / denom );
			return a;
		}

		/// <summary>
		/// Parse a complex representation in this fashion: "( %f, %f )"
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		static public ComplexF Parse( string s ) {
			throw new NotImplementedException( "ComplexF ComplexF.Parse( string s ) is not implemented." );
		}
		
		/// <summary>
		/// Get the string representation
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			return	String.Format( "( {0}, {1}i )", this.Re, this.Im );
		}

		//-----------------------------------------------------------------------------------
		//-----------------------------------------------------------------------------------

		/// <summary>
		/// Determine whether two complex numbers are almost (i.e. within the tolerance) equivalent.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="tolerance"></param>
		/// <returns></returns>
		static public bool IsEqual( ComplexF a, ComplexF b, float tolerance ) {
			return
				( Math.Abs( a.Re - b.Re ) < tolerance ) &&
				( Math.Abs( a.Im - b.Im ) < tolerance );

		}
		
		//----------------------------------------------------------------------------------
		//----------------------------------------------------------------------------------

		/// <summary>
		/// Represents zero
		/// </summary>
		static public ComplexF	Zero {
			get	{	return	new ComplexF( 0, 0 );	}
		}

		/// <summary>
		/// Represents the result of sqrt( -1 )
		/// </summary>
		static public ComplexF	I {
			get {	return	new ComplexF( 0, 1 );	}
		}

		/// <summary>
		/// Represents the largest possible value of ComplexF.
		/// </summary>
		static public ComplexF	MaxValue {
			get {	return	new ComplexF( float.MaxValue, float.MaxValue );	}
		}

		/// <summary>
		/// Represents the smallest possible value of ComplexF.
		/// </summary>
		static public ComplexF	MinValue {
			get {	return	new ComplexF( float.MinValue, float.MinValue );	}
		}


		//----------------------------------------------------------------------------------
		//----------------------------------------------------------------------------------
	}

}

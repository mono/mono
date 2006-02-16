/*
 Copyright (c) 2003-2006 Niels Kokholm and Peter Sestoft
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
*/

using System;
using MSG = System.Collections.Generic;

namespace Try
{
/*
	/// <summary>
	/// Affenpinscher 
	/// </summary>
	public class SSS<T,U>
	{
		/// <summary>
		/// GGGGGGG <cell>tidlig <seealso cref="T:Try.Foo!3"/> <b alk="foo">bonny</b> namse</cell> efter
		/// </summary>
		/// <param name="u">zeU</param>
		/// <param name="cell">zeI</param>
		/// <param name="t">zeT</param>
		/// <returns>zeReturn</returns>
		public static T g(out U u, ref int cell, T t) { u = default(U); return default(T); }
		
		/// <summary>
		/// Nested klasse
		/// </summary>
		public class Inner<K>
		{
			/// <summary>
			/// IIIIIIIIII
			/// </summary>
			/// <param name="t">TTTTTTTTTT</param>
			/// <param name="k">KKKKKKK</param>
			/// <returns>RRRRRRRRRRR</returns>
			public U indre(T t, K k) { return default(U); }
		}
        /// <summary>
        /// Jaj ja
        /// </summary>
        public class NongenNested
        {
            /// <summary>
            /// Det er et y
            /// </summary>
            public int y;
        }
        /// <summary>
		/// Lolololola
		/// </summary>
		public U lefield;
		/// <summary>
		/// Cococococola
		/// </summary>
		public static Foo<T,U,U> gramse;
		/// <summary>
		/// zesum
		/// </summary>
		/// <value>zeval</value>
		public SSS<T,T> leprop { get { return default(SSS<T,T>); } }
		/// <summary>
		/// Sju
		/// </summary>
		/// <param name="f">ffffffff</param>
		/// <param name="johndoe">ffffffff</param>
		public string s<V,X,Y,Z>(Foo<T,Y,SSS<Z,T>> f, int johndoe) { return "7"; }
		/// <summary>
		/// intuism
		/// </summary>
		/// <param name="tudse">froe</param>
		/// <param name="frank">sin</param>
		public SSS(T[] tudse, double frank) { }
	}

	/// <summary>
	/// ooh 
	/// </summary>

	public class HHH
	{
		/// <summary>
		/// MusseKom
		/// </summary>
		public class Musse { }
	}



	/// <summary>
	/// Hejsa 
	/// </summary>
	public class Foo<K,L,M> { }

*/
	/// <summary>
	/// Hejsa
	/// </summary>
	public delegate void Mapper<T,U>(T t);



	/// <summary>
	/// Los accoutables
	/// </summary>
	public interface IEnumerable<T>
	{
		// <summary>
		// getegetegeteg
		// </summary>
		// <returns>leturn</returns>
		//MSG.IEnumerator<T> GetEnumerator();


		/// <summary>
		/// Apply a delegate to all items of this collection.
		/// </summary>
		/// <param name="a">The delegate to apply</param>
		void Map<U>(Mapper<T,U> a);
	}
}

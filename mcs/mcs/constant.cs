//
// constant.cs: Constants.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//
//

namespace Mono.CSharp {

	using System;

	/// <summary>
	///   Base class for constants and literals.
	/// </summary>
	public abstract class Constant : Expression {

		protected Constant ()
		{
			eclass = ExprClass.Value;
		}

		/// <remarks>
		///   This is different from ToString in that ToString
		///   is supposed to be there for debugging purposes,
		///   and is not guaranteed to be useful for anything else,
		///   AsString() will provide something that can be used
		///   for round-tripping C# code.  Maybe it can be used
		///   for IL assembly as well.
		/// </remarks>
		public abstract string AsString ();

		override public string ToString ()
		{
			return AsString ();
		}

		/// <summary>
		///  This is used to obtain the actual value of the literal
		///  cast into an object.
		/// </summary>
		public abstract object GetValue ();
	}
}



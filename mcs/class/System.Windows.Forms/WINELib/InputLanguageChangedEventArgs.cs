//
// System.Windows.Forms.InputLanguageChangeEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Partial Completed by Dennis Hayes (dennish@raytek.com)
//  Giananadrea Terzi (gianandrea.terzi@lario.com)
//
// (C) 2002 Ximian, Inc
//
using System.Globalization;

namespace System.Windows.Forms {

	// <summary>
	// Template. Needs completition on CharSet property
	// </summary>

	public class InputLanguageChangedEventArgs : EventArgs {

		#region Fields
		private CultureInfo culture;
		private byte b;
		#endregion

		//
		//  --- Constructor
		//
		public InputLanguageChangedEventArgs ( CultureInfo culture, byte b) {
			this.culture = culture;
			this.b = b;
		}

		#region Public Properties
		[MonoTODO]
		public byte CharSet 
		{
			get {
				throw new NotImplementedException ();
			}
		}
		public CultureInfo Culture {
			get {
				return culture;
			}
		}
		[MonoTODO]
		public InputLanguage InputLanguage {
			get {
				throw new NotImplementedException ();
			}
		}
		#endregion

		#region Public Methods

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two InputLanguageChangedEventArgs objects.
		///	The return value is based on the equivalence of
		///	CharSet, Culture and InputLanguage Property
		///	of the two InputLanguageChangedEventArgs.
		/// </remarks>
		public static bool operator == (InputLanguageChangedEventArgs InputLanguageChangedEventArgsA, InputLanguageChangedEventArgs InputLanguageChangedEventArgsB) 
		{
			return (InputLanguageChangedEventArgsA.CharSet == InputLanguageChangedEventArgsB.CharSet) && 
				   (InputLanguageChangedEventArgsA.Culture == InputLanguageChangedEventArgsB.Culture) && 
				   (InputLanguageChangedEventArgsA.InputLanguage == InputLanguageChangedEventArgsB.InputLanguage);

		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two InputLanguageChangedEventArgs objects.
		///	The return value is based on the equivalence of
		///	CharSet, Culture and InputLanguage Property
		///	of the two InputLanguageChangedEventArgs.
		/// </remarks>
		public static bool operator != (InputLanguageChangedEventArgs InputLanguageChangedEventArgsA, InputLanguageChangedEventArgs InputLanguageChangedEventArgsB) 
		{
			return (InputLanguageChangedEventArgsA.CharSet != InputLanguageChangedEventArgsB.CharSet) || 
				(InputLanguageChangedEventArgsA.Culture != InputLanguageChangedEventArgsB.Culture) || 
				(InputLanguageChangedEventArgsA.InputLanguage != InputLanguageChangedEventArgsB.InputLanguage);

		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this
		///	InputLanguageChangedEventArgs and another
		///	object.
		/// </remarks>
		public override bool Equals (object obj) 
		{
			if (!(obj is InputLanguageChangedEventArgs))return false;
			return (this == (InputLanguageChangedEventArgs) obj);
		}

		/// <summary>
		///	GetHashCode Method
		/// </summary>
		///
		/// <remarks>
		///	Calculates a hashing value.
		/// </remarks>
		[MonoTODO]
		public override int GetHashCode () 
		{
			//FIXME: add class specific stuff;
			return base.GetHashCode();
		}

		/// <summary>
		///	ToString Method
		/// </summary>
		///
		/// <remarks>
		///	Formats the object as a string.
		/// </remarks>
		[MonoTODO]
		public override string ToString () 
		{
			//FIXME: add class specific stuff;
			return base.ToString() + " InputLanguageChangedEventArgs";
		}


		#endregion

	}
}

//
// System.Windows.Forms.InputLanguageChangingEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	 Partially completed by Dennis Hayes (dennish@raytek.com)
//   Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) 2002 Ximian, Inc
//
using System.Globalization;
using System.ComponentModel;

namespace System.Windows.Forms {

	// <summary>
	// </summary>

	public class InputLanguageChangingEventArgs : EventArgs {

		#region Fields
		private CultureInfo culture;
		private bool systemcharset;
		private InputLanguage inputlanguage;
		#endregion

		//
		//  --- Constructor
		//
		[MonoTODO] //what about input lang?
		public InputLanguageChangingEventArgs(CultureInfo culture, bool sysCharSet) {
			this.culture = culture;
			this.systemcharset =sysCharSet;
		}

		[MonoTODO] //what about culture?
		public InputLanguageChangingEventArgs(InputLanguage inputlanguage, bool sysCharSet) {
			this.culture = culture;
			this.inputlanguage = inputlanguage;
		}

		#region Public Properties
		public CultureInfo Culture 
		{
			get {
				return culture;
			}
		}
		public InputLanguage InputLanguage {
			get {
				return inputlanguage;
			}
		}
		public bool SysCharSet {
			get {
				return systemcharset;
			}
		}
		#endregion

		#region Public Methods

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two InputLanguageChangingEventArgs objects.
		///	The return value is based on the equivalence of
		///	Culture, InputLanguage and SysCharSet Property
		///	of the two InputLanguageChangingEventArgs.
		/// </remarks>
		public static bool operator == (InputLanguageChangingEventArgs InputLanguageChangingEventArgsA, InputLanguageChangingEventArgs InputLanguageChangingEventArgsB) 
		{
			return (InputLanguageChangingEventArgsA.Culture == InputLanguageChangingEventArgsB.Culture) && 
				   (InputLanguageChangingEventArgsA.InputLanguage == InputLanguageChangingEventArgsB.InputLanguage) && 
				   (InputLanguageChangingEventArgsA.SysCharSet == InputLanguageChangingEventArgsB.SysCharSet);

		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two InputLanguageChangingEventArgs objects.
		///	The return value is based on the equivalence of
		///	Culture, InputLanguage and SysCharSet Property
		///	of the two InputLanguageChangingEventArgs.
		/// </remarks>
		public static bool operator != (InputLanguageChangingEventArgs InputLanguageChangingEventArgsA, InputLanguageChangingEventArgs InputLanguageChangingEventArgsB) 
		{
			return (InputLanguageChangingEventArgsA.Culture != InputLanguageChangingEventArgsB.Culture) || 
				(InputLanguageChangingEventArgsA.InputLanguage != InputLanguageChangingEventArgsB.InputLanguage) || 
				(InputLanguageChangingEventArgsA.SysCharSet != InputLanguageChangingEventArgsB.SysCharSet);

		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this
		///	InputLanguageChangingEventArgs and another
		///	object.
		/// </remarks>
		public override bool Equals (object obj) 
		{
			if (!(obj is InputLanguageChangingEventArgs))return false;
			return (this == (InputLanguageChangingEventArgs) obj);
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
			return base.ToString() + " InputLanguageChangingEventArgs";
		}


		#endregion

	}
}

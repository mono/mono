//
// System.Windows.Forms.ConvertEventArgs.cs
//
// Author:
//  Stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	Finished by Dennis Hayes (dennish@raytek.com)
//  Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) Ximian, Inc., 2002
//

namespace System.Windows.Forms {

	/// <summary>
	/// Provides data for the Format and Parse events.
	/// </summary>

	public class ConvertEventArgs : EventArgs {

		#region Fields

		private Type desiredtype;
		private object objectvalue;

		#endregion
		
		//Constructor
		public ConvertEventArgs(object objectValue,Type desiredType) 
		{
			this.desiredtype = desiredType;
			this.objectvalue = objectValue;
		}
		
		#region Public Properties
		public Type DesiredType 
		{
			get { 
					return desiredtype; 
				}
		}
		
		public object Value {
			get { 
				return objectvalue; 
			}
			set {
				objectvalue = value; 
			}
		}
		#endregion

		#region Public Methods

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two ConvertEventArgs objects.
		///	The return value is based on the equivalence of
		///	DesiredType and Value Property
		///	of the two ContentsResizedEventArgs.
		/// </remarks>
		public static bool operator == (ConvertEventArgs ConvertEventArgsA, ConvertEventArgs ConvertEventArgsB) 
		{
			return (ConvertEventArgsA.DesiredType == ConvertEventArgsB.DesiredType) && 
				   (ConvertEventArgsA.Value == ConvertEventArgsB.Value);

		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two ConvertEventArgs objects.
		///	The return value is based on the equivalence of
		///	DesiredType and Value Property
		///	of the two ContentsResizedEventArgs.
		/// </remarks>
		public static bool operator != (ConvertEventArgs ConvertEventArgsA, ConvertEventArgs ConvertEventArgsB) 
		{
			return (ConvertEventArgsA.DesiredType != ConvertEventArgsB.DesiredType) || 
				   (ConvertEventArgsA.Value != ConvertEventArgsB.Value);
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this
		///	ConvertEventArgs and another
		///	object.
		/// </remarks>
		public override bool Equals (object obj) 
		{
			if (!(obj is ConvertEventArgs))return false;
			return (this == (ConvertEventArgs) obj);
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
			return base.ToString() + " ConvertEventArgs";
		}


		#endregion

	}
}

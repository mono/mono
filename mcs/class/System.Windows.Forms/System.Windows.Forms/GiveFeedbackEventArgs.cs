//
// System.Windows.Forms.GiveFeedbackEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	 Partialy completed by Dennis Hayes (dennish@raytek.com)
//   Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	/// <summary>
	/// Completed
	/// </summary>

	public class GiveFeedbackEventArgs : EventArgs {

		#region Fields
		DragDropEffects effect;
		bool useDefaultCursors;
		#endregion

		public GiveFeedbackEventArgs(  DragDropEffects effect,  bool useDefaultCursors ) 
		{
			this.effect = effect;
			this.useDefaultCursors = useDefaultCursors;
		}
		
		#region Public Properties
		public DragDropEffects Effect 
		{
			get {
				return effect;
			}
		}
		public bool UseDefaultCursors {
			get {
				return useDefaultCursors;
			}
			set {
				useDefaultCursors = value;
			}
		}
		#endregion

		#region Public Methods


		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two GiveFeedbackEventArgs objects.
		///	The return value is based on the equivalence of
		///	Effect and UseDefaultCursors Property
		///	of the two GiveFeedbackEventArgs.
		/// </remarks>
		public static bool operator == (GiveFeedbackEventArgs GiveFeedbackEventArgsA, GiveFeedbackEventArgs GiveFeedbackEventArgsB) 
		{
			return (GiveFeedbackEventArgsA.Effect == GiveFeedbackEventArgsB.Effect) && 
				   (GiveFeedbackEventArgsA.UseDefaultCursors == GiveFeedbackEventArgsB.UseDefaultCursors);

		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two GiveFeedbackEventArgs objects.
		///	The return value is based on the equivalence of
		///	Effect and UseDefaultCursors Property
		///	of the two GiveFeedbackEventArgs.
		/// </remarks>
		public static bool operator != (GiveFeedbackEventArgs GiveFeedbackEventArgsA, GiveFeedbackEventArgs GiveFeedbackEventArgsB) 
		{
			return (GiveFeedbackEventArgsA.Effect != GiveFeedbackEventArgsB.Effect) ||
				(GiveFeedbackEventArgsA.UseDefaultCursors != GiveFeedbackEventArgsB.UseDefaultCursors);

		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this
		///	GiveFeedbackEventArgs and another
		///	object.
		/// </remarks>
		public override bool Equals (object obj) 
		{
			if (!(obj is GiveFeedbackEventArgs))return false;
			return (this == (GiveFeedbackEventArgs) obj);
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
			return base.ToString() + " GiveFeedbackEventArgs";
		}

		#endregion

	}
}

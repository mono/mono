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
	}
}

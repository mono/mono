//
// System.Windows.Forms.GiveFeedbackEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Partialy completed by Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	/// <summary>
	///
	/// </summary>

    public class GiveFeedbackEventArgs : EventArgs {

		DragDropEffects effect;
		bool useDefaultCursors;

		//  --- Constructor
		
		public GiveFeedbackEventArgs(  DragDropEffects effect,  bool useDefaultCursors )
		{
			this.effect = effect;
			this.useDefaultCursors = useDefaultCursors;
		}
		
		//  --- Public Properties
		public DragDropEffects Effect {
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

		
		//	--- Public Methods
		
//		[MonoTODO]
//		public virtual bool Equals(object o);
//		{
//			//throw new NotImplementedException ();
//			return false;
//		}
//		[MonoTODO]
//		public static bool Equals(object o1, object o2);
//		{
//			throw new NotImplementedException ();
//		}
	}
}

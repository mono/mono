//
// System.Windows.Forms.NavigateEventArgs.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//	Partially completed by Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

        public class NavigateEventArgs : EventArgs {
			private bool isforward;
		//
		//  --- Constructor
		//
		public NavigateEventArgs(bool isForward)
		{
			isforward = isForward;
		}

		//
		//  --- Public Properties
		//
		public bool Forward {
			get {
				return isforward;
			}
		}

		//
		//  --- Public Methods
		//
//		[MonoTODO]
//		public virtual bool Equals(object o)
//		{
//			throw new NotImplementedException ();
//		}
//		[MonoTODO]
//		public static bool Equals(object o1, object o2)
//		{
//			throw new NotImplementedException ();
//		}
//		[MonoTODO]
//		public virtual int GetHashCode()
//		{
//			throw new NotImplementedException ();
//		}
//		[MonoTODO]
//		public Type GetType()
//		{
//			throw new NotImplementedException ();
//		}
//		[MonoTODO]
//		public virtual string ToString()
//		{
//			throw new NotImplementedException ();
//		}
//
//		//
//		//  --- Protected Methods
//		//
//		[MonoTODO]
//		protected object MemberwiseClone()
//		{
//			throw new NotImplementedException ();
//		}
//		//
//		//  --- DeConstructor
//		//
//		[MonoTODO]
//		~NavigateEventArgs()
//		{
//			throw new NotImplementedException ();
//		}
	 }
}

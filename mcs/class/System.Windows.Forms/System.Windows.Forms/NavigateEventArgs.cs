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
		[MonoTODO]
		public override bool Equals(object o)
		{
			throw new NotImplementedException ();
		}
		//inherited
		//public static bool Equals(object o1, object o2)
		//{
		//	throw new NotImplementedException ();
		//}
			[MonoTODO]
			public override int GetHashCode() {
				//FIXME add our proprities
				return base.GetHashCode();
			}
		//inherited
		//public Type GetType()
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException ();
		}

		//
		//  --- Protected Methods
		//
		//inherited
		//protected object MemberwiseClone()
		//{
		//	throw new NotImplementedException ();
		//}
		//
		//  --- DeConstructor
		//
		[MonoTODO]
		~NavigateEventArgs()
		{
			throw new NotImplementedException ();
		}
	 }
}

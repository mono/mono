//
// System.Windows.Forms.HelpProvider.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>
	using System.ComponentModel;
	public class HelpProvider : Component, IExtenderProvider {

		//
		//  --- Constructor
		//

		[MonoTODO]
		public HelpProvider() {
			
		}

		//
		//  --- Public Properties
		//

		[MonoTODO]
		public virtual string HelpNamespace {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Public Methods
		//

		//public void Dispose() {
		//	throw new NotImplementedException ();
		//}

		[MonoTODO]
		public override bool Equals(object o) {
			throw new NotImplementedException ();
		}

		//public static bool Equals(object o1, object o2) {
		//	throw new NotImplementedException ();
		//}

		[MonoTODO]
		public override int GetHashCode() {
			//FIXME add our proprities
			return base.GetHashCode();
		}

		[MonoTODO]
		public virtual  string GetHelpKeyword(Control ctl) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual  HelpNavigator GetHelpNavigator(Control ctl) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual  string GetHelpString(Control ctl) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual  bool GetShowHelp(Control ctl) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual  void SetHelpKeyword(Control ctl, string keyword) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual  void SetHelpNavigator(Control ctl, HelpNavigator nav) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual  void SetHelpString(Control ctl, string helpString) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual  void SetShowHelp(Control ctl, bool val) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ToString() {
			throw new NotImplementedException ();
		}

		//
		//  --- Public Events
		
		//
		//  --- Protected Methods
		//

		bool IExtenderProvider.CanExtend(object extendee){
			throw new NotImplementedException ();
		}	
	}
}

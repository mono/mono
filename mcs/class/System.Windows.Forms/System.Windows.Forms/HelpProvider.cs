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
				//FIXME:
			}
		}

		//
		//  --- Public Methods
		//

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
			//FIXME:
		}

		[MonoTODO]
		public virtual  void SetHelpNavigator(Control ctl, HelpNavigator nav) {
			//FIXME:
		}

		[MonoTODO]
		public virtual  void SetHelpString(Control ctl, string helpString) {
			//FIXME:
		}

		[MonoTODO]
		public virtual  void SetShowHelp(Control ctl, bool val) {
			//FIXME:
		}

		[MonoTODO]
		public override string ToString() {
			//FIXME:
			return base.ToString();
		}
		
		//
		//  --- Protected Methods
		//

		bool IExtenderProvider.CanExtend(object extendee){
			throw new NotImplementedException ();
		}	
	}
}

//
// System.Windows.Forms.HelpProvider.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Dennis Hayes (dennish@raytek.com)
// (C) 2002/3 Ximian, Inc
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
		public virtual bool CanExtend(object target) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void ResetShowHelp(Control ctl) {
		}

		[MonoTODO]
		public virtual string GetHelpKeyword(Control ctl) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual HelpNavigator GetHelpNavigator(Control ctl) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string GetHelpString(Control ctl) {
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
		public virtual  void SetHelpNavigator(Control ctl, HelpNavigator navigator) {
			//FIXME:
		}

		[MonoTODO]
		public virtual  void SetHelpString(Control ctl, string helpString) {
			//FIXME:
		}

		[MonoTODO]
		public virtual  void SetShowHelp(Control ctl, bool value) {
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

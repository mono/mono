//
// System.Windows.Forms.Timer
//
// Author:
//   stubbed out by Jackson Harper (jackson@latitudegeo.com)
//	Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	// </summary>
using System.ComponentModel;
    public class Timer : Component {

		//
		//  --- Public Constructors
		//
		[MonoTODO]
		public Timer()
		{
			
		}
		[MonoTODO]
		public Timer(IContainer container)
		{
			
		}
		//
		// --- Public Properties
		//
		[MonoTODO]
		public virtual bool Enabled {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public int Interval {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		//
		// --- Public Methods
		//
		[MonoTODO]
		public void Start() 
		{
			//FIXME:
		}
		[MonoTODO]
		public void Stop() 
		{
			//FIXME:
		}
		[MonoTODO]
		public override string ToString() 
		{
			//FIXME:
			return base.ToString();
		}
		//
		// --- Public Events
		//
		[MonoTODO]
		public event EventHandler Tick;
		//
		// --- Protected Methods
		//

		[MonoTODO]
		protected virtual void OnTick(EventArgs e) 
		{
			//FIXME:
		}
	}
}


//
// System.Windows.Forms.Timer
//
// Authors:
//	Alberto Fernández (infjaf00@yahoo.es)

// mirar.
// TODO: If it's active, it can't be garbage collected.
// TODO: If it's owned by a Container, it's freed when the container is freed.

using System.ComponentModel;
using System.Timers;


namespace System.Windows.Forms{

	[MonoTODO]
	public class Timer : Component {
		
		private System.Timers.Timer t = new System.Timers.Timer();

		public Timer (){
			t.Elapsed += new ElapsedEventHandler (this.OnElapsed);
		}
		public Timer (IContainer container){
			container.Add(this);
			t.Elapsed += new ElapsedEventHandler (this.OnElapsed);
		}
		
		private void OnElapsed (object o, ElapsedEventArgs e){
			OnTick (e);
		}

		public bool Enabled {
			get{return t.Enabled;}
			set{t.Enabled = value;}
		}

		public int Interval {
			get {return (int) t.Interval;}
			set {t.Interval = (float) value;}
		}


		public virtual void Start() {
			t.Start();
		}

		public virtual void Stop() {
			t.Stop();
		}

		public override string ToString(){
			return "[" + GetType().FullName.ToString() + "], Interval: " + Interval;
		}

		public event EventHandler Tick;

		protected virtual void OnTick(EventArgs e){
			if ( Tick != null )
				Tick ( this, e );
		}
		protected override void Dispose( bool disposing	) {
			Enabled = false;
			base.Dispose ( disposing );
		}
	}

}

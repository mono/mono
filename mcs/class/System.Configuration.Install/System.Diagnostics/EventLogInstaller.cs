// System.Diagnostics.EventLogInstaller.cs
//
// Author:
// 	Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) Novell
//

using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Runtime.InteropServices;

namespace System.Diagnostics
{
	public class EventLogInstaller : ComponentInstaller
	{
		public EventLogInstaller ()
		{
		}

		[MonoTODO]
		public override void CopyFromComponent (IComponent component)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Install (IDictionary stateSaver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool IsEquivalentInstaller (ComponentInstaller otherInstaller)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Rollback (IDictionary savedState)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Uninstall (IDictionary savedState)
		{
			throw new NotImplementedException ();
		}

		[TypeConverter ("System.Diagnostics.Design.StringValueConverter, " + Consts.AssemblySystem_Design)]
		public string Log {
			get {
				if (_log == null && _source != null)
					_log = EventLog.LogNameFromSourceName (_source, ".");

				return _log;
			}
			set {
				_log = value;
			}
		}

		[TypeConverter ("System.Diagnostics.Design.StringValueConverter, " + Consts.AssemblySystem_Design)]
		public string Source {
			get {
				return _source;
			}
			set {
				_source = value;
			}
		}

		[DefaultValue (UninstallAction.Remove)]
		public UninstallAction UninstallAction {
			get {
				return _uninstallAction;
			}
			set {
				if (!Enum.IsDefined(typeof(UninstallAction), value))
					// LAMESPEC, the docs do not mention this, but 
					// this exception is indeed thrown for invalid
					// values
					throw new InvalidEnumArgumentException("value", 
						(int) value, typeof(UninstallAction));

				_uninstallAction = value;
			}
		}

		private string _log;
		private string _source;
		private UninstallAction _uninstallAction = UninstallAction.Remove;
	}
}

// System.Diagnostics.PerformanceCounterInstaller.cs
//
// Author:
// 	Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) Novell
//

using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;

namespace System.Diagnostics
{
	public class PerformanceCounterInstaller : ComponentInstaller
	{
		public PerformanceCounterInstaller ()
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
		public override void Rollback (IDictionary savedState)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Uninstall (IDictionary savedState)
		{
			throw new NotImplementedException ();
		}

		[DefaultValue ("")]
		[MonitoringDescription ("PCI_CategoryHelp")]
		public string CategoryHelp {
			get {
				return _categoryHelp;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				_categoryHelp = value;
			}
		}

		[DefaultValue ("")]
		[TypeConverter ("System.Diagnostics.Design.StringValueConverter, " + Consts.AssemblySystem_Design)]
		public string CategoryName {
			get {
				return _categoryName;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				_categoryName = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[MonitoringDescription("PCI_Counters")]
		public CounterCreationDataCollection Counters {
			get {
				return _counters;
			}
		}

		[DefaultValue (UninstallAction.Remove)]
		[MonitoringDescription ("PCI_UninstallAction")]
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

		private string _categoryHelp = string.Empty;
		private string _categoryName = string.Empty;
		private CounterCreationDataCollection _counters = new CounterCreationDataCollection ();
		private UninstallAction _uninstallAction = UninstallAction.Remove;
	}
}

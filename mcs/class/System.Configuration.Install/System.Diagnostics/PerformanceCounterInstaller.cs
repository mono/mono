// System.Diagnostics.PerformanceCounterInstaller.cs
//
// Author:
// 	Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) Gert Driesen
// (C) Novell
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

using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Runtime.InteropServices;

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
#if !NET_2_0
		[MonitoringDescription ("PCI_CategoryHelp")]
#endif
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
#if !NET_2_0
		[MonitoringDescription("PCI_Counters")]
#endif
		public CounterCreationDataCollection Counters {
			get {
				return _counters;
			}
		}

		[DefaultValue (UninstallAction.Remove)]
#if !NET_2_0
		[MonitoringDescription ("PCI_UninstallAction")]
#endif
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

#if NET_2_0
		[ComVisible (false)]
		[DefaultValue (PerformanceCounterCategoryType.Unknown)]
		public PerformanceCounterCategoryType CategoryType {
			get {
				return _categoryType;
			}
			set {
				if (!Enum.IsDefined(typeof(PerformanceCounterCategoryType), value))
					// LAMESPEC, the docs do not mention this, but 
					// this exception is indeed thrown for invalid
					// values
					throw new InvalidEnumArgumentException("value", 
						(int) value, typeof(PerformanceCounterCategoryType));

				_categoryType = value;
			}
		}
#endif

		private string _categoryHelp = string.Empty;
		private string _categoryName = string.Empty;
		private CounterCreationDataCollection _counters = new CounterCreationDataCollection ();
		private UninstallAction _uninstallAction = UninstallAction.Remove;
#if NET_2_0
		private PerformanceCounterCategoryType _categoryType;
#endif
	}
}

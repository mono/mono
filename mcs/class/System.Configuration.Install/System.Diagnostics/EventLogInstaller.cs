// System.Diagnostics.EventLogInstaller.cs
//
// Author:
// 	Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) Gert Driesen
// (C) Novell Inc.
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
	public class EventLogInstaller : ComponentInstaller
	{
		public EventLogInstaller ()
		{
		}

#if NET_2_0
		[MonoTODO]
		[ComVisible (false)]
		public int CategoryCount {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		[Editor ("System.Windows.Forms.Design.FileNameEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing )]
		[TypeConverter ("System.Diagnostics.Design.StringValueConverter, " + Consts.AssemblySystem_Design)]
		[ComVisible (false)]
		public string CategoryResourceFile {
			get { return _categoryResourceFile; }
			set { _categoryResourceFile = value; }
		}

		[MonoTODO]
		[Editor ("System.Windows.Forms.Design.FileNameEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing )]
		[TypeConverter ("System.Diagnostics.Design.StringValueConverter, " + Consts.AssemblySystem_Design)]
		[ComVisible (false)]
		public string MessageResourceFile {
			get { return _messageResourceFile; }
			set { _messageResourceFile = value; }
		}

		[MonoTODO]
		[Editor ("System.Windows.Forms.Design.FileNameEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing )]
		[TypeConverter ("System.Diagnostics.Design.StringValueConverter, " + Consts.AssemblySystem_Design)]
		[ComVisible (false)]
		public string ParameterResourceFile {
			get { return _parameterResourceFile; }
			set { _parameterResourceFile = value; }
		}
#endif

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
#if NET_2_0
		private string _categoryResourceFile;
		private string _messageResourceFile;
		private string _parameterResourceFile;
#endif
	}
}

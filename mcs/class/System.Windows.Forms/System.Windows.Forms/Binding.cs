//
// System.Windows.Forms.Binding.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	Dennis hayes (dennish@raytek.com)
//
// (C) 2002/3 Ximian, Inc
//

namespace System.Windows.Forms {
	public class Binding {

		/// <summary>
		/// Represents the simple binding between the property value of an object and the property value of a control.
		///
		/// ToDo note:
		///  - MarshalByRefObject members not stubbed out
		/// </summary>

		// --- Constructor
		//
		//Needed for CE
		public Binding(string propertyName,object dataSource,string dataMember)
		{
			
		}

		//
		// --- Public Properties
		//

		//Most functions needed for compact frame work

		//Needed for CE
		[MonoTODO]
		public BindingManagerBase BindingManagerBase  {

			get { throw new NotImplementedException (); }
		}
		//Needed for CE		
		[MonoTODO]
		public BindingMemberInfo BindingMemberInfo  {

			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public Control Control  {

			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public object DataSource  {

			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public bool IsBinding  {

			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public string PropertyName  {

			get { throw new NotImplementedException (); }
		}

		//
		// --- Public Methods
		//
		[MonoTODO]
		protected virtual void OnFormat(ConvertEventArgs cevent)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnParse(ConvertEventArgs cevent)
		{
			throw new NotImplementedException ();
		}

		//
		// --- Public Events
		//
		[MonoTODO]
		public event ConvertEventHandler Format {

			add { throw new NotImplementedException (); }
			remove { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public event ConvertEventHandler Parse {

			add { throw new NotImplementedException (); }
			remove { throw new NotImplementedException (); }
		}
	}
}

//
// System.Windows.Forms.Binding.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	Dennis hayes (dennish@raytek.com)
//
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

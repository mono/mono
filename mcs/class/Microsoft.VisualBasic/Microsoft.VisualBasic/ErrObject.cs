//
// ErrObject.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//
// (C) 2002 Chris J Breisch
//

using System;

namespace Microsoft.VisualBasic 
{
	sealed public class ErrObject {
		// Declarations
		// Constructors
		// Properties
		[MonoTODO]
		public System.Int32 HelpContext { get { throw new NotImplementedException (); } set { throw new NotImplementedException (); } }
		[MonoTODO]
		public System.Int32 LastDllError {  get { throw new NotImplementedException (); } }
		[MonoTODO]
		public System.Int32 Number {  get { throw new NotImplementedException (); } set { throw new NotImplementedException (); } }
		[MonoTODO]
		public System.Int32 Erl {  get { throw new NotImplementedException (); } }
		[MonoTODO]
		public System.String Source {  get { throw new NotImplementedException (); } set { throw new NotImplementedException (); } }
		[MonoTODO]
		public System.String HelpFile {  get { throw new NotImplementedException (); } set { throw new NotImplementedException (); } }
		[MonoTODO]
		public System.String Description {  get { throw new NotImplementedException (); } set { throw new NotImplementedException (); } }
		// Methods
		[MonoTODO]
		public System.Exception GetException () { throw new NotImplementedException (); }
		[MonoTODO]
		public void Clear () { throw new NotImplementedException (); }
		[MonoTODO]
		public void Raise (System.Int32 Number, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(null)] System.Object Source, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(null)] System.Object Description, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(null)] System.Object HelpFile, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(null)] System.Object HelpContext) { throw new NotImplementedException (); }
		// Events
	};
}

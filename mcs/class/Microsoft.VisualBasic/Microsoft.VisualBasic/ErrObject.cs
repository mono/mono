//
// ErrObject.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//   Francesco Delfino (pluto@tipic.com)
//
// (C) 2002 Chris J Breisch
//     2002 pluto@tipic.com
//

using System;

namespace Microsoft.VisualBasic 
{
	sealed public class ErrObject {
		// Declarations
		int pHelpContext;
		int pLastDllError;
		int pNumber;
		int pErl;
		string pSource;
		string pHelpFile;
		string pDescription;
		System.Exception pException;
		// Constructors
		// Properties
		[MonoTODO]
		public System.Int32 HelpContext 
		{ 
			get { 
				return pHelpContext; 
			} 
			set { 
				pHelpContext = value;
			} 
		}
		[MonoTODO]
		public System.Int32 LastDllError {  
			get { 
				return pLastDllError; 
			} 
		}
		[MonoTODO]
		public System.Int32 Number {  
			get { 
				return pNumber;
			} 
			set { 
				pNumber = value;
			} 
		}
		[MonoTODO]
		public System.Int32 Erl {  
			get { 
				return pErl;
			} 
		}
		[MonoTODO]
		public System.String Source {  
			get { 
				return pSource;
			} 
			set { 
				pSource = value;
			} 
		}
		[MonoTODO]
		public System.String HelpFile {  
			get { 
				return pHelpFile;
			} 
			set { 
				pHelpFile = value;
			} 
		}
		[MonoTODO]
		public System.String Description {  
			get { 
				return pDescription; 
			} 
			set { 
				pDescription = value;
			} 
		}
		// Methods
		[MonoTODO("We should parse the exception object to obtain VB-like error code. Not a trivial task!")]
		internal void SetException (Exception ex)
		{
			if (pException != ex) 
			{
				pNumber = 0xFFFF;
				pSource = ex.Source;
				pDescription = ex.Message + "\n" + ex.StackTrace;
			}
		}

		[MonoTODO]
		public System.Exception GetException () 
		{
			  return pException;
		}
		[MonoTODO]
		public void Clear () {
			pHelpContext=0;
			pLastDllError=0;
			pNumber=0;
			pErl=0;
			pSource="";
			pHelpFile="";
			pDescription="";
			pException= null;
		}
		[MonoTODO]
		public void Raise (System.Int32 Number, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(null)] System.Object Source, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(null)] System.Object Description, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(null)] System.Object HelpFile, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(null)] System.Object HelpContext) 
		{ 
			throw new NotImplementedException (); 
		}
		// Events
	};
}
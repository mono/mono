//
// Financial.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//
// (C) 2002 Chris J Breisch
//

using System;

namespace Microsoft.VisualBasic 
{
	[Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute] 
	sealed public class Financial {
		// Declarations
		// Constructors
		// Properties
		// Methods
		[MonoTODO]
		public static System.Double DDB (System.Double Cost, System.Double Salvage, System.Double Life, System.Double Period, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(2)] System.Double Factor) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Double FV (System.Double Rate, System.Double NPer, System.Double Pmt, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0)] System.Double PV, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0)] Microsoft.VisualBasic.DueDate Due) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Double IPmt (System.Double Rate, System.Double Per, System.Double NPer, System.Double PV, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0)] System.Double FV, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0)] Microsoft.VisualBasic.DueDate Due) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Double IRR (ref System.Double[] ValueArray, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0.1)] ref System.Double Guess) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Double MIRR (ref System.Double[] ValueArray, ref System.Double FinanceRate, ref System.Double ReinvestRate) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Double NPer (System.Double Rate, System.Double Pmt, System.Double PV, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0)] System.Double FV, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0)] Microsoft.VisualBasic.DueDate Due) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Double NPV (System.Double Rate, ref System.Double[] ValueArray) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Double Pmt (System.Double Rate, System.Double NPer, System.Double PV, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0)] System.Double FV, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0)] Microsoft.VisualBasic.DueDate Due) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Double PPmt (System.Double Rate, System.Double Per, System.Double NPer, System.Double PV, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0)] System.Double FV, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0)] Microsoft.VisualBasic.DueDate Due) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Double PV (System.Double Rate, System.Double NPer, System.Double Pmt, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0)] System.Double FV, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0)] Microsoft.VisualBasic.DueDate Due) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Double Rate (System.Double NPer, System.Double Pmt, System.Double PV, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0)] System.Double FV, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0)] Microsoft.VisualBasic.DueDate Due, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0.1)] System.Double Guess) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Double SLN (System.Double Cost, System.Double Salvage, System.Double Life) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Double SYD (System.Double Cost, System.Double Salvage, System.Double Life, System.Double Period) { throw new NotImplementedException (); }
		// Events
	};
}

//
// FlowControl.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
//
namespace Microsoft.VisualBasic.CompilerServices {
	[Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute] 
	[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)] 
	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Auto)] 
	sealed public class FlowControl {
		// Declarations
		// Constructors
		// Properties
		// Methods
		public static System.Boolean ForNextCheckR4 (System.Single count, System.Single limit, System.Single StepValue) { return false;}
		public static System.Boolean ForNextCheckR8 (System.Double count, System.Double limit, System.Double StepValue) { return false;}
		public static System.Boolean ForNextCheckDec (System.Decimal count, System.Decimal limit, System.Decimal StepValue) { return false;}
		public static System.Boolean ForLoopInitObj (System.Object Counter, System.Object Start, System.Object Limit, System.Object StepValue, ref System.Object LoopForResult, ref System.Object CounterResult) { return false;}
		public static System.Boolean ForNextCheckObj (System.Object Counter, System.Object LoopObj, ref System.Object CounterResult) { return false;}
		public static System.Collections.IEnumerator ForEachInArr (System.Array ary) { return null;}
		public static System.Collections.IEnumerator ForEachInObj (System.Object obj) { return null;}
		public static System.Boolean ForEachNextObj (ref System.Object obj, ref System.Collections.IEnumerator enumerator) { return false;}
		public static void CheckForSyncLockOnValueType (System.Object obj) { }
		// Events
	};
}

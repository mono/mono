//
// System.Collections.DebuggableAttribute.cs
//
// Author:
//   Nick Drochak II (ndrochak@gol.com)
//
// (C) 2001 Nick Drochak II
//

namespace System.Diagnostics {

	public sealed class DebuggableAttribute : System.Attribute {

		private bool JITTrackingEnabledFlag;
		private bool JITOptimizerDisabledFlag;

		// Public Instance Constructors
		public DebuggableAttribute(bool isJITTrackingEnabled, bool isJITOptimizerDisabled) {
			JITTrackingEnabledFlag = isJITTrackingEnabled;
			JITOptimizerDisabledFlag = isJITOptimizerDisabled;
		}
		
		// Public Instance Properties
		public bool IsJITTrackingEnabled { get { return JITTrackingEnabledFlag; } }
		
		public bool IsJITOptimizerDisabled { get { return JITOptimizerDisabledFlag; } }
	}
}

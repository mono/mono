//
// System.Management.Instrumentation.InstrumentationClassAttribute
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.Management.Instrumentation
{
	[AttributeUsage(AttributeTargets.Class | 
			AttributeTargets.Struct)]
	public class InstrumentationClassAttribute : Attribute {
		public InstrumentationClassAttribute (InstrumentationType instrumentationType)
		{
			_instrumentationType = instrumentationType;
		}

		public InstrumentationClassAttribute (InstrumentationType instrumentationType, string managedBaseClassName)
		{
			_instrumentationType = instrumentationType;
			_managedBaseClassName = managedBaseClassName;
		}

		public InstrumentationType InstrumentationType {
			get {
				return _instrumentationType;
			}
		}

		public string ManagedBaseClassName {
			get {
				if (_managedBaseClassName == null || _managedBaseClassName.Length == 0)
					return null;

				return _managedBaseClassName;
			}
		}

		private InstrumentationType _instrumentationType;
		private string _managedBaseClassName;
	}
}

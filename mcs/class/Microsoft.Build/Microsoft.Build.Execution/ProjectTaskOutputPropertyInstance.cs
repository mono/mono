using System;
using Microsoft.Build.Construction;

namespace Microsoft.Build.Execution
{
	public class ProjectTaskOutputPropertyInstance : ProjectTaskInstanceChild
	{
		public string PropertyName { get; private set; }
		public string TaskParameter { get; private set; }

		public override string Condition {
			get { throw new NotImplementedException (); }
		}
		#if NET_4_5
		public ElementLocation PropertyNameLocation { get; private set; }

		public override ElementLocation ConditionLocation {
			get { throw new NotImplementedException (); }
		}
		public override ElementLocation Location {
			get { throw new NotImplementedException (); }
		}
		public override ElementLocation TaskParameterLocation {
			get { throw new NotImplementedException (); }
		}
		#endif
	}
}


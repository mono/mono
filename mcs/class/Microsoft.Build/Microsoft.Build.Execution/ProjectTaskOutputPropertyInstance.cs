using System;
using Microsoft.Build.Construction;

namespace Microsoft.Build.Execution
{
	public class ProjectTaskOutputPropertyInstance : ProjectTaskInstanceChild
	{
		internal ProjectTaskOutputPropertyInstance (ProjectOutputElement xml)
		{
			condition = xml.Condition;
			PropertyName = xml.PropertyName;
			TaskParameter = xml.TaskParameter;
			#if NET_4_5
			condition_location = xml.ConditionLocation;
			location = xml.Location;
			task_parameter_location = xml.TaskParameterLocation;
			#endif
		}

		public string PropertyName { get; private set; }
		public string TaskParameter { get; private set; }

		readonly string condition;
		public override string Condition {
			get { return condition; }
		}
		
		#if NET_4_5
		readonly ElementLocation condition_location, location, task_parameter_location;
		public ElementLocation PropertyNameLocation { get; private set; }
		public override ElementLocation ConditionLocation {
			get { return condition_location; }
		}		
		public override ElementLocation Location {
			get { return location; }
		}
		public override ElementLocation TaskParameterLocation {
			get { return task_parameter_location; }
		}
		#endif
	}
}


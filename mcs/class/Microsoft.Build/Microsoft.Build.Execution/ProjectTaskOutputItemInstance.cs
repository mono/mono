using System;
using Microsoft.Build.Construction;

namespace Microsoft.Build.Execution
{
	public class ProjectTaskOutputItemInstance : ProjectTaskInstanceChild
	{
		internal ProjectTaskOutputItemInstance (ProjectOutputElement xml)
		{
			condition = xml.Condition;
			ItemType = xml.ItemType;
			TaskParameter = xml.TaskParameter;
			condition_location = xml.ConditionLocation;
			location = xml.Location;
			task_parameter_location = xml.TaskParameterLocation;
		}
		
		public string ItemType { get; private set; }
		public string TaskParameter { get; private set; }

		readonly string condition;
		public override string Condition {
			get { return condition; }
		}
		readonly ElementLocation condition_location, location, task_parameter_location;
		public ElementLocation ItemTypeLocation { get; private set; }
		public override ElementLocation ConditionLocation {
			get { return condition_location; }
		}
		public override ElementLocation Location {
			get { return location; }
		}
		public override ElementLocation TaskParameterLocation {
			get { return task_parameter_location; }
		}
	}
}


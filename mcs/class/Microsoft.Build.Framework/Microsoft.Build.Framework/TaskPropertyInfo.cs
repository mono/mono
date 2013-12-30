using System;
using System.Collections.Generic;

namespace Microsoft.Build.Framework
{
	[Serializable]
	public class TaskPropertyInfo
	{
		public TaskPropertyInfo (string name, Type typeOfParameter, bool output, bool required)
		{
			Name = name;
			PropertyType = typeOfParameter;
			Output = output;
			Required = required;
		}
		
		public string Name { get; private set; }
		public bool Output { get; private set; }
		public Type PropertyType { get; private set; }
		public bool Required { get; private set; }
	}
}


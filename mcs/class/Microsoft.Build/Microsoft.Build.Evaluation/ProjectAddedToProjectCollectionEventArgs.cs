using System;
using Microsoft.Build.Construction;

namespace Microsoft.Build.Evaluation
{
	public class ProjectAddedToProjectCollectionEventArgs : EventArgs
	{
		public ProjectAddedToProjectCollectionEventArgs (ProjectRootElement project)
		{
			if (project == null)
				throw new ArgumentNullException ("project");
			ProjectRootElement = project;
		}
		
		public ProjectRootElement ProjectRootElement { get; private set; }
	}
}


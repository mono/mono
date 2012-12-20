using System;

namespace Monodoc
{
	public abstract class Provider
	{
		//
		// This code is used to "tag" all the different sources
		//
		static short serial;

		public int Code { get; set; }

		public Provider ()
		{
			Code = serial++;
		}

		public abstract void PopulateTree (Tree tree);

		//
		// Called at shutdown time after the tree has been populated to perform
		// any fixups or final tasks.
		//
		public abstract void CloseTree (HelpSource hs, Tree tree);
	}
}

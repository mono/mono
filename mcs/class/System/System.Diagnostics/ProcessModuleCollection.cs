//
// System.Diagnostics.ProcessModuleCollection.cs
//
// Authors:
//   Dick Porter (dick@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System.Collections;

namespace System.Diagnostics 
{
	public class ProcessModuleCollection : ReadOnlyCollectionBase 
	{
		
		protected ProcessModuleCollection() 
		{
		}

		public ProcessModuleCollection(ProcessModule[] processModules) 
		{
			InnerList.AddRange (processModules);
		}
		
		public ProcessModule this[int index] {
			get {
				return (ProcessModule)InnerList[index];
			}
		}

		public bool Contains(ProcessModule module) 
		{
			return InnerList.Contains (module);
		}

		public void CopyTo(ProcessModule[] array, int index) 
		{
			InnerList.CopyTo (array, index);
		}

		public int IndexOf(ProcessModule module) 
		{
			return InnerList.IndexOf (module);
		}
	}
}

//
// System.Diagnostics.ProcessThreadCollection.cs
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
	public class ProcessThreadCollection : ReadOnlyCollectionBase 
	{
		protected ProcessThreadCollection() 
		{
		}

		public ProcessThreadCollection(ProcessThread[] processThreads) 
		{
			InnerList.AddRange (processThreads);
		}
		
		public ProcessThread this[int index] {
			get {
				return (ProcessThread)InnerList[index];
			}
		}

		public int Add(ProcessThread thread) 
		{
			return InnerList.Add (thread);
		}

		public bool Contains(ProcessThread thread) 
		{
			return InnerList.Contains (thread);
		}

		public void CopyTo(ProcessThread[] array, int index) 
		{
			InnerList.CopyTo (array, index);
		}

		public int IndexOf(ProcessThread thread) 
		{
			return InnerList.IndexOf (thread);
		}

		public void Insert(int index, ProcessThread thread) 
		{
			InnerList.Insert (index, thread);
		}

		public void Remove(ProcessThread thread) 
		{
			InnerList.Remove (thread);
		}
	}
}

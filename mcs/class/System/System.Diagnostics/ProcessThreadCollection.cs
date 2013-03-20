//
// System.Diagnostics.ProcessThreadCollection.cs
//
// Authors:
//   Dick Porter (dick@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections;

namespace System.Diagnostics 
{
#if NET_2_1
	public class ProcessThreadCollectionBase : System.Collections.Generic.List<ProcessThread>
	{
		protected ProcessThreadCollectionBase InnerList {
			get { return this; }
		}

		public new int Add (ProcessThread thread)
		{
			base.Add (thread);
			return Count - 1;
		}
	}
#endif

	public class ProcessThreadCollection :
#if !NET_2_1
		ReadOnlyCollectionBase
#else
		ProcessThreadCollectionBase
#endif
	{
		protected ProcessThreadCollection() 
		{
		}

		internal static ProcessThreadCollection GetEmpty ()
		{
			return new ProcessThreadCollection ();
		}
		
		public ProcessThreadCollection(ProcessThread[] processThreads) 
		{
			InnerList.AddRange (processThreads);
		}

#if !NET_2_1		
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
#endif
	}
}

//
// System.Diagnostics.ProcessModuleCollection.cs
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
	public class ProcessModuleCollectionBase : System.Collections.Generic.List<ProcessModule>
	{
		protected ProcessModuleCollectionBase InnerList {
			get { return this; }
		}
	}
#endif

	public class ProcessModuleCollection :
#if !NET_2_1	
		ReadOnlyCollectionBase
#else
		ProcessModuleCollectionBase
#endif
	{
		protected ProcessModuleCollection() 
		{
		}

		public ProcessModuleCollection(ProcessModule[] processModules) 
		{
			InnerList.AddRange (processModules);
		}
		
#if !NET_2_1		
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
#endif
	}
}

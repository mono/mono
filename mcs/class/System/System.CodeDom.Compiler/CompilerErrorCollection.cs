//
// System.CodeDom.Compiler CompilerErrorCollection Class implementation
//
// Authors:
// 	Daniel Stodden (stodden@in.tum.de)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc. (http://www.ximian.com)
//
using System.Collections;
namespace System.CodeDom.Compiler
{
	[MonoTODO]
	public class CompilerErrorCollection : CollectionBase
	{
		[MonoTODO]
		public CompilerErrorCollection ()
		{
		}

		public CompilerErrorCollection (CompilerErrorCollection value)
		{
			InnerList.AddRange(value.InnerList);
		}

		public CompilerErrorCollection (CompilerError[] value)
		{
			InnerList.AddRange(value);
		}

		public int Add (CompilerError value)
		{
			return InnerList.Add(value);
		}

		public void AddRange (CompilerError[] value)
		{
			InnerList.AddRange(value);
		}

		public void AddRange (CompilerErrorCollection value)
		{
			InnerList.AddRange(value.InnerList);
		}

		public bool Contains (CompilerError value)
		{
			return InnerList.Contains(value);
		}

		public void CopyTo (CompilerError[] array, int index)
		{
			InnerList.CopyTo(array,index);
		}

		public int IndexOf (CompilerError value)
		{
			return InnerList.IndexOf(value);
		}

		public void Insert (int index, CompilerError value)
		{
			InnerList.Insert(index,value);
		}

		public void Remove (CompilerError value)
		{
			InnerList.Remove(value);
		}

		public CompilerError this [int index]
		{
			get { return (CompilerError) InnerList[index]; }
			set { InnerList[index]=value; }
		}

		public bool HasErrors
		{
			get {
				foreach (CompilerError error in InnerList)
					if (!error.IsWarning) return true;
				return false;
			}
		}

		public bool HasWarnings
		{
			get {
				foreach (CompilerError error in InnerList)
					if (error.IsWarning) return true;
				return false;
			}
		}
	}
}


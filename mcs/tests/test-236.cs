// Bug #56300

using System;
using System.Collections;

namespace Tests
{
	public interface IIndexer { object this[int index] { get; set; } }
	
	public class Test : IIndexer
	{
		object[] InnerList;
		object IIndexer.this[int index] { 
			get { return InnerList[index]; }
			set { InnerList[index] = value; }
		}

		public static void Main() {
			if (Attribute.GetCustomAttribute(
				    typeof(Test),
				    typeof(System.Reflection.DefaultMemberAttribute)) != null)
				throw new Exception("Class 'Test' has a DefaultMemberAttribute");
		}
	}
}

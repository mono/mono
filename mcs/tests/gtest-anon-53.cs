using System;
using System.Collections.Generic;

namespace TestComp
{
	class Program
	{
		internal class MyClass
		{
			public delegate void MyDelegate (out List<int> intToAdd);
			private void MyTemplate (MyDelegate myData)
			{
			}
			public void UseATemplate ()
			{
				MyTemplate (
					delegate (out List<int> intToAdd) {
						intToAdd = new List<int> ();
						intToAdd.Add (0);
					}
				);
			}
		}

		static void Main (string[] args)
		{
		}
	}
}

using System;
using System.Collections.Generic;

class Test
{
	static void Main ()
	{

	}

	private void DetermineLinkedCells ()
	{
		List<object> objectList1 = null;
		List<object> objectList2 = null;

		{
			object object1 = null;
			{
				objectList2.FindAll (new Predicate<object> (
						delegate (object objectarg1) {
							return objectarg1.Equals (objectList1);
						}));

			}

			objectList2.FindAll (new Predicate<object> (
				delegate (object objectarg2) {
					return objectarg2.Equals (object1);
				}));
		}
	}
}

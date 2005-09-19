// cs0619-44.cs: `Obsolete' is obsolete: `!!!'
// Line: 9

[System.Obsolete("!!!", true)]
class Obsolete {
}

class Class {
		void VV ()
		{
			object[] o = new object [] { new Obsolete () };
		}
}

// cs0619.cs: 'AA' is obsolete: ''
// Line: 9

[System.Obsolete ("", true)]
class AA
{
        public AA ()
        {
                foreach (AA aa in new System.Collections.ArrayList ()) {
                        System.Console.WriteLine (aa);
                }
        }
	static void Main ()
	{
	}

}

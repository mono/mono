// cs0619.cs: 'AA' is obsolete: ''
// Line: 9

[System.Obsolete ("", true)]
class AA
{
        public AA ()
        {
                for (AA aa = null; aa != null;) {
                        System.Console.WriteLine (aa);
                }
        }

	static void Main () {}
}

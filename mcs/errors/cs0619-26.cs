// cs0619-26.cs: `A' is obsolete: `'
// Line: 13

[System.Obsolete ("", true)]
class A
{
}

class AA
{
        public AA ()
        {
                for (A aa = null; aa != null;) {
                        System.Console.WriteLine (aa);
                }
        }
}

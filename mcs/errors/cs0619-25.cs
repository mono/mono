// CS0619: `A' is obsolete: `'
// Line: 9

[System.Obsolete ("", true)]
class A
{
}

class AA
{
        public AA ()
        {
                foreach (A aa in new System.Collections.ArrayList ()) {
                        System.Console.WriteLine (aa);
                }
        }
}

// CS0619: `O' is obsolete: `'
// Line: 12

[System.Obsolete("", true)]
class O
{
}

class MainClass {
        public void Method ()
        {
                lock (new O ()) {
                }
        }
}
// cs0619.cs: 'MainClass' is obsolete: ''
// Line: 8

[System.Obsolete("", true)]
class MainClass {
        public void Method ()
        {
                lock (new MainClass ()) {
                }
        }
}
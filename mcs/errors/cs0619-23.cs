// cs0619.cs: 'AA' is obsolete: ''
// Line: 17

[System.Obsolete ("", true)]
class AA
{
        public void Foo () {}
}

class B {
        public B (object a)
        {
                ((AA)a).Foo ();
        }
}
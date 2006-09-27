// CS0619: `AA' is obsolete: `'
// Line: 13

[System.Obsolete ("", true)]
class AA
{
        public void Foo () {}
}

class B {
        public B (object a)
        {
                object o = ((AA)a);
        }
}
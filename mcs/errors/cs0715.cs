// cs0715.cs: 'StaticClass.implicit operator StaticClass(int)': static classes cannot contain user-defined operators
// Line: 5

static class StaticClass {
        public static implicit operator StaticClass (int arg)
        {
            return null;
        }
}



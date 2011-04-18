// CS0715: `StaticClass.implicit operator StaticClass(int)': Static classes cannot contain user-defined operators
// Line: 5

static class StaticClass {
        public static implicit operator StaticClass (int arg)
        {
            return null;
        }
}



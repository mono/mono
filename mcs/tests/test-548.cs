using System;

namespace Bugs
{
    class Bug0
    {
        struct MyBoolean
        {
            private bool value;
            public MyBoolean(bool value)
            {
                this.value = value;
            }
            public static implicit operator MyBoolean(bool value)
            {
                return new MyBoolean(value);
            }
            public static implicit operator bool(MyBoolean b)
            {
                return b.value;
            }
        }

        public static int Main()
        {
            MyBoolean b = true;
            if (true && b)
            {
                return 0;
            }
            else
            {
                return 100;
            }
        }
    }
}
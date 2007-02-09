using System;

namespace Bugs
{
    class Bug2
    {
        struct MyByte
        {
            private byte value;
            public MyByte(byte value)
            {
                this.value = value;
            }
            public static implicit operator MyByte(byte value)
            {
                return new MyByte(value);
            }
            public static implicit operator byte(MyByte b)
            {
                return b.value;
            }
        }
        
        struct MyInt
        {
            private int value;
            public MyInt(int value)
            {
                this.value = value;
            }
            public static implicit operator MyInt(int value)
            {
                return new MyInt(value);
            }
            public static implicit operator int(MyInt b)
            {
                return b.value;
            }
        }

        public static void Main(string[] args)
        {
            MyByte b = 255;
            b += 255;
            Console.WriteLine(b);
            
            MyInt i = 3;
            i &= (4 + i);
            Console.WriteLine(i);
        }
    }
}

interface IFoo {}
interface IBar : IFoo {}

class Mona<T> where T : IFoo {}

class Test
{
        public Mona<K> GetMona<K> () where K : IBar
        {
                return new Mona<K> ();
        }

        public static void Main () {}
}



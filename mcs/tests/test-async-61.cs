using System.Threading.Tasks;
using System;

class X
{
        public static void Main ()
        {
                new X ().Foo ().Wait ();
        }

        async Task Foo ()
        {
                await Task.Run (async () => {
                        for (var count = 1; count < 5; count++) {
                                Invoke (() => {
                                        Console.WriteLine ("{0}", count);
                                });
                        }
                });
        }

        void Invoke (Action a)
        {
                a ();
        }
}
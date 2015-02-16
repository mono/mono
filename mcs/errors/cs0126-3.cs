// CS0126: An object of a type convertible to `int' is required for the return statement
// Line: 15

using System.Threading.Tasks;

class MainClass
{
    public static void Main ()
    {
        Task<C> v = null;

        Task.Run (async () => {
            await Task.Yield ();
            if (v == null) {
                return;
            }

            return 1;
        });
    }
}

public class C
{
    string Id { get; set; }
}
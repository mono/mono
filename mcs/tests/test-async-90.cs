using System.Threading.Tasks;

static class Y
{
    public static string ExCall (this X x)
    {
        return null;
    }
}

class X
{
    static X Test (object o)
    {
        return null;
    }

    X Prop { get; set;}

    X Call ()
    {
        return null;
    }

    public static void Main ()
    {
        var x = new X ();
        x.Test ().Wait ();
    }

    async Task Test ()
    {
        var x = X.Test (await Task.FromResult (1))?.Prop?.ExCall ();
    }
}
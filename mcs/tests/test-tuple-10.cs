using System.Linq;

class Program {
    public static int Main ()
    {
        var l = (from f in (typeof (Program)).GetFields() select (name: f.Name, offset: 0)).ToList();
        return 0;
    }
}
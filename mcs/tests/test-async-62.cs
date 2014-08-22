using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Model
{
}

public class Tests<X>
{

    Task DeleteClient (Model m)
    {
        return null;
    }

    public async Task Delete<T> (IEnumerable<T> models)
        where T : Model
    {
        await Task.WhenAll (models.Select ((model) => DeleteClient (model)));
    }
}

class O
{
    public static void Main ()
    {
        new Tests<long> ().Delete (new Model[0]).Wait ();
    }
}
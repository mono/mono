// Bug #81158
using System;

public class App
{
    private delegate void TGenericDelegate<T>(string param);
    public static void Main (string[] args)
    {
	App app = new App ();
	app.Run();
    }
    
    public void Run ()
    {
	    TGenericDelegate<string> del = ADelegate<string>;
	    TestMethod <string> ("a param", ADelegate<string>);
	    TestMethod <string> ("another param", del);
    }
    
    private void TestMethod <T> (string param, TGenericDelegate<T> del)
    {
	Console.WriteLine ("TestMethod <T> called with param: {0}. Calling a delegate", param);
	if (del != null)
		del (param);
    }
    
    private void ADelegate <T> (string param)
    {
	Console.WriteLine ("ADelegate <T> called with param: {0}", param);
    }
}

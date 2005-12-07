using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public class CompositePoker : CompositeControl
{
	public CompositePoker () {
	}


	protected override void CreateChildControls () {
		throw new Exception ("who called me?");
	}
}

public class Test {
  	public static void Main (string[] args) {
		CompositePoker poker;
	  
		poker = new CompositePoker ();
		Console.WriteLine ("DataBind");
		Console.WriteLine ("--------");
		try { poker.DataBind(); } catch (Exception e) { Console.WriteLine (e);}

		poker = new CompositePoker ();
		Console.WriteLine ("Render");
		Console.WriteLine ("--------");
		try { poker.DataBind(); } catch (Exception e) { Console.WriteLine (e);}

		poker = new CompositePoker ();
		Console.WriteLine ("Controls");
		Console.WriteLine ("--------");
		try { ControlCollection c = poker.Controls; } catch (Exception e) { Console.WriteLine (e);}

		poker = new CompositePoker ();
		ICompositeControlDesignerAccessor accessor = (ICompositeControlDesignerAccessor)poker;
		Console.WriteLine ("RecreateChildControls");
		Console.WriteLine ("--------");
		try { accessor.RecreateChildControls(); } catch (Exception e) { Console.WriteLine (e);}
	}
}

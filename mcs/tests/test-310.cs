namespace test
{

	interface IIntf1
	{
		string GetType(int index);
	}
	
	interface IIntf2: IIntf1
	{
		bool IsDone();
	}
	
	class Impl: IIntf2
	{
		public string GetType(int index)
		{
			return "none";
		}
		
		public bool IsDone()
		{
			return true;
		}
	}

	class myclass
	{ 
	
	  public static void Main(string[] args)
	  {
	    IIntf1 intf = new Impl();
	    IIntf2 intf2 = intf as IIntf2;
	    if (intf2 != null) {
	    	string str = intf2.GetType(0);	    
	    }	  
	  }
	}
}

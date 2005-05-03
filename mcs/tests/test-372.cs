// Some interfaces, one is a superset of the other  
public interface Node  
{  
    int GetStat();  
}  
public interface FileNode : Node  
{  
    int NotUsed();  
}  
  
// Some basic implementations, one is a superset of the other  
public class GenericNode : Node  
{  
    public virtual int GetStat() { return 0; }  
}  
  
public class GenericFileNode : GenericNode , FileNode  
{  
    public virtual int NotUsed() { return -1; }  
}  
  
  
// Now the ability to override a method depends on if we specify again that we  
// implement an interface -- although we must because we derive from a class  
// that does.  
public class WorkingTest : GenericFileNode , FileNode  
{  
    public override int GetStat() { return 42; }  
}  
  
public class FailingTest : GenericFileNode  
{  
    // This never gets called, but it builds, so what did we override?!!! 
    public override int GetStat() { return 42; }  
}  
  
public class TestWrapper  
{  
    static bool Test(Node inst, string name)  
    {  
        if(inst.GetStat() == 42)  
        {  
            System.Console.WriteLine("{0} -- Passed", name);  
            return true;  
        } else  
        {  
            System.Console.WriteLine("{0} -- FAILED", name);  
            return false;  
        }  
    }  
  
    public static int Main()  
    {  
        if( Test(new WorkingTest(), "WorkingTest")  
                && Test(new FailingTest(), "FailingTest") )  
            return 0; // everything worked  
        else  
            return 1;  
    }  
}




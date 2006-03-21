using System;

[AttributeUsage( AttributeTargets.Property, AllowMultiple=false,
Inherited=true )]
public class TableColumn : Attribute
{
	public object MagicValue 
	{
                get { return null; }
		set { }
	}
        
        public object Value2;
}
        
class Bug
{
    [TableColumn(MagicValue=2,Value2=0)] 
    public int TInt 
    {
         get { return 0; }
    }    
    
    public static void Main ()
    {
        const object o = null;       
    }

}

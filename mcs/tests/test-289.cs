using System;

public class Test
{
	public static void Main () {}
            
	public string Value { set { } }
	public void set_Value () { return; }
        
        void set_Item (int a, int b, bool c) {}
        int this[int i] { set {} }             
        bool this [bool i] { get { return false; } }
       
}

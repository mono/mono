// Compiler options: -doc:xml-058.xml
using System; 

///<summary>This file throws an error when compiled with XML documentation</summary>
public class GenericClass <gT>
{
    gT m_data; 

    ///<summary>This line caused bug #77183</summary>
    public GenericClass (gT Data)
    {
        m_data = Data; 
    }
}

class Foo
{
    public static void Main () {}
}

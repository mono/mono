// CS1570: XML documentation comment on `Test' is not well-formed XML markup (Unexpected XML declaration. The XML declaration must be the first node in the document, and no white space characters are allowed to appear before it. Line 3, position 3.)
// Line: 13
// Compiler options: -doc:dummy.xml -warnaserror -warn:1

/// Text goes here.
///
/// <?xml version = "1.0" encoding = "utf-8" ?>
/// <configuration>
///     <appSettings>
///         <add key = "blah" value = "blech"/>
///     </appSettings>
/// </configuration> 
public class Test
{    
    static void Main ()
    {
    }
}


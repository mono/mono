// cs0550.cs: 'PropertyClass.PropertyInterface.this[bool].get' adds an accessor not found in interface member 'PropertyInterface.Value'
// Line: 13

using System.Runtime.CompilerServices;

interface PropertyInterface {
        int this[bool b] { set; }
}

public class PropertyClass: PropertyInterface {
        int PropertyInterface.this [bool b]{ 
                get { 
                        return 0;
                } 
                set { }
        }
}



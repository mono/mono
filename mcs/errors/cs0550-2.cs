// CS0550: `PropertyClass.PropertyInterface.this[bool].get' is an accessor not found in interface member `PropertyInterface.this[bool]'
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



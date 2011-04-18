// CS0551: Explicit interface implementation `PropertyClass.PropertyInterface.Value' is missing accessor `PropertyInterface.Value.set'
// Line: 9

interface PropertyInterface {
        int Value { get; set; }
}

public class PropertyClass: PropertyInterface {
        int PropertyInterface.Value { 
                get { 
                        return 0;
                } 
        }
}



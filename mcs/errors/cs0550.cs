// CS0550: `PropertyClass.PropertyInterface.Value.set' is an accessor not found in interface member `PropertyInterface.Value'
// Line: 13

interface PropertyInterface {
        int Value { get; }
}

public class PropertyClass: PropertyInterface {
        int PropertyInterface.Value { 
                get { 
                        return 0;
                } 
                set { }
        }
}



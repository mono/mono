// cs0550.cs: 'PropertyClass.PropertyInterface.Value.set' adds an accessor not found in interface member 'PropertyInterface.Value'
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



// cs3025.cs: CLS-compliant accessors must have the same accessibility as their property
// Line: 12

using System; 
[assembly: CLSCompliant (true)] 

public class Class {
	public int Property {
		protected get {
			return 5;
		}
		set {}
	}
}
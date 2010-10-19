// cs0144-3.cs: Cannot create an instance of the abstract class or interface `ITest'
// Line: 9
// Compiler options: -r:CS0144-3-lib.dll

public class SampleClass {
		public void Main ()
		{
			ITest modelo;
			modelo= new ITest ();
		}
}
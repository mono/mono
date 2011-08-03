// CS0102: The type `SampleClass' already contains a definition for `add_XX'
// Line: 13

public class SampleClass {
	public delegate void MyEvent ();
	public event MyEvent XX {
		add { }
		remove { }
	}
        
        bool add_XX;
}
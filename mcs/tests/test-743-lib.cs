// Compiler options: -t:library

public class A
{
	public int Prop {
		get {
			return 1;
		}
		private set { }
	}
	
	protected internal string this [int i] {
		private get {
			return null;
		}
		set { }
	}
}
// CS0535: `B' does not implement interface member `ITest2.GetName(string)'
// Line: 17

public interface ITest1 {
	void GetName(string id);
}

public interface ITest2 {
	void GetName(string id);
}

public class A : ITest1 {
	void ITest1.GetName(string id) {
	}
}

public class B : A, ITest2 {
}
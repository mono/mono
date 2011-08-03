// CS1574: XML comment on `A' has cref attribute `ExecuteSilently' that could not be resolved
// Line: 11
// Compiler options: -doc:dummy.xml -warnaserror
/// <summary />
public interface IExecutable {
	/// <summary />
	void ExecuteSilently ();
}

/// <summary>
/// <see cref="ExecuteSilently">this is not allowed</see>
/// </summary>
public class A : IExecutable {
	static void Main () {
	}

	/// <summary />
	void IExecutable.ExecuteSilently () {
	}
}


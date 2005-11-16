// cs1574-7.cs: XML comment on `A' has cref attribute `ExecuteSilently' that could not be resolved
// Compiler options: -doc:dummy.xml -warnaserror
// Line: 11
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


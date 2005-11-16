// Compiler options: -doc:xml-046.xml -warnaserror
/// <summary />
public interface IExecutable {
	/// <summary />
	void Execute ();

	/// <summary />
	object Current {
		get; 
	}
}

/// <summary>
/// <see cref="Execute" />
/// <see cref="Current" />
/// </summary>
public class A : IExecutable {
	static void Main () {
	}

	/// <summary />
	public void Execute () {
	}

	/// <summary />
	public object Current {
		get { return null; }
	}
}


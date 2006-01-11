// Compiler options: -doc:xml-050.xml -warnaserror
// see bug #76954.
// NOTE: It might got broken after some /doc related merge.
/// <summary>
/// <see cref="IB.Execute ()" />
/// <see cref="IB.Name" />
/// <see cref="B.Execute ()" />
/// <see cref="B.Name" />
/// </summary>
public class EntryPoint {
  static void Main () {
  }
}

/// <summary />
public interface IA {
  /// <summary />
  string Name {
    get;
  }

  /// <summary />
  string Execute ();
}

/// <summary />
public interface IB : IA {
  /// <summary />
  new int Name {
    get;
  }

  /// <summary />
  new int Execute ();
}

/// <summary />
public class A {
  /// <summary />
  public string Name {
    get { return null; }
  }

  /// <summary />
  public string Execute () {
    return null;
  }
}

/// <summary />
public class B : A {
  /// <summary />
  public new int Name {
    get { return 0; }
  }

  /// <summary />
  public new int Execute () {
    return 0;
  }
}

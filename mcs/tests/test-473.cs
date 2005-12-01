using System;

[Obsolete ("Use Errno", true)]
public enum Error {
	EROFS,
	ERANGE
}

[Obsolete ("Use Errno", true)]
public sealed class UnixMarshal {
	public static string GetDescription (Error e) {
		return null;
	}
}

public sealed class UnixMarshal2 {
	[Obsolete ("Use Errno", true)]
	public static string GetDescription (Error e) {
		return null;
	}
}

class Test {
	public static void Main () {
	}
}

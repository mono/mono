// Compiler options: -warnaserror

using System;

[assembly:CLSCompliant(true)]

namespace aa {
    public class I1 {
    }
}

namespace bb {
    public interface i1 {
    }
}

public class CLSClass {
        public static void Main() {}
}

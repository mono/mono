// warning CS1041: Identifier expected, 'true' is a keyword
// warning CS1041: Identifier expected, 'catch' is a keyword
// Compiler options: -warnaserror -doc:dummy.xml

/// <summary><see cref="true" />, <see cref="catch" /></summary>
public enum Test {
        /// <summary>Maybe</summary>
        @true,

        /// <summary>Maybe</summary>
        @catch
}

class Foo { static void Main () {} }

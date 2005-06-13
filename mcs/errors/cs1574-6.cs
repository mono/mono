// cs1574.cs: XML comment on 'Test' has cref attribute 'MyDelegate(int)' that could not be resolved in 'Test'.
// Line: 8
// Compiler options: -doc:dummy.xml -warnaserror -warn:1

/// <summary>
/// <see cref="MyDelegate(int)" />
/// </summary>
public class Test {
        /// <summary>
        /// whatever
        /// </summary>
        public delegate void MyDelegate(int i);
}


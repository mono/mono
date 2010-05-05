// CS0246: The type or namespace name `IBase' could not be found. Are you missing a using directive or an assembly reference?
// Line: 155555

namespace Foo {
        public interface IBase {
                object X { get; }
        }
}

public interface IDerived<T> : Foo.IBase {
        T X { get; }
}

public class Test<T> {
        public class Y : IDerived<T>, IBase
        {
                public T X { get { return default (T); } }
                object Foo.IBase.X {
                        get { return default (T); }
                }
        }
}

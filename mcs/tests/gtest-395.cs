public class RuleBuilder<T> where T : class {}

public interface IDynamicObject {
    RuleBuilder<T> GetRule<T>() where T : class;
}

public class RubyMethod : IDynamicObject {
    RuleBuilder<T> IDynamicObject.GetRule<T>() /* where T : class */ {
        return new RuleBuilder<T>();
    }
}

public class T {
	public static void Main ()
	{
	}
}

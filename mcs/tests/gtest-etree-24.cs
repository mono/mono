using System;
using System.Linq.Expressions;

public static class NotifyingProperty
{
	public static void CreateDependent<TValue> (
		Expression<Func<TValue>> property,
		Func<object> notifier,
		params Expression<Func<object>>[] dependents)
	{
	}
}

public class NotifyingPropertyTest
{
	public void CreateDependent_NotifierNull ()
	{
		int v = 0;
		NotifyingProperty.CreateDependent (() => v, null);
	}

	public void CreateDependent_DependentsNull ()
	{
		Expression<Func<object>>[] dependents = null;
		int v = 0;
		NotifyingProperty.CreateDependent (() => v, () => null, dependents);
	}

	public static void Main ()
	{
	}
}


using System;

public abstract class ThingWithOrganizationId
{
	public Guid OrganizationId;
}

public class Thing : ThingWithOrganizationId
{
}

public abstract class BaseService<TConstraint> 
{
	public abstract void Save<T> (T newThing) where T : TConstraint;
}

public class DerivedService:BaseService<Thing>
{
	public override void Save<TThing>(TThing newThing)
	{
		Console.WriteLine (newThing.OrganizationId);
	}

	static void Main ()
	{
	}
}
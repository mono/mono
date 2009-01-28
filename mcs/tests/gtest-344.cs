using System;

public abstract class ConfigurationElement
{
	protected ConfigurationElement ()
	{
	}
	
	public static void Main ()
	{
	}
}

public class CustomConfigurationElement : ConfigurationElement
{
}

public class CustomConfigurationElementCollection : BaseCollection<CustomConfigurationElement>
{
}

public class BaseCollection<T> where T : ConfigurationElement, new ()
{
}

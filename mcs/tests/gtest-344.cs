// Compiler options: /target:library

using System;

public abstract class ConfigurationElement
{
	protected ConfigurationElement ()
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

// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;

namespace NUnit.Mocks
{
	/// <summary>
	/// Summary description for MockSignature.
	/// </summary>
	public class MethodSignature
	{
		public readonly string typeName;
		public readonly string methodName;
		public readonly Type[] argTypes;

		public MethodSignature( string typeName, string methodName, Type[] argTypes )
		{
			this.typeName = typeName;
			this.methodName = methodName;
			this.argTypes = argTypes; 
		}

		public bool IsCompatibleWith( object[] args )
		{
			if ( args.Length != argTypes.Length )
				return false;

			for( int i = 0; i < args.Length; i++ )
				if ( !argTypes[i].IsAssignableFrom( args[i].GetType() ) )
					return false;

			return true;
		}

		public static Type[] GetArgTypes( object[] args )
		{
			if ( args == null )
				return new Type[0];

			Type[] argTypes = new Type[args.Length];
			for (int i = 0; i < argTypes.Length; ++i)
			{
				if (args[i] == null)
					argTypes[i] = typeof(object);
				else
					argTypes[i] = args[i].GetType();
			}

			return argTypes;
		}
	}
}

// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;
using System.Reflection;
using System.Collections;

namespace NUnit.Core
{
	/// <summary>
	/// Helper methods for inspecting a type by reflection. 
	/// 
	/// Many of these methods take a MemberInfo as an argument to avoid
	/// duplication, even though certain attributes can only appear on
	/// specific types of members, like MethodInfo or Type.
	/// 
	/// In the case where a type is being examined for the presence of
	/// an attribute, interface or named member, the Reflect methods
	/// operate with the full name of the member being sought. This
	/// removes the necessity of the caller having a reference to the
	/// assembly that defines the item being sought and allows the
	/// NUnit core to inspect assemblies that reference an older
	/// version of the NUnit framework.
	/// </summary>
	public class Reflect
	{
		#region Attributes 

		/// <summary>
		/// Check presence of attribute of a given type on a member.
		/// </summary>
		/// <param name="member">The member to examine</param>
		/// <param name="attrName">The FullName of the attribute type to look for</param>
		/// <param name="inherit">True to include inherited attributes</param>
		/// <returns>True if the attribute is present</returns>
		public static bool HasAttribute( MemberInfo member, string attrName, bool inherit )
		{
			object[] attributes = member.GetCustomAttributes( inherit );
			foreach( Attribute attribute in attributes )
				if ( IsInstanceOfType( attrName, attribute ) )
					return true;
			return false;
		}

        /// <summary>
        /// Get attribute of a given type on a member. If multiple attributes
        /// of a type are present, the first one found is returned.
        /// </summary>
        /// <param name="member">The member to examine</param>
        /// <param name="attrName">The FullName of the attribute type to look for</param>
        /// <param name="inherit">True to include inherited attributes</param>
        /// <returns>The attribute or null</returns>
        public static System.Attribute GetAttribute(MemberInfo member, string attrName, bool inherit)
        {
            object[] attributes = member.GetCustomAttributes(inherit);
            foreach (Attribute attribute in attributes)
                if ( IsInstanceOfType( attrName, attribute ) )
                    return attribute;
            return null;
        }

        /// <summary>
        /// Get attribute of a given type on an assembly. If multiple attributes
        /// of a type are present, the first one found is returned.
        /// </summary>
        /// <param name="assembly">The assembly to examine</param>
        /// <param name="attrName">The FullName of the attribute type to look for</param>
        /// <param name="inherit">True to include inherited attributes</param>
        /// <returns>The attribute or null</returns>
        public static System.Attribute GetAttribute(Assembly assembly, string attrName, bool inherit)
        {
            object[] attributes = assembly.GetCustomAttributes(inherit);
            foreach (Attribute attribute in attributes)
                if ( IsInstanceOfType(attrName, attribute) )
                    return attribute;
            return null;
        }

        /// <summary>
		/// Get all attributes of a given type on a member.
		/// </summary>
		/// <param name="member">The member to examine</param>
		/// <param name="attrName">The FullName of the attribute type to look for</param>
		/// <param name="inherit">True to include inherited attributes</param>
		/// <returns>The attribute or null</returns>
		public static System.Attribute[] GetAttributes( MemberInfo member, string attrName, bool inherit )
		{
			object[] attributes = member.GetCustomAttributes( inherit );
			ArrayList result = new ArrayList();
			foreach( Attribute attribute in attributes )
				if ( IsInstanceOfType( attrName, attribute ) )
					result.Add( attribute );
			return (System.Attribute[])result.ToArray( typeof( System.Attribute ) );
		}

        /// <summary>
        /// Get all attributes on a member.
        /// </summary>
        /// <param name="member">The member to examine</param>
        /// <param name="inherit">True to include inherited attributes</param>
        /// <returns>The attribute or null</returns>
        public static System.Attribute[] GetAttributes(MemberInfo member, bool inherit)
        {
            object[] attributes = member.GetCustomAttributes(inherit);
            System.Attribute[] result = new System.Attribute[attributes.Length];
            int n = 0;
            foreach (Attribute attribute in attributes)
                result[n++] = attribute;
            return result;
        }

        /// <summary>
        /// Get all attributes on an assembly.
        /// </summary>
        /// <param name="assembly">The assembly to examine</param>
        /// <param name="inherit">True to include inherited attributes</param>
        /// <returns>The attributes or null</returns>
        public static System.Attribute[] GetAttributes(Assembly assembly, bool inherit)
        {
            object[] attributes = assembly.GetCustomAttributes(inherit);
            System.Attribute[] result = new System.Attribute[attributes.Length];
            int n = 0;
            foreach (Attribute attribute in attributes)
                result[n++] = attribute;
            return result;
        }

        #endregion

		#region Interfaces

		/// <summary>
		/// Check to see if a type implements a named interface.
		/// </summary>
		/// <param name="fixtureType">The type to examine</param>
		/// <param name="interfaceName">The FullName of the interface to check for</param>
		/// <returns>True if the interface is implemented by the type</returns>
		public static bool HasInterface( Type fixtureType, string interfaceName )
		{
			foreach( Type type in fixtureType.GetInterfaces() )
				if ( type.FullName == interfaceName )
						return true;
			return false;
		}

		#endregion

		#region Inheritance
		//SHMARYA: [ 10/12/2005 ]
		/// <summary>
		/// Checks to see if a type inherits from a named type. 
		/// </summary>
		/// <param name="type">The type to examine</param>
		/// <param name="parentType">The FullName of the inherited type to look for</param>
		/// <returns>True if the type inherits from the named type.</returns>
		public static bool InheritsFrom( Type type, string typeName )
		{
			for( Type current = type; current != typeof( object ); current = current.BaseType )
				if( current.FullName == typeName )
					return true;

			return false;
		}

		public static bool InheritsFrom( object obj, string typeName )
		{
			return InheritsFrom( obj.GetType(), typeName );
		}

		public static bool IsInstanceOfType( string typeName, Attribute attr )
		{
			Type type = attr.GetType();
			return type.FullName == typeName || InheritsFrom( type, typeName );
		}
		#endregion

		#region Get Methods of a type

		/// <summary>
		/// Find the default constructor on a type
		/// </summary>
		/// <param name="fixtureType"></param>
		/// <returns></returns>
		public static ConstructorInfo GetConstructor( Type fixtureType )
		{
			return fixtureType.GetConstructor( Type.EmptyTypes );
		}

		/// <summary>
		/// Find the default constructor on a type
		/// </summary>
		/// <param name="fixtureType"></param>
		/// <returns></returns>
		public static ConstructorInfo GetConstructor( Type fixtureType, Type[] types )
		{
			return fixtureType.GetConstructor( types );
		}

		/// <summary>
		/// Examine a fixture type and return a method having a particular attribute.
		/// In the case of multiple methods, the first one found is returned.
		/// </summary>
		/// <param name="fixtureType">The type to examine</param>
		/// <param name="attributeName">The FullName of the attribute to look for</param>
		/// <param name="bindingFlags">BindingFlags to use in looking for method</param>
		/// <returns>A MethodInfo or null</returns>
		public static MethodInfo GetMethodWithAttribute( Type fixtureType, string attributeName, BindingFlags bindingFlags, bool inherit )
		{
			foreach(MethodInfo method in fixtureType.GetMethods( bindingFlags ) )
			{
				if( HasAttribute( method, attributeName, inherit ) ) 
					return method;
			}

			return null;
		}

		/// <summary>
		/// Examine a fixture type and return a count of the methods having a 
		/// particular attribute.
		/// </summary>
		/// <param name="fixtureType">The type to examine</param>
		/// <param name="attributeName">The FullName of the attribute to look for</param>
		/// <param name="bindingFlags">BindingFlags to use in looking for method</param>
		/// <returns>The number of such methods found</returns>
		public static int CountMethodsWithAttribute( Type fixtureType, string attributeName, BindingFlags bindingFlags, bool inherit )
		{
			int count = 0;

			foreach(MethodInfo method in fixtureType.GetMethods( bindingFlags ) )
			{
				if( HasAttribute( method, attributeName, inherit ) ) 
					count++;
			}

			return count;
		}

        /// <summary>
        /// Examine a fixture type and get a method with a particular name.
        /// In the case of overloads, the first one found is returned.
        /// </summary>
        /// <param name="fixtureType">The type to examine</param>
        /// <param name="methodName">The name of the method</param>
        /// <param name="bindingFlags">BindingFlags to use in the search</param>
        /// <returns>A MethodInfo or null</returns>
        public static MethodInfo GetNamedMethod(Type fixtureType, string methodName, BindingFlags bindingFlags)
        {
            foreach (MethodInfo method in fixtureType.GetMethods(bindingFlags))
            {
                if (method.Name == methodName)
                    return method;
            }

            return null;
        }

        /// <summary>
        /// Examine a fixture type and get a method with a particular name and list
        /// of arguments. In the case of overloads, the first one found is returned.
        /// </summary>
        /// <param name="fixtureType">The type to examine</param>
        /// <param name="methodName">The name of the method</param>
        /// <param name="argTypes">The full names of the argument types to search for</param>
        /// <param name="bindingFlags">BindingFlags to use in the search</param>
        /// <returns>A MethodInfo or null</returns>
        public static MethodInfo GetNamedMethod(Type fixtureType, string methodName, 
            string[] argTypes, BindingFlags bindingFlags)
        {
            foreach (MethodInfo method in fixtureType.GetMethods(bindingFlags))
            {
                if (method.Name == methodName)
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length == argTypes.Length)
                    {
                        bool match = true;
                        for (int i = 0; i < argTypes.Length; i++)
                            if (parameters[i].ParameterType.FullName != argTypes[i])
                            {
                                match = false;
                                break;
                            }

                        if (match)
                            return method;
                    }
                }
            }

            return null;
        }

        #endregion

		#region Get Properties of a type

		/// <summary>
		/// Examine a type and return a property having a particular attribute.
		/// In the case of multiple methods, the first one found is returned.
		/// </summary>
		/// <param name="fixtureType">The type to examine</param>
		/// <param name="attributeName">The FullName of the attribute to look for</param>
		/// <param name="bindingFlags">Binding flags to use in searching</param>
		/// <returns>A PropertyInfo or null</returns>
		public static PropertyInfo GetPropertyWithAttribute( Type fixtureType, string attributeName, BindingFlags bindingFlags )
		{
			foreach(PropertyInfo property in fixtureType.GetProperties( bindingFlags ) )
			{
				if( HasAttribute( property, attributeName, true ) ) 
					return property;
			}

			return null;
		}

		/// <summary>
		/// Examine a type and get a property with a particular name.
		/// In the case of overloads, the first one found is returned.
		/// </summary>
		/// <param name="type">The type to examine</param>
		/// <param name="bindingFlags">BindingFlags to use</param>
		/// <returns>A PropertyInfo or null</returns>
		public static PropertyInfo GetNamedProperty( Type type, string name, BindingFlags bindingFlags )
		{
			return type.GetProperty( name, bindingFlags );
		}

		/// <summary>
		/// Get the value of a named property on an object using binding flags of Public and Instance
		/// </summary>
		/// <param name="obj">The object for which the property value is needed</param>
		/// <param name="name">The name of a non-indexed property of the object</param>
		/// <returns></returns>
		public static object GetPropertyValue( object obj, string name )
		{
			return GetPropertyValue( obj, name, BindingFlags.Public | BindingFlags.Instance );
		}

		/// <summary>
		/// Get the value of a named property on an object
		/// </summary>
		/// <param name="obj">The object for which the property value is needed</param>
		/// <param name="name">The name of a non-indexed property of the object</param>
		/// <param name="bindingFlags">BindingFlags for use in determining which properties are needed</param>param>
		/// <returns></returns>
		public static object GetPropertyValue( object obj, string name, BindingFlags bindingFlags )
		{
			PropertyInfo property = GetNamedProperty( obj.GetType(), name, bindingFlags );
			if ( property != null )
				return property.GetValue( obj, null );
			return null;
		}

		/// <summary>
		/// Set the value of a named property on an object
		/// </summary>
		/// <param name="obj">The object for which the property value is to be set</param>
		/// <param name="name">The name of a non-indexed property of the object</param>
		/// <param name="val">The value to which the property is to be set</param>
		/// <param name="bindingFlags">BindingFlags for use in determining which properties are needed</param>param>
		public static void SetPropertyValue( object obj, string name, object val, BindingFlags bindingFlags )
		{
			PropertyInfo property = GetNamedProperty( obj.GetType(), name, bindingFlags );
			if ( property != null )
				property.SetValue( obj, val, null );
		}

		#endregion

		#region Invoke Methods

		/// <summary>
		/// Invoke the default constructor on a type
		/// </summary>
		/// <param name="type">The type to be constructed</param>
		/// <returns>An instance of the type</returns>
		public static object Construct( Type type )
		{
			ConstructorInfo ctor = GetConstructor( type );
			if ( ctor == null )
				throw new InvalidTestFixtureException(type.FullName + " does not have a valid constructor");
			
			return ctor.Invoke( Type.EmptyTypes );
		}

		/// <summary>
		/// Invoke a parameterless method returning void on an object.
		/// </summary>
		/// <param name="method">A MethodInfo for the method to be invoked</param>
		/// <param name="fixture">The object on which to invoke the method</param>
		public static void InvokeMethod( MethodInfo method, object fixture ) 
		{
			InvokeMethod( method, fixture, null );
		}

		/// <summary>
		/// Invoke a method returning void, converting any TargetInvocationException
		/// to an NUnitException
		/// </summary>
		/// <param name="method">A MethodInfo for the method to be invoked</param>
		/// <param name="fixture">The object on which to invoke the method</param>
		public static void InvokeMethod( MethodInfo method, object fixture, params object[] args )
		{
			if(method != null)
			{
				try
				{
					method.Invoke( fixture, args );
				}
				catch(TargetInvocationException e)
				{
					Exception inner = e.InnerException;
					throw new NUnitException("Rethrown",inner);
				}
			}
		}

		#endregion

		#region Private Constructor for static-only class

		private Reflect() { }

		#endregion
	}
}

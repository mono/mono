// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;

namespace NUnit.Framework
{
	/// <summary>
	/// PropertyAttribute is used to attach information to a test as a name/value pair..
	/// </summary>
	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method|AttributeTargets.Assembly, AllowMultiple=true)]
	public class PropertyAttribute : Attribute
	{
		/// <summary>
		/// The property name
		/// </summary>
		protected string propertyName;

		/// <summary>
		/// The property value
		/// </summary>
		protected object propertyValue;

		/// <summary>
		/// Construct a PropertyAttribute with a name and value
		/// </summary>
		/// <param name="propertyName">The name of the property</param>
		/// <param name="propertyValue">The property value</param>
		public PropertyAttribute( string propertyName, object propertyValue )
		{
			this.propertyName = propertyName;
			this.propertyValue = propertyValue;
		}

		/// <summary>
		/// Constructor for use by inherited classes that use the
		/// name of the type as the property name.
		/// </summary>
		protected PropertyAttribute( object propertyValue )
		{
			this.propertyName = this.GetType().Name;
			if ( propertyName.EndsWith( "Attribute" ) )
				propertyName = propertyName.Substring( 0, propertyName.Length - 9 );
			this.propertyValue = propertyValue;
		}

		/// <summary>
		/// Gets the property name
		/// </summary>
		public string Name
		{
			get { return propertyName; }
		}

		/// <summary>
		/// Gets the property value
		/// </summary>
		public object Value
		{
			get { return propertyValue; }
		}
	}
}

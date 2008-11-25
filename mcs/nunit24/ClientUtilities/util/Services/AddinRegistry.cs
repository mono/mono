// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;
using System.Collections;
using System.Reflection;
using NUnit.Core;
using NUnit.Core.Extensibility;

namespace NUnit.Util
{
	/// <summary>
	/// Summary description for AddinRegistry.
	/// </summary>
	public class AddinRegistry : MarshalByRefObject, IAddinRegistry, IService
    {
        #region Instance Fields
        private ArrayList addins = new ArrayList();
		#endregion

		#region IAddinRegistry Members

		public void Register(Addin addin)
		{
			addins.Add( addin );
		}

		public  IList Addins
		{
			get
			{
				return addins;
			}
		}

		public void SetStatus( string name, AddinStatus status, string message )
		{
			foreach( Addin addin in addins )
				if ( addin.Name == name )
				{
					addin.Status = status;
					addin.Message = message;
				}
		}
		#endregion

		#region IService Members
		public void InitializeService()
		{
		}

		public void UnloadService()
		{
		}
		#endregion
	}
}

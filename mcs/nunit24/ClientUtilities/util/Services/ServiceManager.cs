// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;
using System.Collections;
using NUnit.Core;

namespace NUnit.Util
{
	/// <summary>
	/// Summary description for ServiceManger.
	/// </summary>
	public class ServiceManager
	{
		private ArrayList services = new ArrayList();
		private Hashtable serviceIndex = new Hashtable();

		private static ServiceManager defaultServiceManager = new ServiceManager();

		public static ServiceManager Services
		{
			get { return defaultServiceManager; }
		}

		public void AddService( IService service )
		{
			services.Add( service );
			NTrace.Debug( "Added " + service.GetType().Name );
		}

		public IService GetService( Type serviceType )
		{
			IService theService = (IService)serviceIndex[serviceType];
			if ( theService == null )
				foreach( IService service in services )
				{
					// TODO: Does this work on Mono?
					if( serviceType.IsInstanceOfType( service ) )
					{
						serviceIndex[serviceType] = service;
						theService = service;
						break;
					}
				}

			if ( theService == null )
				NTrace.Error( string.Format( "Requested service {0} was not found", serviceType.FullName ) );
			else
				NTrace.Info( string.Format( "Request for service {0} satisfied by {1}", serviceType.Name, theService.GetType().Name ) );
			
			return theService;
		}

		public void InitializeServices()
		{
			foreach( IService service in services )
			{
				NTrace.Info( "Initializing " + service.GetType().Name );
				service.InitializeService();
			}
		}

		public void StopAllServices()
		{
			// Stop services in reverse of initialization order
			// TODO: Deal with dependencies explicitly
			int index = services.Count;
			while( --index >= 0 )
				((IService)services[index]).UnloadService();
		}

		public void ClearServices()
		{
			services.Clear();
		}

		private ServiceManager() { }
	}
}

//
// Mono.Data.SybaseClient.SybaseCommandBuilder.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Data;
using System.ComponentModel;

namespace Mono.Data.SybaseClient {
	public sealed class SybaseCommandBuilder : Component 
	{
		#region Constructors

		[MonoTODO]
		public SybaseCommandBuilder() 
		{
		}

		[MonoTODO]
		public SybaseCommandBuilder(SybaseDataAdapter adapter) 
		{
		}
		
		#endregion // Constructors

		#region Properties

		[MonoTODO]
		public SybaseDataAdapter DataAdapter {
			get { throw new NotImplementedException (); }
			set{ throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string QuotePrefix {
			get { throw new NotImplementedException (); } 
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string QuoteSuffix {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public static void DeriveParameters (SybaseCommand command) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void Dispose(bool disposing) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SybaseCommand GetDeleteCommand() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SybaseCommand GetInsertCommand() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SybaseCommand GetUpdateCommand() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RefreshSchema() 
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}


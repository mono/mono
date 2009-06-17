//
// MetaModel.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell Inc. http://novell.com
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web.Caching;
using System.Web.DynamicData.ModelProviders;

namespace System.Web.DynamicData
{
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class MetaModel
	{
		static object registered_models_lock = new object ();
		static object description_providers_lock = new object ();
		
		static MetaModel default_model;
		static Exception registration_exception;
		static Dictionary<Type, MetaModel> registered_models;
		static Dictionary<Type, TypeDescriptionProvider> description_providers;
		
		public static MetaModel Default {
			get { return default_model; }
			internal set { default_model = value; }
		}

		public static MetaModel GetModel (Type contextType)
		{
			if (contextType == null)
				throw new ArgumentNullException ("contextType");

			MetaModel m;
			if (registered_models.TryGetValue (contextType, out m))
				return m;
			throw new InvalidOperationException (String.Format ("Type '{0}' is not registered as a MetaModel", contextType));
		}

		public static void ResetRegistrationException ()
		{
			registration_exception = null;
		}

		public MetaModel ()
		{
			if (default_model == null)
				default_model = this;

			DynamicDataFolderVirtualPath = "~/DynamicData/";
			FieldTemplateFactory = new FieldTemplateFactory ();
			Tables = new ReadOnlyCollection<MetaTable> (new MetaTable [0]);
			VisibleTables = new List<MetaTable> ();
		}

		DataModelProvider provider;


		public string DynamicDataFolderVirtualPath { get; set; }

		public IFieldTemplateFactory FieldTemplateFactory { get; set; }

		public ReadOnlyCollection<MetaTable> Tables { get; private set; }

		public List<MetaTable> VisibleTables { get; private set; }

		void CheckRegistrationError ()
		{
			if (registration_exception != null)
				throw new InvalidOperationException ("An error occured during context model registration", registration_exception);
		}
		
		public string GetActionPath (string tableName, string action, object row)
		{
			return GetTable (tableName).GetActionPath (action, row);
		}

		public MetaTable GetTable (string uniqueTableName)
		{
			MetaTable mt;
			if (TryGetTable (uniqueTableName, out mt))
				return mt;
			throw new ArgumentException (String.Format ("Table '{0}' does not exist in registered context", uniqueTableName));
		}

		public MetaTable GetTable (Type entityType)
		{
			if (entityType == null)
				throw new ArgumentNullException ("entityType");
			if (provider != null)
				foreach (var t in Tables)
					if (t.EntityType == entityType)
						return t;
			throw new ArgumentException (String.Format ("Entity type '{0}' does not exist in registered context", entityType));
		}

		public MetaTable GetTable (string tableName, Type contextType)
		{
			return GetModel (contextType).GetTable (tableName);
		}

		internal static ICustomTypeDescriptor GetTypeDescriptor (Type type)
		{
			lock (description_providers_lock) {
				TypeDescriptionProvider provider;
				
				if (description_providers != null && description_providers.TryGetValue (type, out provider))
					return provider.GetTypeDescriptor (type);
			}

			return TypeDescriptor.GetProvider (type).GetTypeDescriptor (type);
		}
		

		public void RegisterContext (Func<object> contextFactory)
		{
			RegisterContext (contextFactory, null);
		}

		public void RegisterContext (Type contextType)
		{
			RegisterContext (contextType, null);
		}

		public void RegisterContext (DataModelProvider dataModelProvider)
		{
			RegisterContext (dataModelProvider, null);
		}

		public void RegisterContext (Type contextType, ContextConfiguration configuration)
		{
			if (contextType == null)
				throw new ArgumentNullException ("contextType");
			CheckRegistrationError ();
			RegisterContext (() => Activator.CreateInstance (contextType), configuration);
			RegisterModel (contextType, this);
			RegisterTypeDescriptionProvider (contextType, configuration);
		}

		public void RegisterContext (Func<object> contextFactory, ContextConfiguration configuration)
		{
			if (contextFactory == null)
				throw new ArgumentNullException ("contextFactory");
			CheckRegistrationError ();
			try {
				// FIXME: entity framework support is not done.
				RegisterContextCore (new DLinqDataModelProvider (contextFactory), configuration);
			} catch (Exception ex) {
				registration_exception = ex;
				throw;
			}
		}

		public void RegisterContext (DataModelProvider dataModelProvider, ContextConfiguration configuration)
		{
			if (dataModelProvider == null)
				throw new ArgumentNullException ("dataModelProvider");
			CheckRegistrationError ();
			try {
				RegisterContextCore (dataModelProvider, configuration);
			} catch (Exception ex) {
				registration_exception = ex;
				throw;
			}
		}

		void RegisterContextCore (DataModelProvider dataModelProvider, ContextConfiguration configuration)
		{
			var l = new List<MetaTable> ();
			foreach (var t in dataModelProvider.Tables)
				l.Add (new MetaTable (this, t, configuration != null && configuration.ScaffoldAllTables));
			Tables = new ReadOnlyCollection<MetaTable> (l);
			VisibleTables = l;
			RegisterModel (dataModelProvider.ContextType, this);
		}

		static void RegisterModel (Type contextType, MetaModel model)
		{
			lock (registered_models_lock) {
				if (registered_models == null)
					registered_models = new Dictionary <Type, MetaModel> ();

				if (registered_models.ContainsKey (contextType))
					return;

				registered_models.Add (contextType, model);
			}
		}

		static void RegisterTypeDescriptionProvider (Type type, ContextConfiguration config)
		{
			Func <Type, TypeDescriptionProvider> factory = config == null ? null : config.MetadataProviderFactory;
			if (config == null || factory == null)
				return;

			TypeDescriptionProvider provider = factory (type);
			if (provider == null)
				return;

			lock (description_providers) {
				if (description_providers == null)
					description_providers = new Dictionary <Type, TypeDescriptionProvider> ();

				if (description_providers.ContainsKey (type))
					return;

				description_providers.Add (type, provider);
			}
			
		}

		public bool TryGetTable (string uniqueTableName, out MetaTable table)
		{
			if (uniqueTableName == null)
				throw new ArgumentNullException ("uniqueTableName");
			foreach (var t in Tables)
				if (t.Name == uniqueTableName) {
					table = t;
					return true;
				}
			table = null;
			return false;
		}
	}
}

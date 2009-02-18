#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Stefan Klinger
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Reflection;

#if MONO_STRICT
using System.Data.Linq;
using DbLinq.Util;

#else
using DbLinq.Data.Linq;
using DbLinq.Data.Linq.Mapping;
using DbLinq.Util;

#endif

//Change notes:
//removed virtual init call from constructor
//renamed member variables to be better distinguishable from local variables

#if MONO_STRICT
namespace System.Data.Linq.Mapping
#else
namespace DbLinq.Data.Linq.Mapping
#endif
{
    /// <summary>
    /// This class is a stateless attribute meta model (it does not depend on any provider)
    /// So the MappingSource can use singletons
    /// </summary>
    [DebuggerDisplay("MetaModel for {DatabaseName}")]
    internal class AttributedMetaModel : MetaModel
	{
		private readonly Type _ContextType;

		/// <summary>
		/// The DataContext (or a derived type) that is used for this model.
		/// </summary>
		public override Type ContextType
		{
			get { return _ContextType; }
		}


		// just because of this, the whole model can not be cached efficiently, since we can not guarantee
		// that another mapping source instance will not use the same model
		private MappingSource _MappingSource;

		/// <summary>
		/// The mapping source used for that model.
		/// </summary>
		public override MappingSource MappingSource
		{
			get { return _MappingSource; }
		}


		private string _DatabaseName;

		/// <summary>
		/// Name of the database.
		/// </summary>
		/// <remarks>
		/// The name of the database is the type name of the DataContext inheriting class.
		/// If a plain DataContext is used, the database name is "DataContext".
		/// </remarks>
		public override string DatabaseName
		{
			get { return _DatabaseName; }
		}


		//Currently not implemented Properties
		public override Type ProviderType
		{
			get { throw new NotImplementedException(); }
		}

		//This function will try to add unknown table types
		//TODO: locking for multithreaded access, since it is not only used during init
		private IDictionary<Type, MetaTable> _Tables = new Dictionary<Type, MetaTable>();

		private IDictionary<MethodInfo, MetaFunction> _metaFunctions;
		

		/// <summary>
		/// Initializes a new instance of the <see cref="AttributedMetaModel"/> class.
		/// </summary>
		/// <param name="contextType">DataContext type used.</param>
		/// <param name="mappingSource">The mapping source.</param>
        public AttributedMetaModel(Type contextType, MappingSource mappingSource)
        {
            _ContextType = contextType;
            _MappingSource = mappingSource;

			DiscoverDatabaseName();

			FindTables();
            
			//Load looks a bit useles since it is only called here
			Load(); //TODO refactor this method
        }

        private void Load()
        {
            // stored procedures
            _metaFunctions = new Dictionary<MethodInfo, MetaFunction>();
            var functionAttributes = GetFunctionsAttributes();
            foreach (var functionPair in functionAttributes)
            {
                _metaFunctions[functionPair.Key] = new AttributedMetaFunction(functionPair.Key, functionPair.Value);
            }
        }

        private IDictionary<MethodInfo, FunctionAttribute> GetFunctionsAttributes()
        {
            var functionAttributes = new Dictionary<MethodInfo, FunctionAttribute>();
            foreach (var methodInfo in _ContextType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                var function = ReflectionExtensions.GetAttribute<FunctionAttribute>(methodInfo);
                if (function != null)
                    functionAttributes[methodInfo] = function;
            }
            return functionAttributes;
        }

		/// <summary>
		/// Gets the <see cref="MetaFunction"/> for the given MethodInfo.
		/// </summary>
		/// <param name="method">The method info for which the <see cref="MetaFunction"/> should be returned.</param>
        public override MetaFunction GetFunction(MethodInfo method)
        {
            MetaFunction metaFunction;
            _metaFunctions.TryGetValue(method, out metaFunction);
            return metaFunction;
        }

		/// <summary>
		/// Returns an enumeration of all mapped functions.
		/// </summary>
        public override IEnumerable<MetaFunction> GetFunctions()
        {
            return _metaFunctions.Values;
        }

        public override MetaType GetMetaType(Type type)
        {
            var metaTable = GetTable(type);
            if (metaTable == null)
                return null;
            return metaTable.RowType;
        }

		/// <summary>
		/// Returns the <see cref="MetaTable"/> for the given table type.
		/// </summary>
		/// <remarks>
		/// If the given type is not allready mapped it tries to map it.
		/// </remarks>
		/// <param name="tableType"><see cref="MetaTable"/> for the table type or null if not mappable.</param>
		public override MetaTable GetTable(Type tableType)
		{
			MetaTable metaTable;
			_Tables.TryGetValue(tableType, out metaTable);
			if (metaTable != null)
			{
				return metaTable;
			}

			return AddTableType(tableType);
		}

		/// <summary>
		/// Returns an enumeration of all mapped tables.
		/// </summary>
        public override IEnumerable<MetaTable> GetTables()
        {
            return _Tables.Values;
		}

		/// <summary>
		/// Tries to discover the name of the database.
		/// Database name == class name of the DataContext's most derived class used for this MetaModel.
		/// </summary>
		private void DiscoverDatabaseName()
		{
			var databaseAttribute = _ContextType.GetAttribute<DatabaseAttribute>();
			if (databaseAttribute != null)
			{
				_DatabaseName = databaseAttribute.Name;
			}
			else //Found no DatabaseAttribute get the class name
			{
				_DatabaseName = _ContextType.Name;
			}
		}

		//Discover all the tables used with this context, used for the GetTable/GetTables function
		//Behaviour of GetTables in the Framework: STRANGE
		//If the DataContext was a strong typed one (derived with fields for the tables),
		//it returns a list of MetaTables for all this tables.
		//But if you call GetTable<T> with an additional table - the table doesn't get added to this list.
		//If you use a vanilla DataContext the list is empty at the beginning (ok no surprise here),
		//if you call GetTable<T> here the table is added to the list.
		//
		//If you add to properties with the same T of Table<T> only the first gets into the list.
		private void FindTables()
		{
			MemberInfo[] memberInfos = _ContextType.GetMembers(BindingFlags.GetField
					| BindingFlags.GetProperty | BindingFlags.Static | BindingFlags.Instance
					| BindingFlags.NonPublic | BindingFlags.Public);

			//foreach (var info in memberInfos)
			for (int i = 0; i < memberInfos.Length; ++i )
			{
				var info = memberInfos[i];

				Type memberType = info.GetMemberType();

				if (memberType == null)
				{
					continue;
				}

				//Ok first possible problem here - there seems to be the .net ITable/Table and the local one
				//Same goes for the attribute types
				//Any reason for that?
				//looking for a table generic
				if (memberType.IsGenericType)
				{
					if (memberType.GetGenericTypeDefinition() != typeof(Table<>))
					{
						continue;
					}

					Type argumentType = memberType.GetGenericArguments()[0];

					//If the argument type is a generic parameter we are not interested
					//Most likly it is the GetTable function
					if (argumentType.IsGenericParameter)
					{
						continue;
					}

					AddTableType(argumentType);
				}
			}
		}

		/// <summary>
		/// Adds the table of the given type to the mappings.
		/// </summary>
		/// <remarks>
		/// The given type must have a <see cref="TableAttribute" /> to be mappable.
		/// </remarks>
		/// <param name="tableType">Type of the table.</param>
		/// <returns>
		/// Returns the <see cref="MetaTable"/> for the given table type or null if it is not mappable.
		/// </returns>
		private MetaTable AddTableType(Type tableType)
		{
			//No need to check base types because framework implementation doesn't do this either
			var tableAttribute = tableType.GetAttribute<TableAttribute>();

			if (tableAttribute == null)
			{
				return null;
			}

			//First set up the table without associations
			var metaType = new AttributedMetaType(tableType);
			var metaTable = new AttributedMetaTable(tableAttribute, metaType, this);
			metaType.SetMetaTable(metaTable);
			_Tables[tableType] = metaTable;

			//After that we are ready to setup table associations, need to to this late
			//because of possible circular dependencies
			//In worst case if SetupAssociations throws an exception we end up with a table
			//without complete association information, but this seems to be and ok tradeoff
			metaType.SetupAssociations();

			return metaTable;
		}
	}
}
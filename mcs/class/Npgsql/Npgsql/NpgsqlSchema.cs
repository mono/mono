// Npgsql.NpgsqlCommand.cs
//
// Author:
//  Josh Cooley <jbnpgsql@tuxinthebox.net>
//
//  Copyright (C) 2002-2005 The Npgsql Development Team
//  npgsql-general@gborg.postgresql.org
//  http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA


using System;
using System.Data;
using System.Text;

namespace Npgsql
{
	/// <summary>
	/// Provides the underlying mechanism for reading schema information.
	/// </summary>
	internal sealed class NpgsqlSchema
	{	
        private NpgsqlConnection _connection;

        /// <summary>
        /// Creates an NpgsqlSchema that can read schema information from the database.
        /// </summary>
        /// <param name="connection">An open database connection for reading metadata.</param>
        internal NpgsqlSchema(NpgsqlConnection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// Returns the MetaDataCollections that lists all possible collections.
        /// </summary>
        /// <returns>The MetaDataCollections</returns>
        internal static DataTable GetMetaDataCollections()
        {
            DataTable metaDataCollections = new DataTable("MetaDataCollections");

            metaDataCollections.Columns.AddRange(new DataColumn[] {
                                                                      new DataColumn("CollectionName"),
                                                                      new DataColumn("NumberOfRestrictions", typeof(int)),
                                                                      new DataColumn("NumberOfIdentifierParts", typeof(int)) });

            // Add(object[] { CollectionName, NumberOfRestrictions, NumberOfIdentifierParts })
            metaDataCollections.Rows.Add(new object[]{"MetaDataCollections",0,0});
            metaDataCollections.Rows.Add(new object[]{"Restrictions",0,0});
            metaDataCollections.Rows.Add(new object[]{"Databases",1,1});
            metaDataCollections.Rows.Add(new object[]{"Tables",4,3});
            metaDataCollections.Rows.Add(new object[]{"Columns",4,4});
            metaDataCollections.Rows.Add(new object[]{"Views",3,3});
            metaDataCollections.Rows.Add(new object[]{"Users",1,1});

            return metaDataCollections;
        }

        /// <summary>
        /// Returns the Restrictions that contains the meaning and position of the values in the restrictions array.
        /// </summary>
        /// <returns>The Restrictions</returns>
        internal static DataTable GetRestrictions()
        {
            DataTable restrictions = new DataTable("Restrictions");

            restrictions.Columns.AddRange(new DataColumn[] {
                                                               new DataColumn("CollectionName"),
                                                               new DataColumn("RestrictionName"),
                                                               new DataColumn("RestrictionDefault"),
                                                               new DataColumn("RestrictionNumber", typeof(int)) });

            restrictions.Rows.Add(new object[]{"Databases","Name","Name",1});
            restrictions.Rows.Add(new object[]{"Tables","Catalog","table_catalog",1});
            restrictions.Rows.Add(new object[]{"Tables","Schema","table_schema",2});
            restrictions.Rows.Add(new object[]{"Tables","Table","table_name",3});
            restrictions.Rows.Add(new object[]{"Tables","TableType","table_type",4});
            restrictions.Rows.Add(new object[]{"Columns","Catalog","table_catalog",1});
            restrictions.Rows.Add(new object[]{"Columns","Schema","table_schema",2});
            restrictions.Rows.Add(new object[]{"Columns","Table","table_name",3});
            restrictions.Rows.Add(new object[]{"Columns","Column","column_name",4});
            restrictions.Rows.Add(new object[]{"Views","Catalog","table_catalog",1});
            restrictions.Rows.Add(new object[]{"Views","Schema","table_schema",2});
            restrictions.Rows.Add(new object[]{"Views","Table","table_name",3});

            return restrictions;
        }

        private NpgsqlCommand BuildCommand(StringBuilder query, string[] restrictions, params string[] names)
        {
            NpgsqlCommand command = new NpgsqlCommand();

            if (restrictions != null && names != null)
            {
                bool addWhere = true;
                for(int i=0; i<restrictions.Length && i<names.Length; ++i)
                {
                    if (restrictions[i] != null && restrictions[i].Length != 0)
                    {
                        if (addWhere)
                        {
                            query.Append(" WHERE ");
                            addWhere = false;
                        }
                        else
                        {
                            query.Append(" AND ");
                        }
                        query.AppendFormat("{0} = :{0}", names[i]);

                        command.Parameters.Add(new NpgsqlParameter(names[i], restrictions[i]));
                    }
                }
            }
            command.CommandText = query.ToString();
            command.Connection = _connection;

            return command;
        }

        /// <summary>
        /// Returns the Databases that contains a list of all accessable databases.
        /// </summary>
        /// <param name="restrictions">The restrictions to filter the collection.</param>
        /// <returns>The Databases</returns>
        internal DataTable GetDatabases(string[] restrictions)
        {
            DataTable databases = new DataTable("Databases");

            databases.Columns.AddRange(new DataColumn[] {
                                                            new DataColumn("database_name"),
                                                            new DataColumn("owner"),
                                                            new DataColumn("encoding") });

            StringBuilder getDatabases = new StringBuilder();

            getDatabases.Append("SELECT d.datname AS database_name, u.usename AS owner, pg_catalog.pg_encoding_to_char(d.encoding) AS encoding FROM pg_catalog.pg_database d LEFT JOIN pg_catalog.pg_user u ON d.datdba = u.usesysid");

            using (NpgsqlCommand command = BuildCommand(getDatabases, restrictions, "datname"))
            using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command))
            {
                adapter.Fill(databases);
            }

            return databases;
        }

        /// <summary>
        /// Returns the Tables that contains table and view names and the database and schema they come from.
        /// </summary>
        /// <param name="restrictions">The restrictions to filter the collection.</param>
        /// <returns>The Tables</returns>
        internal DataTable GetTables(string[] restrictions)
        {
            DataTable tables = new DataTable("Tables");

            tables.Columns.AddRange(new DataColumn[] {
                                                         new DataColumn("table_catalog"),
                                                         new DataColumn("table_schema"),
                                                         new DataColumn("table_name"),
                                                         new DataColumn("table_type") });

            StringBuilder getTables = new StringBuilder();

            getTables.Append("SELECT table_catalog, table_schema, table_name, table_type FROM information_schema.tables");

            using (NpgsqlCommand command = BuildCommand(getTables, restrictions, "table_catalog", "table_schema", "table_name", "table_type"))
            using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command))
            {
                adapter.Fill(tables);
            }

            return tables;
        }

        /// <summary>
        /// Returns the Columns that contains information about columns in tables. 
        /// </summary>
        /// <param name="restrictions">The restrictions to filter the collection.</param>
        /// <returns>The Columns.</returns>
        internal DataTable GetColumns(string[] restrictions)
        {
            DataTable columns = new DataTable("Columns");

            columns.Columns.AddRange(new DataColumn[] {
                                                          new DataColumn("table_catalog"),
                                                          new DataColumn("table_schema"),
                                                          new DataColumn("table_name"),
                                                          new DataColumn("column_name"),
                                                          new DataColumn("ordinal_position", typeof(int)),
                                                          new DataColumn("column_default"),
                                                          new DataColumn("is_nullable"),
                                                          new DataColumn("data_type"),
                                                          new DataColumn("character_maximum_length", typeof(int)),
                                                          new DataColumn("character_octet_length", typeof(int)),
                                                          new DataColumn("numeric_precision", typeof(int)),
                                                          new DataColumn("numeric_precision_radix", typeof(int)),
                                                          new DataColumn("numeric_scale", typeof(int)),
                                                          new DataColumn("datetime_precision", typeof(int)),
                                                          new DataColumn("character_set_catalog"),
                                                          new DataColumn("character_set_schema"),
                                                          new DataColumn("character_set_name"),
                                                          new DataColumn("collation_catalog") });

            StringBuilder getColumns = new StringBuilder();

            getColumns.Append("SELECT table_catalog, table_schema, table_name, column_name, ordinal_position, column_default, is_nullable, udt_name AS data_type, character_maximum_length, character_octet_length, numeric_precision, numeric_precision_radix, numeric_scale, datetime_precision, character_set_catalog, character_set_schema, character_set_name, collation_catalog FROM information_schema.columns");

            using (NpgsqlCommand command = BuildCommand(getColumns, restrictions, "table_catalog", "table_schema", "table_name", "column_name"))
            using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command))
            {
                adapter.Fill(columns);
            }

            return columns;
        }

        /// <summary>
        /// Returns the Views that contains view names and the database and schema they come from.
        /// </summary>
        /// <param name="restrictions">The restrictions to filter the collection.</param>
        /// <returns>The Views</returns>
        internal DataTable GetViews(string[] restrictions)
        {
            DataTable views = new DataTable("Views");

            views.Columns.AddRange(new DataColumn[] {
                                                        new DataColumn("table_catalog"),
                                                        new DataColumn("table_schema"),
                                                        new DataColumn("table_name"),
                                                        new DataColumn("check_option"),
                                                        new DataColumn("is_updatable") });

            StringBuilder getViews = new StringBuilder();

            getViews.Append("SELECT table_catalog, table_schema, table_name, check_option, is_updatable FROM information_schema.views");

            using (NpgsqlCommand command = BuildCommand(getViews, restrictions, "table_catalog", "table_schema", "table_name"))
            using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command))
            {
                adapter.Fill(views);
            }

            return views;
        }

        /// <summary>
        /// Returns the Users containing user names and the sysid of those users.
        /// </summary>
        /// <param name="restrictions">The restrictions to filter the collection.</param>
        /// <returns>The Users.</returns>
        internal DataTable GetUsers(string[] restrictions)
        {
            DataTable users = new DataTable("Users");

            users.Columns.AddRange(new DataColumn[] {
                                                        new DataColumn("user_name"),
                                                        new DataColumn("user_sysid", typeof(int)) });

            StringBuilder getUsers = new StringBuilder();

            getUsers.Append("SELECT usename as user_name, usesysid as user_sysid FROM pg_catalog.pg_user");

            using (NpgsqlCommand command = BuildCommand(getUsers, restrictions, "usename"))
            using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command))
            {
                adapter.Fill(users);
            }

            return users;
        }
	}
}

#if ENTITIES
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Xml;
using System.Data.Common.CommandTrees;
using System.Data.Metadata.Edm;
using Npgsql.SqlGenerators;

namespace Npgsql
{
    internal class NpgsqlServices : DbProviderServices
    {
        private static readonly NpgsqlServices _instance = new NpgsqlServices();

        public static NpgsqlServices Instance
        {
            get { return _instance; }
        }

        protected override DbCommandDefinition CreateDbCommandDefinition(DbProviderManifest providerManifest, DbCommandTree commandTree)
        {
            return CreateCommandDefinition(CreateDbCommand(commandTree));
        }

        private DbCommand CreateDbCommand(DbCommandTree commandTree)
        {
            if (commandTree == null)
                throw new ArgumentNullException("commandTree");

            DbCommand command = NpgsqlFactory.Instance.CreateCommand();

            foreach (KeyValuePair<string, TypeUsage> parameter in commandTree.Parameters)
            {
                DbParameter dbParameter = command.CreateParameter();
                dbParameter.ParameterName = parameter.Key;
                command.Parameters.Add(dbParameter);
            }

            TranslateCommandTree(commandTree, command);

            return command;
        }

        private void TranslateCommandTree(DbCommandTree commandTree, DbCommand command)
        {
            SqlBaseGenerator sqlGenerator = null;

            DbQueryCommandTree select;
            DbInsertCommandTree insert;
            DbUpdateCommandTree update;
            DbDeleteCommandTree delete;
            if ((select = commandTree as DbQueryCommandTree) != null)
            {
                sqlGenerator = new SqlSelectGenerator(select);
            }
            else if ((insert = commandTree as DbInsertCommandTree) != null)
            {
                sqlGenerator = new SqlInsertGenerator(insert);
            }
            else if ((update = commandTree as DbUpdateCommandTree) != null)
            {
                sqlGenerator = new SqlUpdateGenerator(update);
            }
            else if ((delete = commandTree as DbDeleteCommandTree) != null)
            {
                sqlGenerator = new SqlDeleteGenerator(delete);
            }
            else
            {
                // TODO: get a message (unsupported DbCommandTree type)
                throw new ArgumentException();
            }

            sqlGenerator.BuildCommand(command);
        }

        protected override string GetDbProviderManifestToken(DbConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");
            // TODO: used to use connection.ServerVersion
            // that doesn't work with a closed connection.
            bool openedConnection = false;
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                    openedConnection = true;
                }
                return connection.ServerVersion;
            }
            finally
            {
                if (openedConnection)
                {
                    connection.Close();
                }
            }
        }

        protected override DbProviderManifest GetDbProviderManifest(string versionHint)
        {
            if (versionHint == null)
                throw new ArgumentNullException("versionHint");
            return new NpgsqlProviderManifest(versionHint);
        }
    }
}

#endif
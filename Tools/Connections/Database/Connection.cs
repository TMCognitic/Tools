using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Tools.Connections.Database
{
    public class Connection : IConnection
    {
        private readonly DbProviderFactory _providerFactory;
        private readonly string _connectionString;

        public Connection(DbProviderFactory providerFactory, string connectionString)
        {
            if (providerFactory is null)
                throw new ArgumentNullException(nameof(providerFactory));

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Invalid connection string...", nameof(connectionString));

            _providerFactory = providerFactory;
            _connectionString = connectionString;

            using (DbConnection dbConnection = CreateConnection())
            {
                dbConnection.Open();
            }
        }

        public int ExecuteNonQuery(Command command)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            using (DbConnection dbConnection = CreateConnection())
            {
                using (DbCommand dbCommand = CreateCommand(command, dbConnection))
                {
                    dbConnection.Open();

                    return dbCommand.ExecuteNonQuery();
                }
            }
        }

        public IEnumerable<TResult> ExecuteReader<TResult>(Command command, Func<IDataRecord, TResult> selector)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (selector is null)
                throw new ArgumentNullException(nameof(selector));

            using (DbConnection dbConnection = CreateConnection())
            {
                using (DbCommand dbCommand = CreateCommand(command, dbConnection))
                {
                    dbConnection.Open();

                    using (DbDataReader dbDataReader = dbCommand.ExecuteReader())
                    {
                        while (dbDataReader.Read())
                        {
                            yield return selector(dbDataReader);
                        }
                    }
                }
            }
        }

        public object ExecuteScalar(Command command)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            using (DbConnection dbConnection = CreateConnection())
            {
                using (DbCommand dbCommand = CreateCommand(command, dbConnection))
                {
                    dbConnection.Open();

                    object o = dbCommand.ExecuteNonQuery();
                    return o is DBNull ? null : o;
                }
            }
        }

        public DataSet GetDataSet(Command command)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            using (DbConnection dbConnection = CreateConnection())
            {
                using (DbCommand dbCommand = CreateCommand(command, dbConnection))
                {
                    using (DbDataAdapter dbDataAdapter = _providerFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.SelectCommand = dbCommand;
                        DataSet dataSet = new DataSet();
                        dbDataAdapter.Fill(dataSet);

                        return dataSet;
                    }
                }
            }
        }

        public DataTable GetDataTable(Command command)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            using (DbConnection dbConnection = CreateConnection())
            {
                using (DbCommand dbCommand = CreateCommand(command, dbConnection))
                {
                    using (DbDataAdapter dbDataAdapter = _providerFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.SelectCommand = dbCommand;
                        DataTable dataTable = new DataTable();
                        dbDataAdapter.Fill(dataTable);

                        return dataTable;
                    }
                }
            }
        }

        private DbConnection CreateConnection()
        {
            DbConnection dbConnection = _providerFactory.CreateConnection();
            dbConnection.ConnectionString = _connectionString;

            return dbConnection;
        }


        private static DbCommand CreateCommand(Command command, DbConnection dbConnection)
        {
            DbCommand dbCommand = dbConnection.CreateCommand();
            dbCommand.CommandText = command.Query;

            if (command.IsStoredProcedure)
                dbCommand.CommandType = CommandType.StoredProcedure;

            foreach (KeyValuePair<string, object> item in command.Parameters)
            {
                DbParameter dbParameter = dbCommand.CreateParameter();
                dbParameter.ParameterName = item.Key;
                dbParameter.Value = item.Value;

                dbCommand.Parameters.Add(dbParameter);
            }

            return dbCommand;
        }
    }
}

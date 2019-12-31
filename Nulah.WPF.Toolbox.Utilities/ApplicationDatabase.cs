using System;
using System.Collections.Generic;
using Nulah.Data.Sqlite;
using Nulah.Data.Sqlite.Models;

namespace Nulah.WPF.Toolbox.Utilities
{
    public class ApplicationDatabase
    {
        private Database _database;
        public ApplicationDatabase()
        {
            _database = new Database("app");
        }

        public void CreateTable<T>()
        {
            _database.CreateTable<T>();
        }

        public void Insert<T>(T value)
        {
            _database.Insert<T>(value);
        }

        public IEnumerable<T> Query<T>(string query, Dictionary<string, object> parameters = null)
        {
            return _database.Query<T>(query, parameters);
        }

        public void Update<T>(string query, Dictionary<string, object> parameters = null)
        {
            _database.Update<T>(query, parameters);
        }

        public void Delete<T>(string deleteQuery, Dictionary<string, object> parameters = null)
        {
            _database.Delete<T>(deleteQuery, parameters);
        }

        /// <summary>
        /// Returns the table name for the given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public string GetTableName<T>()
        {
            return _database.DatabaseTableName<T>();
        }

        /// <summary>
        /// Returns column mapping information for the given table mapped type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Dictionary<string, SqliteMapping> GetColumnMappings<T>()
        {
            return _database.GetColumnMappingsForType<T>();
        }

        /// <summary>
        /// Uses the serialiser for the given column on the given table type
        /// </summary>
        /// <param name="tableClassInstance"></param>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public object UseTableColumnSerialiser(object tableClassInstance, SqliteMapping column, object value)
        {
            return _database.SerialiseValue(tableClassInstance, column.Serialiser, value);
        }

    }
}

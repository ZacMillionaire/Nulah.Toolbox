using System;
using System.Collections.Generic;
using Nulah.Data.Sqlite;

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

        public IEnumerable<T> Query<T>(string query, Dictionary<string, object> parameters)
        {
            return _database.Query<T>(query, parameters);
        }
    }
}

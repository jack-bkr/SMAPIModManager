using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Sqlite;
using Tmds.DBus.Protocol;

namespace SMAPIModManager;

public class DBConnector
{
    public static List<List<String>> SendSQL(string sql)
    {
        List<List<String>> result = new List<List<String>>();
        
        using (var connection = new SqliteConnection("Data Source=DB.db")) // define location of database
        {
            connection.Open();

            var command = connection.CreateCommand(); // assign SQL command to connection
            command.CommandText = sql;

            using (var reader = command.ExecuteReader()) // execute SQL command
            {
                
                while (reader.Read()) // extract data from SQL command
                {
                    List<String> row = new List<String>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row.Add(reader.GetString(i));
                    }
                    result.Add(row);
                }
            }
            connection.Close();
        }
        return result;
    }

    public static bool SendDML(string sql)
    {
        using (var connection = new SqliteConnection("Data Source=DB.db")) // define location of database
        {
            connection.Open();

            var command = connection.CreateCommand(); // assign SQL command to connection
            command.CommandText = sql;

            command.ExecuteNonQuery(); // execute SQL command

            connection.Close();
        }
        return true;
    }
}
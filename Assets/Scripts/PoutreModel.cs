using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;

class PoutreModel
{
    private static string _connectionString;

    public PoutreModel(string connectionString)
    {
        _connectionString = connectionString;
    }

    // récupération de toutes les poutre IPE
    public List<String> GetAllIPE()
    {
        List<String> result = new List<String>();

        using (IDbConnection dbConnection = new SqliteConnection(_connectionString))
        {
            dbConnection.Open();
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string SQLQuery = "SELECT name FROM PoutreI";
                dbCmd.CommandText = SQLQuery;

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetString(0));
                    }
                    dbConnection.Close();
                    reader.Close();
                }
            }
        }
        return result;
    }

    // récupération de toute les poutres de sections carré
    public List<String> GetAllsquare()
    {
        List<String> result = new List<String>();

        using (IDbConnection dbConnection = new SqliteConnection(_connectionString))
        {
            dbConnection.Open();
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string SQLQuery = "SELECT name FROM PoutreCarre";
                dbCmd.CommandText = SQLQuery;

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetString(0));
                    }
                    dbConnection.Close();
                    reader.Close();
                }
            }
        }
        return result;
    }

    // recuperation des poutres IPE par nom
    public List<float> GetIPEByName(string name)
    {
        List<float> result = new List<float>();

        using (IDbConnection dbConnection = new SqliteConnection(_connectionString))
        {
            dbConnection.Open();
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string SQLQuery = String.Format("SELECT h, b, tw, tf FROM PoutreI WHERE name = \"{0}\"", name);

                dbCmd.CommandText = SQLQuery;

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetFloat(0));
                        result.Add(reader.GetFloat(1));
                        result.Add(reader.GetFloat(2));
                        result.Add(reader.GetFloat(3));
                    }
                    dbConnection.Close();
                    reader.Close();
                }
            }
        }
        return result;
    }

    // recherche d'une poutre par son nom
    public List<float> GetSquareByName(string name)
    {
        List<float> result = new List<float>();

        using (IDbConnection dbConnection = new SqliteConnection(_connectionString))
        {
            dbConnection.Open();
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string SQLQuery = String.Format("SELECT size FROM PoutreCarre WHERE name = \"{0}\"", name);

                dbCmd.CommandText = SQLQuery;

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetFloat(0));
                    }
                    dbConnection.Close();
                    reader.Close();
                }
            }
        }
        return result;
    }
}

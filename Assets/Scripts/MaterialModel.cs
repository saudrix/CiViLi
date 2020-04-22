using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;

class MaterialModel
{
    private static string _connectionString;

    public MaterialModel(string connectionString)
    {
        _connectionString = connectionString;
    }

    // permet de récuperer le nom de tous les matériaux
    public List<string> GetAll()
    {
        List<string> result = new List<string>();
        using (IDbConnection dbConnection = new SqliteConnection(_connectionString))
        {
            dbConnection.Open();
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string SQLQuery = "SELECT name FROM materials";
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

    // permet de récuperer toutes les données d'un matériaux avec son nom
    public List<float> GetAllDataByName(string name) 
    {
        List<float> result = new List<float>();
        using (IDbConnection dbConnection = new SqliteConnection(_connectionString))
        {
            dbConnection.Open();
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string SQLQuery = String.Format("SELECT youngModule, sigmaMax, alpha FROM materials WHERE name = \"{0}\"", name);
                dbCmd.CommandText = SQLQuery;

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetFloat(0));
                        result.Add(reader.GetFloat(1));
                        result.Add(reader.GetFloat(2));
                    }
                    dbConnection.Close();
                    reader.Close();
                }
            }
        }
        return result;
    }

    // permet de récupérer la masse volumique d'un élément avec son nom
    public float GetRoByName(string name)
    {
        float result = 0;
        using (IDbConnection dbConnection = new SqliteConnection(_connectionString))
        {
            dbConnection.Open();
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string SQLQuery = String.Format("SELECT volumicMass FROM materials WHERE name = \"{0}\"", name);
                dbCmd.CommandText = SQLQuery;
                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result = reader.GetFloat(0);
                    }
                    dbConnection.Close();
                    reader.Close();
                }
            }
        }
        return result;
    }

    // permet de récuperer le module de young d'un matériaux avec son nom
    public float GetYoungByName(string name)
    {
        float result = 0;
        using (IDbConnection dbConnection = new SqliteConnection(_connectionString))
        {
            dbConnection.Open();
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string SQLQuery = String.Format("SELECT youngModule FROM materials WHERE name = \"{0}\"", name);
                dbCmd.CommandText = SQLQuery;
                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result = reader.GetFloat(0);
                    }
                    dbConnection.Close();
                    reader.Close();
                }
            }
        }
        return result;
    }
}


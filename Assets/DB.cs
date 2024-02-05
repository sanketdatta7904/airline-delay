using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;

public class DB : MonoBehaviour
{
    private string dbName = "URI=file:Aviation111.db";
    // Start is called before the first frame update
    void Start()
    {
        // CreateDB(); // create table if it doesn't exist already
        DisplayData();
    }

    public void CreateDB()
    {
        using (var connection = new SqliteConnection(dbName)) 
        {
            connection.Open();
        }
    }

        public void DisplayData()
            {
                using (var connection = new SqliteConnection(dbName)) 
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT * FROM aggregated_delays;";

                        using (IDataReader reader = command.ExecuteReader()) 
                            {
                                while (reader.Read())
                                    Debug.Log("Name: " + reader["airport_name"]);

                                reader.Close();
                            }
                    }
                    

    
                }
                // connection.Close();
            }

    // Update is called once per frame
    void Update()
    {
        
    }
}

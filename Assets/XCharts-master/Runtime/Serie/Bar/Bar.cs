using UnityEngine;
using System;
using System.Data;
using System.Collections.Generic;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using System.Data.SQLite;
#endif

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
using Mono.Data.Sqlite;
#endif

namespace XCharts.Runtime
{
    [System.Serializable]
    [SerieHandler(typeof(BarHandler), true)]
    [SerieConvert(typeof(Line), typeof(Pie))]
    [CoordOptions(typeof(GridCoord), typeof(PolarCoord))]
    [DefaultAnimation(AnimationType.BottomToTop)]
    [DefaultTooltip(Tooltip.Type.Shadow, Tooltip.Trigger.Axis)]
    [SerieComponent(typeof(LabelStyle), typeof(EmphasisStyle), typeof(BlurStyle), typeof(SelectStyle))]
    [SerieDataComponent(typeof(ItemStyle), typeof(LabelStyle), typeof(EmphasisStyle), typeof(BlurStyle), typeof(SelectStyle))]
    [SerieDataExtraField("m_Ignore")]
    public class Bar : Serie, INeedSerieContainer
    {
        public int containerIndex { get; internal set; }
        public int containterInstanceId { get; internal set; }

        public static Serie AddDefaultSerie(BaseChart chart, string serieName)
        {
            var serie = chart.AddSerie<Bar>(serieName);
            return serie;
        }

        public static Bar ConvertSerie(Serie serie)
        {
            var newSerie = SerieHelper.CloneSerie<Bar>(serie);
            return newSerie;
        }
    }

    // Your chart class (replace YourChartClass with the actual class name)
    public class YourChartClass : BaseChart
    {
        protected override void DefaultChart()
        {
            string dbPath = "URI=file:" + Application.dataPath + "/../../aviation.db";
            Debug.Log(dbPath);

            IDbConnection dbConnection = null;
            List<float> avgDelays = new List<float>();

            #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                dbConnection = new SQLiteConnection(dbPath);
            #endif

            #if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                dbConnection = new SqliteConnection(dbPath);
            #endif

            try
            {
                dbConnection.Open();
                string query = "SELECT avg_delay FROM aggregated_delays LIMIT 5";
                IDbCommand dbCommand = dbConnection.CreateCommand();
                dbCommand.CommandText = query;

                IDataReader reader = dbCommand.ExecuteReader();

                while (reader.Read())
                {
                    float currentAvgDelay = DBNull.Value.Equals(reader["avg_delay"]) ? 0f : Convert.ToSingle(reader["avg_delay"]);
                    avgDelays.Add(currentAvgDelay);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error executing query: {e.Message}");
            }
            finally
            {
                if (dbConnection != null && dbConnection.State == ConnectionState.Open)
                    dbConnection.Close();
            }

            // Clear existing data and components
            RemoveData();
            RemoveChartComponents<GridCoord>();
            RemoveChartComponents<XAxis>();
            RemoveChartComponents<YAxis>();

            // Ensure necessary chart components are present
            EnsureChartComponent<GridCoord>();
            EnsureChartComponent<XAxis>();
            EnsureChartComponent<YAxis>();

            // Create a bar series and add it to the chart
            Bar barSeries = (Bar)Bar.AddDefaultSerie(this, GenerateDefaultSerieName());


            // Set the data for the Y-axis (values)
            for (int i = 0; i < avgDelays.Count; i++)
            {
                barSeries.AddData(avgDelays[i]);
            }
        }
    }
}

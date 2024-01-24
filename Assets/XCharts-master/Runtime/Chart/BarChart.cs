using UnityEngine;
using System;
using System.Data;
using System.Collections.Generic;
// using System.Data.SQLite;
using UnityEngine.UI;
// import SQLite on windows platform
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    using System.Data.SQLite;
#endif
// import SQLite on macOS platform
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
    using Mono.Data.Sqlite;
#endif


namespace XCharts.Runtime
{
    /// <summary>
    /// Bar chart shows different data through the height of a bar, which is used in rectangular coordinate with at least 1 category axis.
    /// || 柱状图（或称条形图）是一种通过柱形的高度（横向的情况下则是宽度）来表现数据大小的一种常用图表类型。
    /// </summary>
    [AddComponentMenu("XCharts/BarChart", 14)]
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    [HelpURL("https://xcharts-team.github.io/docs/configuration")]
    public class BarChart : BaseChart
    {
protected override void DefaultChart()
{
    string dbPath = "URI=file:" + Application.dataPath + "/../../aviation.db";
    Debug.Log(dbPath);

    IDbConnection dbConnection = null;
    List<string> airportNames = new List<string>(); // Use a list to store multiple airport codes

    #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        dbConnection = new SQLiteConnection(dbPath);
    #endif
    #if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        dbConnection = new SqliteConnection(dbPath);
    #endif

    try
    {
        dbConnection.Open();
        string query = "SELECT * FROM aggregated_delays LIMIT 5";
        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = query;

        IDataReader reader = dbCommand.ExecuteReader();

        while (reader.Read())
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                Debug.Log(reader.GetName(i) + ": " + reader[i]);
            }
            // dsd
            string currentAirportName = DBNull.Value.Equals(reader["airport_name"]) ? string.Empty : reader["airport_name"].ToString();
            airportNames.Add(currentAirportName);
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

    EnsureChartComponent<GridCoord>();
    EnsureChartComponent<XAxis>();
    EnsureChartComponent<YAxis>();

    RemoveData();
    Bar.AddDefaultSerie(this, GenerateDefaultSerieName());

    // Add all retrieved airport codes to X-axis data
    foreach (string name in airportNames)
    {
        AddXAxisData(name);
    }
}











        /// <summary>
        /// default zebra column chart.
        /// || 斑马柱状图。
        /// </summary>
        public void DefaultZebraColumnChart()
        {
            CheckChartInit();
            var serie = GetSerie(0);
            if (serie == null) return;
            serie.barType = BarType.Zebra;
        }

        /// <summary>
        /// default capsule column chart.
        /// || 胶囊柱状图。
        /// </summary>
        public void DefaultCapsuleColumnChart()
        {
            CheckChartInit();
            var serie = GetSerie(0);
            if (serie == null) return;
            serie.barType = BarType.Capsule;
        }

        /// <summary>
        /// default grouped column chart.
        /// || 默认分组柱状图。
        /// </summary>
        public void DefaultGroupedColumnChart()
        {
            CheckChartInit();
            Bar.AddDefaultSerie(this, GenerateDefaultSerieName());
        }

        /// <summary>
        /// default stacked column chart.
        /// || 默认堆叠分组柱状图。
        /// </summary>
        public void DefaultStackedColumnChart()
        {
            CheckChartInit();
            var serie1 = GetSerie(0);
            serie1.stack = "stack1";
            var serie2 = Bar.AddDefaultSerie(this, GenerateDefaultSerieName());
            serie2.stack = "stack1";
        }

        /// <summary>
        /// default percent column chart.
        /// || 默认百分比柱状图。
        /// </summary>
        public void DefaultPercentColumnChart()
        {
            CheckChartInit();
            var serie1 = GetSerie(0);
            serie1.stack = "stack1";
            serie1.barPercentStack = true;
            var serie2 = Bar.AddDefaultSerie(this, GenerateDefaultSerieName());
            serie2.stack = "stack1";
            serie2.barPercentStack = true;
        }

        /// <summary>
        /// default bar chart.
        /// || 默认条形图。
        /// </summary>
        public void DefaultBarChart()
        {
            CheckChartInit();
            CovertColumnToBar(this);
        }

        /// <summary>
        /// default zebra bar chart.
        /// || 默认斑马条形图。 
        /// </summary>
        public void DefaultZebraBarChart()
        {
            CheckChartInit();
            var serie = GetSerie(0);
            serie.barType = BarType.Zebra;
            CovertColumnToBar(this);
        }

        /// <summary>
        /// default capsule bar chart.
        /// || 默认胶囊条形图。
        /// </summary>
        public void DefaultCapsuleBarChart()
        {
            CheckChartInit();
            var serie = GetSerie(0);
            serie.barType = BarType.Capsule;
            CovertColumnToBar(this);
        }

        /// <summary>
        /// default grouped bar chart.
        /// || 默认分组条形图。
        /// </summary>
        public void DefaultGroupedBarChart()
        {
            CheckChartInit();
            Bar.AddDefaultSerie(this, GenerateDefaultSerieName());
            CovertColumnToBar(this);
        }

        /// <summary>
        /// default stacked bar chart.
        /// || 默认堆叠条形图。
        /// </summary>
        public void DefaultStackedBarChart()
        {
            CheckChartInit();
            var serie1 = GetSerie(0);
            serie1.stack = "stack1";
            var serie2 = Bar.AddDefaultSerie(this, GenerateDefaultSerieName());
            serie2.stack = "stack1";
            CovertColumnToBar(this);
        }

        /// <summary>
        /// default percent bar chart.
        /// || 默认百分比条形图。
        /// </summary>
        public void DefaultPercentBarChart()
        {
            CheckChartInit();
            var serie1 = GetSerie(0);
            serie1.stack = "stack1";
            serie1.barPercentStack = true;
            var serie2 = Bar.AddDefaultSerie(this, GenerateDefaultSerieName());
            serie2.stack = "stack1";
            serie2.barPercentStack = true;
            CovertColumnToBar(this);
        }

        private static void CovertColumnToBar(BarChart chart)
        {
            chart.ConvertXYAxis(0);
            var xAxis = chart.GetChartComponent<XAxis>();
            xAxis.axisLine.show = false;
            xAxis.axisTick.show = false;

            var yAxis = chart.GetChartComponent<YAxis>();
            yAxis.axisTick.alignWithLabel = true;
        }
    }
}
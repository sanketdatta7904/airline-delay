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

            // Dummy data list
            List<float> dummyData = new List<float> { 19f, 820f, 30f, 20f, 701f };

            for (int i = 0; i < dummyData.Count; i++)
            {
                chart.AddData(serie.index, dummyData[i]);
            }

            return serie;
        }

        public static Bar ConvertSerie(Serie serie)
        {
            var newSerie = SerieHelper.CloneSerie<Bar>(serie);
            return newSerie;
        }
    }
}



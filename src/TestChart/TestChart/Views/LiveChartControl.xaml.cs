using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Geared;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TestChart.Models;

namespace TestChart.Views
{
    /// <summary>
    /// LiveChartControl.xaml 的交互逻辑
    /// </summary>
    public partial class LiveChartControl : UserControl, IViewer
    {
        class ChartVectorValue
        {
            public DateTime Time { get; set; }
            public sbyte Value { get; set; }
        }

        private Stopwatch _stopwatch = new Stopwatch();

        public LiveChartControl()
        {
            InitializeComponent();

            XAxis.LabelFormatter= value => new DateTime((long)value).ToString("HH:mm:ss");

            //设置横轴单位
            XAxis.Unit = TimeSpan.TicksPerMillisecond;
        }

        private void UpdateXAxisLimit(DateTime maxTime)
        {
            XAxis.MinValue = (maxTime - TimeSpan.FromSeconds(8)).Ticks;
            XAxis.MaxValue = maxTime.Ticks;
        }

        public void AppendDatas(List<VectorInfo> infos)
        {
            _stopwatch.Restart();
            _stopwatch.Start();

            if (lineChart.Series == null)
            {
                var dayConfig = Mappers.Xy<ChartVectorValue>()
                     .X(dateModel => dateModel.Time.Ticks)
                     .Y(dateModel => dateModel.Value);

                lineChart.Series = new SeriesCollection(dayConfig);
            }

            var nowTime = DateTime.Now;
            UpdateXAxisLimit(nowTime);

            var startTime = nowTime - TimeSpan.FromSeconds(2);

            var maxCount = infos.Max(info => info.Data.Count);
            var interval = TimeSpan.FromSeconds(2).Ticks * 1.0 / maxCount;

            infos.ForEach(info =>
            {
                var lineSeries = lineChart.Series.FirstOrDefault(ls => ls.Title == info.ID.ToString());
                if (lineSeries == null)
                {
                    lineSeries = new LineSeries
                    {
                        Title = info.ID.ToString(),
                        Fill = Brushes.Transparent,
                        StrokeThickness = 0.5,
                        PointGeometry = null,
                        Values = new GearedValues<ChartVectorValue>(info.Data.Select((d, i) => new ChartVectorValue { Time = startTime + TimeSpan.FromTicks((long)(i * interval)), Value = d })).WithQuality(Quality.Medium)
                    };

                    lineChart.Series.Add(lineSeries);
                }
                else
                {
                    var chartValues = lineSeries.Values as GearedValues<ChartVectorValue>;

                    while (true)
                    {
                        var point = chartValues[0];
                        if (point.Time.Ticks < XAxis.MinValue)
                        {
                            chartValues.RemoveAt(0);
                            continue;
                        }

                        break;
                    }

                    chartValues.AddRange(info.Data.Select((d, i) => new ChartVectorValue { Time = startTime + TimeSpan.FromTicks((long)(i * interval)), Value = d }));
                }
            });

            _stopwatch.Stop();

            Trace.WriteLine("[ Time ] " + _stopwatch.ElapsedMilliseconds + " ms " + nowTime.ToString("HH:mm:ss"));
        }
    }
}

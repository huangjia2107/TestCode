using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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

namespace TestChart
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly int MinValue = -80;
        private readonly int MaxValue = -60;
        private readonly int ValueCountPerVector = 40;
        private readonly int VectorCount = 250;

        private Timer _timer = null;
        private int _triggerCount = 0;
        private ConcurrentDictionary<int, VectorInfo> _vectorInfoDic = new ConcurrentDictionary<int, VectorInfo>();
        private Dictionary<int, Pen> _vectorPenDic = new Dictionary<int, Pen>();

        public MainWindow()
        {
            InitializeComponent();

            _timer = new Timer(500);
            _timer.Elapsed += _timer_Elapsed;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            for (int i = 1; i <= VectorCount; i++)
            {
                var info = new VectorInfo { ID = i, Data = GetRandomValues(MinValue, MaxValue, ValueCountPerVector) };

                if (_vectorInfoDic.ContainsKey(i))
                    _vectorInfoDic[i].Data.AddRange(info.Data);
                else
                {
                    if (_vectorPenDic.ContainsKey(i))
                        info.P = _vectorPenDic[i];
                    else
                    {
                        var p = new Pen(GetColor(), 1);
                        p.Freeze();
                        _vectorPenDic.Add(i, p);

                        info.P = p;
                    }
                    
                    _vectorInfoDic.TryAdd(i, info);
                }
            }

            _triggerCount++;

            if (_triggerCount == 4)
            {
                _triggerCount = 0;

                var values = _vectorInfoDic.Values.ToList();
                _vectorInfoDic.Clear();

                this.Dispatcher.BeginInvoke((Action<List<VectorInfo>>)((infos) =>
                {
                    DealCurrentVectors(infos);
                }), values);
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_timer != null)
                _timer.Enabled = true;
        }

        private Brush GetColor()
        {
            Random ro = new Random(10);
            long tick = DateTime.Now.Ticks;
            Random ran = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));

            var R = ran.Next(255);
            var G = ran.Next(255);
            var B = ran.Next(255);
            B = (R + G > 400) ? R + G - 400 : B;//0 : 380 - R - G;
            B = (B > 255) ? 255 : B;
            return new SolidColorBrush(Color.FromRgb((byte)R, (byte)G, (byte)B));
        }

        private List<sbyte> GetRandomValues(int min, int max, int count)
        {
            var result = new List<sbyte>();

            while (result.Count < count)
            {
                result.Add((sbyte)(new Random(BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0))).Next(min, max));
            }

            return result;
        }

        private void DealCurrentVectors(List<VectorInfo> infos)
        {
            liveChartControl.AppendDatas(infos);
        }
    }
}


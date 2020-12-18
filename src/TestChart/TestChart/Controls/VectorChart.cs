using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TestChart.Models;

namespace TestChart.Controls
{
    public class VectorChart:StackPanel
    {
        private VisualCollection _children;
        private readonly Queue<DrawingVisual> drawingVisuals = new Queue<DrawingVisual>();

        public VectorChart()
        {
            this.Orientation = Orientation.Horizontal;
            _children = new VisualCollection(this);
        }

        protected override int VisualChildrenCount
        {
            get { return _children.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return _children[index];
        }

        private void AddVisual(Visual visual)
        {
            _children.Add(visual);
        }

        private void RemoveVisual(Visual visual)
        {
            _children.Remove(visual);
            visual = null;
        }

        public static readonly DependencyProperty YMaxProperty =
            DependencyProperty.Register("YMax", typeof(int), typeof(VectorChart), new PropertyMetadata(-20));
        public int YMax
        {
            get { return (int)GetValue(YMaxProperty); }
            set { SetValue(YMaxProperty, value); }
        }

        public static readonly DependencyProperty YMinProperty =
            DependencyProperty.Register("YMin", typeof(int), typeof(VectorChart), new PropertyMetadata(-120));
        public int YMin
        {
            get { return (int)GetValue(YMinProperty); }
            set { SetValue(YMinProperty, value); }
        }

        public static readonly DependencyProperty XMaxProperty =
            DependencyProperty.Register("XMax", typeof(long), typeof(VectorChart), new PropertyMetadata(0L));
        public long XMax
        {
            get { return (long)GetValue(XMaxProperty); }
            set { SetValue(XMaxProperty, value); }
        }

        public static readonly DependencyProperty XMinProperty =
            DependencyProperty.Register("XMin", typeof(long), typeof(VectorChart), new PropertyMetadata(0L));
        public long XMin
        {
            get { return (long)GetValue(XMinProperty); }
            set { SetValue(XMinProperty, value); }
        }

        public void AppendDatas(List<VectorInfo> infos)
        {
            var disPerDV = this.ActualWidth / 7;
            var startX = drawingVisuals.Count * disPerDV;
            var endX = startX + disPerDV;

            var ddd = this.ActualHeight / (YMax - YMin);
            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                foreach(var info in infos)
                {
                    var disPerPoint = disPerDV / (info.Data.Count-1);
                    for(int i=1;i<info.Data.Count;i++)
                    {
                        var preData = info.Data[i - 1];
                        var data = info.Data[i];

                        var preYOffset = (YMax - preData) * ddd;
                        var yOffset = (YMax - data) * ddd;

                        dc.DrawLine(info.P, new Point((i - 1) * disPerPoint + startX, preYOffset), new Point(i * disPerPoint + startX, yOffset));
                    }
                }

                //dc.PushTransform(new TranslateTransform());
            }

            if (drawingVisuals.Count == 7)
            {
                var first = drawingVisuals.Dequeue();
                this.RemoveVisual(first);
            }

            drawingVisuals.Enqueue(dv);
            this.AddVisual(dv);
        }
    }
}

using System.Collections.Generic;
using System.Windows.Media;

namespace TestChart.Models
{
    public class VectorInfo
    {
        public int ID { get; set; }

        public Pen P { get; set; }

        public List<sbyte> Data { get; set; } = new List<sbyte>();
    }
}

using System.Collections.Generic;
using TestChart.Models;

namespace TestChart.Views
{
    public interface IViewer
    {
        void AppendDatas(List<VectorInfo> infos);
    }
}

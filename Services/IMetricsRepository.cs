using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MetricsAPI.Services
{
    public interface IMetricsRepository
    {
        object GetTable(string table);
        object GetTableItem(string table, string bicycleId);

    }
}

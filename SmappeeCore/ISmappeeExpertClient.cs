using System;
using System.Collections.Generic;
using System.Text;

namespace SmappeeCore
{
    /// <summary>
    /// Smappee Core Expert Client Interface
    /// </summary>
    public interface ISmappeeExpertClient
    {
        bool Login(SmappeeExpertConfiguration configuration);

        List<SmappeeKeyValuePairs> GetInstantValue();

        List<SmappeeKeyValuePairs> GetReportValue();

        bool ResetPeakValue();
    }
}

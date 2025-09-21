using ZergPoolMiner.Miners.Grouping;
using System;

namespace ZergPoolMiner.Interfaces
{
    public interface IMainFormRatesComunication
    {
        void ClearRatesAll();

        void AddRateInfo(string groupName, ApiData iApiData, bool isApiGetException, string ProcessTag, 
            GroupMiner groupMiners, int groupCount);
        //void RaiseAlertSharesNotAccepted(string algoName);

        // The following four must use an invoker since they may be called from non-UI thread

        void ShowNotProfitable(string msg);

        void HideNotProfitable();

        void ForceMinerStatsUpdate();

        void ClearRates(int groupCount);
    }
}

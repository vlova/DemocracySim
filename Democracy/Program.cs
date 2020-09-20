using Democracy.Common;
using Democracy.Simulations;

namespace Democracy
{
    class Program
    {
        static void Main(string[] args)
        {
            NativeMethods.PreventSleep();

            //ResistanceSimulationAggregator.GenerateAndWriteToCSV();
            //ThresholdDemocracySimulationAggregator.GenerateAndWriteToCSV();
            //FactorDemocracySimulationAggregator.GenerateAndWriteToCSV();
            SummarySimulationAggregator.GenerateAndWriteToCSV();
        }
    }
}

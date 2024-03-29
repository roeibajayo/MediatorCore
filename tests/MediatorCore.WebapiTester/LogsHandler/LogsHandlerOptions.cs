namespace MediatorCore.WebapiTester.LogsHandler;

public class LogsHandlerOptions : 
    IAccumulatorQueueOptions
{
    public int MsInterval => 2000;
    public int? AccumulationCapacity => default;
    public int? TotalCapacity => default;
    public MaxCapacityBehaviors? MaxTotalCapacityBehavior => default;
}

namespace MediatorCore.WebapiTester.LogsHandler;

public class LogsHandlerOptions : 
    IAccumulatorQueueOptions
{
    public int MsInterval => 2000;
    public int? MaxMessagesOnDequeue => default;
    public int? MaxMessagesStored => default;
    public MaxMessagesStoredBehaviors? MaxMessagesStoredBehavior => default;
}

namespace MediatorCore.WebapiTester.LogsHandler;

public record LogMessage(string Message) : 
    IAccumulatorQueueMessage;

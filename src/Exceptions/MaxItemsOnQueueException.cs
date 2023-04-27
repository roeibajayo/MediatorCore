namespace MediatorCore.Exceptions;

public class MaxMessagesOnQueueException : Exception
{
    internal MaxMessagesOnQueueException() : base("Max items on queue reached.") { }

    internal static void Throw()
    {
        throw new MaxMessagesOnQueueException();
    }
}
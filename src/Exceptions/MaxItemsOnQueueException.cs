namespace MediatorCore.Exceptions;

public class MaxItemsOnQueueException : Exception
{
    internal MaxItemsOnQueueException() : base("Max items on queue reached.") { }

    internal static void Throw()
    {
        throw new MaxItemsOnQueueException();
    }
}
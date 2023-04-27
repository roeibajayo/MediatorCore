namespace MediatorCore.Exceptions;

public class NoRegisteredHandlerException : Exception
{
    public Type MessageType { get; internal set; }
    internal NoRegisteredHandlerException(Type messageType) :
        base("No registered handler for request " + messageType.Name +
                ", Make sure you registered the assembly of the handler.")
    {
        MessageType = messageType;
    }

    internal static void ThrowIfNull(object? handler, object message)
    {
        if (handler is null)
            throw new NoRegisteredHandlerException(message.GetType());
    }
    internal static void Throw<TMessage>()
    {
        throw new NoRegisteredHandlerException(typeof(TMessage));
    }
}
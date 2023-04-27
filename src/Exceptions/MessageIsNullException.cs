namespace MediatorCore.Exceptions;

public class MessageIsNullException : ArgumentNullException
{
    public MessageIsNullException() : base("Message can not be null.") { }

    internal static void ThrowIfNull(object? message)
    {
        if (message is null)
            throw new MessageIsNullException();
    }
}
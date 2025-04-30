namespace LiteMediator.Exceptions;

public class HandlerNotFoundException : Exception
{
    public HandlerNotFoundException(Type type)
        : base($"Handler not found for {type.Name}")
    {
    }
}

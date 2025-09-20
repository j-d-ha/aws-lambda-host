namespace Lambda.Host;

public sealed class DelegateHolder
{
    public Delegate? Handler { get; set; }
    public bool IsHandlerSet => Handler != null;
}

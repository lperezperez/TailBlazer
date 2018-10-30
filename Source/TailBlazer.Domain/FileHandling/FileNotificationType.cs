namespace TailBlazer.Domain.FileHandling
{
    using System;
    [Flags]
    public enum FileNotificationType
    {
        None,
        CreatedOrOpened,
        Changed,
        Missing,
        Error
    }
}
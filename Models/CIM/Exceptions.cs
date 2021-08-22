using System;

namespace Models.CIM
{
    public class SubCommandException : Exception
    {
        public SubCommandException(string message)
            : base(message) { }
    }

    public class CaughtByAvException : Exception
    {
        public CaughtByAvException(string message)
            : base(message) { }
    }

    public class ExtraCommandException : Exception
    {
    }

    public class ServiceUnknownException : Exception
    {
        public ServiceUnknownException(string message)
            : base(message) { }
    }

    public class RektDebugFilePath : Exception
    {
        public RektDebugFilePath(string message)
            : base(message) { }
    }

    public class ProcessCommandException : Exception
    {
        public ProcessCommandException(string message)
            : base(message) { }
    }
}
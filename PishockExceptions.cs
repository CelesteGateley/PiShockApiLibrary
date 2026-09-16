namespace PiShockApiLibrary;

public class PishockException: Exception
{
    public PishockException() { }
    public PishockException(string message) : base(message) { }
    public PishockException(string message, Exception innerException) : base(message, innerException) { }
}

public class PishockAuthenticationException : PishockException
{
    public PishockAuthenticationException() { }
    public PishockAuthenticationException(string message) : base(message) { }
    public PishockAuthenticationException(string message, Exception innerException) : base(message, innerException) { }
}

public class PishockPermissionException : PishockException
{
    public PishockPermissionException() { }
    public PishockPermissionException(string message) : base(message) { }
    public PishockPermissionException(string message, Exception innerException) : base(message, innerException) { }
}

public class PishockDataException : PishockException
{
    public PishockDataException() { }
    public PishockDataException(string message) : base(message) { }
    public PishockDataException(string message, Exception innerException) : base(message, innerException) { }
}

public class PishockShockerException : PishockException
{
    public PishockShockerException() { }
    public PishockShockerException(string message) : base(message) { }
    public PishockShockerException(string message, Exception innerException) : base(message, innerException) { }
}

public class PishockNotSupportedException : PishockException
{
    public PishockNotSupportedException() { }
    public PishockNotSupportedException(string message) : base(message) { }
    public PishockNotSupportedException(string message, Exception innerException) : base(message, innerException) { }
}
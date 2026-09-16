namespace PiShockApiLibrary;

/// <summary>
/// Base type for all exceptions thrown by <see cref="PiShockApiLibrary"/>. Thrown directly for any unrecognized
/// error response from the PiShock API that doesn't fit one of the more specific subtypes below.
/// </summary>
public class PishockException: Exception
{
    public PishockException() { }
    public PishockException(string message) : base(message) { }
    public PishockException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Thrown when the supplied API key, username, or share code is invalid, missing, or otherwise fails authentication.
/// </summary>
public class PishockAuthenticationException : PishockException
{
    public PishockAuthenticationException() { }
    public PishockAuthenticationException(string message) : base(message) { }
    public PishockAuthenticationException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Thrown when credentials are valid but lack permission for the requested action — e.g. the share/API key can't
/// perform the requested mode (shock/vibrate/beep), or the shocker/share is currently paused or locked.
/// </summary>
public class PishockPermissionException : PishockException
{
    public PishockPermissionException() { }
    public PishockPermissionException(string message) : base(message) { }
    public PishockPermissionException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Thrown when a value supplied by the caller (or returned by the API) doesn't fit — e.g. an out-of-range
/// intensity/duration rejected by the API itself, or a response body that couldn't be parsed as expected.
/// </summary>
public class PishockDataException : PishockException
{
    public PishockDataException() { }
    public PishockDataException(string message) : base(message) { }
    public PishockDataException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Thrown when the requested shocker can't be resolved — it doesn't exist, doesn't support the API being used,
/// or no share matches the given identifying info.
/// </summary>
public class PishockShockerException : PishockException
{
    public PishockShockerException() { }
    public PishockShockerException(string message) : base(message) { }
    public PishockShockerException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Thrown when the client refuses a request before it's sent, because it already knows the underlying API can't
/// honor what was asked (e.g. <see cref="LegacyShocker"/> in <c>strict</c> mode rejecting a randomized duration
/// or an ambiguous share code). Distinct from the exceptions above, which represent the API itself rejecting a request.
/// </summary>
public class PishockNotSupportedException : PishockException
{
    public PishockNotSupportedException() { }
    public PishockNotSupportedException(string message) : base(message) { }
    public PishockNotSupportedException(string message, Exception innerException) : base(message, innerException) { }
}
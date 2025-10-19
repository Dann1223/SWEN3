namespace PaperlessRESTAPI.Infrastructure.Exceptions;

public class BusinessException : Exception
{
    public string ErrorCode { get; }

    public BusinessException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }

    public BusinessException(string errorCode, string message, Exception innerException) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

public class DataException : Exception
{
    public DataException(string message) : base(message)
    {
    }

    public DataException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class ServiceException : Exception
{
    public ServiceException(string message) : base(message)
    {
    }

    public ServiceException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

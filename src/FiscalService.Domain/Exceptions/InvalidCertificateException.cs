namespace FiscalService.Domain.Exceptions;

public class InvalidCertificateException : Exception
{
    public InvalidCertificateException(string message) : base(message)
    {
    }

    public InvalidCertificateException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

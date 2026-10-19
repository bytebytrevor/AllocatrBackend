namespace AllocatrApi.Services;

public sealed class ProjectFileException : Exception
{
    public int StatusCode { get; }
    public ProjectFileException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}

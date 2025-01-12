namespace DemoLibrary.Exceptions;

public class ValidationException : ArgumentException
{
    public ValidationException(IReadOnlyDictionary<string, List<string>> errors)
        : base("Validation failed for one or more fields.")
    {
        this.Errors = errors;
    }

    public IReadOnlyDictionary<string, List<string>> Errors { get; set; }
}
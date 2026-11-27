namespace LeagueManager.Api.Exceptions;

public class BusinessRuleException : Exception
{
    public BusinessRuleException(string field, string message)
        : base(message)
    {
        Field = field;
    }

    public string Field { get; }
}
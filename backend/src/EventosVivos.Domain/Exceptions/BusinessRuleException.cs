namespace EventosVivos.Domain.Exceptions;

public sealed class BusinessRuleException : DomainException
{
    public BusinessRuleException(string ruleCode, string message) : base(message)
    {
        RuleCode = ruleCode;
    }

    public string RuleCode { get; }
}

namespace Fundo.Application.Configuration;

public class DatabaseOptions
{
    public const string SectionName = "Database";

    public string Provider { get; set; } = "SqlServer";
    public string InMemoryName { get; set; } = "FundoLoans";
}

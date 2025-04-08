using Microsoft.AspNetCore.Authentication;

namespace Library.Api.Auth;

public class ApiKeyAuthSchemeOptions : AuthenticationSchemeOptions
{
    public string ApiKey { get; set; } = "VerySecret"; //Realistcally you would not hardcode this, you would call from somewhere secure like Azure Key Vault or AWS Secrets Manager, for demo purposes it is here


}

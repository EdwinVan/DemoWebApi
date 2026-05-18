namespace DemoWebApi.Models.Configs;

public class JwtOptions
{
    public string Issuer { get; set; } = "DemoWebApi";
    public string Audience { get; set; } = "DemoWebApiClient";
    public string SecretKey { get; set; } = string.Empty;
    public int ExpireMinutes { get; set; } = 120;
}

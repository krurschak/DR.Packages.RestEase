# DR RestEase Package

Create services like internal services that query external APIs with a simple options API with [RestEase](https://github.com/canton7/RestEase)
<br/>

# QuickStart
## Service

Create a new service with [RestEase](https://github.com/canton7/RestEase) (e.g. RestServices/IEmailService.cs)
``` c#
using RestEase;

[SerializationMethods(Query = QuerySerializationMethod.Serialized)]
public interface IEmailService
{
    [AllowAnyStatusCode]
    [Get("Emails/{id}")]
    Task<Dto.EmailDetails> GetAsync([Path] Guid id);

    [AllowAnyStatusCode]
    [Get("Emails/Browse")]
    Task<Dto.EmailsResponse> GetAllAsync(
        [Query] string fulltextSearch,
        [Query] int[] emailStatus,
        [Query] int pageIndex,
        [Query] int pageSize);
}
```
## Options
Add service options in appsettings.json
```json
{
  "RestEase": {
    "Services": [
      {
        "Name": "email-service",
        "Scheme": "http",
        "Host": "email-service.services",
        "Port": "80",
        "Prefix": "emails"
      }
    ]
  }
}
```
>This will give us the the example k8s service address: *http://email-service.services/emails*

## Register
Register rest service in startup.cs
```c#
using DR.Packages.RestEase;

public class Startup
{
    public void ConfigureServices(IServiceCollection services,
        IConfiguration configuration)
    {
        services.RegisterServiceForwarder<IEmailService>("email-service",
            configuration.GetSection("RestEase").Get<RestEaseOptions>());
    }
}
```

## Example service usage
```c#
public class MyController : ControllerBase
{
    private readonly IEmailService emailService;

    public MyController(
        IEmailService emailService)
    {
        this.emailService = emailService;
    }

    [HttpGet]
    public async Task<Dto.EmailDetails> GetEmailDetails(Guid id)
    {
        var email = await emailService.GetAsync(id));
        return email;
    }
}
```
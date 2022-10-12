using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace Net6WebAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]/[Action]")]
//[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
[RequiredScope("access_as_user")]
public class HelloController : ControllerBase
{
    public string Get()
    {
        return "hello";
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_API.Controllers
{
    [Route("Errorhandling")]
    [ApiController]
    [AllowAnonymous]//Dont need to auth
    [ApiVersionNeutral]//availble for all versions
    [ApiExplorerSettings(IgnoreApi = true)]//Is not an api and dont generate as api endpoint
    public class ErrorHandlingController : ControllerBase
    {
        [Route("ProcessError")]
        //public IActionResult ProcessError() => Problem();
        public IActionResult ProcessError([FromServices] IHostEnvironment hostEnvironment)
        {
            if (hostEnvironment.IsDevelopment())
            {
                //custom logic
                var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();

                return Problem(
                    detail:feature.Error.StackTrace,
                    title:feature.Error.Message,
                    instance:hostEnvironment.EnvironmentName
                    );
            }
            else
            {
                {
                    return Problem();
                }
            }
        }
    }
}

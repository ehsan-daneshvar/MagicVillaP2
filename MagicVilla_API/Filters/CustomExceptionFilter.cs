using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MagicVilla_API.Filters
{
    public class CustomExceptionFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception is BadImageFormatException)
            {
                context.Result = new ObjectResult("File Format is not valid")
                {
                    StatusCode = 503
                };
                //اگر کد زیر نوشته نوشته ارور هندل نشده و به هندلر پیش فرض میرود
                context.ExceptionHandled = true;
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            throw new NotImplementedException();
        }
    }
}

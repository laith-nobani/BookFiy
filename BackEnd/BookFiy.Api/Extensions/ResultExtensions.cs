using BookFiy.Application.Comman;
using Microsoft.AspNetCore.Mvc;

namespace BookFiy.Api.Extensions
{
    public static class ResultExtensions
    {
        public static IActionResult ToActionResult<T>(this Result<T> result)
        {
            if (result.IsSuccess)
            {
                return new OkObjectResult(new
                {
                    message = result.Message,
                    data = result.Data
                });
            }

            return result.ErrorType switch
            {
                ErrorType.Validation =>
                    new BadRequestObjectResult(result.Message),

                ErrorType.NotFound =>
                    new NotFoundObjectResult(result.Message),

                ErrorType.Unauthorized =>
                    new UnauthorizedObjectResult(result.Message),

                ErrorType.Forbidden =>
                    new ObjectResult(result.Message)
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    },

                ErrorType.Conflict =>
                    new ConflictObjectResult(result.Message),

                ErrorType.ServerError =>
                    new ObjectResult("Internal server error")
                    {
                        StatusCode = 500
                    },

                _ =>
                    new ObjectResult("Unknown error")
                    {
                        StatusCode = 500
                    }
            };
        }
    }
}

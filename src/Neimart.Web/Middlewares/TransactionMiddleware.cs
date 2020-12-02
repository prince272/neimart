using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Neimart.Data;

namespace Neimart.Web.Middlewares
{
    // ASP.NET Core Web API - How to hide DbContext transaction in the middleware pipeline?
    // source: https://stackoverflow.com/questions/58225119/asp-net-core-web-api-how-to-hide-dbcontext-transaction-in-the-middleware-pipel/62587685#62587685
    public class TransactionMiddleware
    {
        private readonly RequestDelegate next;

        public TransactionMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext httpContext, AppDbContext context)
        {
            string httpVerb = httpContext.Request.Method.ToUpper();

            if (httpVerb == "POST" || httpVerb == "PUT" || httpVerb == "DELETE")
            {
                var strategy = context.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync<object, object>(null!, operation: async (dbctx, state, cancel) =>
                {
                    // start the transaction
                    await using var transaction = await context.Database.BeginTransactionAsync();

                    // invoke next middleware 
                    await next(httpContext);

                    // commit the transaction
                    await transaction.CommitAsync();

                    return null!;
                }, null);
            }
            else
            {
                await next(httpContext);
            }
        }
    }

    public static class TransactionMiddlewareExtensions
    {
        public static IApplicationBuilder UseTransaction(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TransactionMiddleware>();
        }
    }
}
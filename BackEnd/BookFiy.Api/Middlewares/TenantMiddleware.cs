using BookFiy.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace BookFiy.Api.Middlewares
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider, BookFiy.Infrastructure.Data.Context.AppDbContext dbContext)
        {
            var claim = context.User.FindFirst("tenant_id")?.Value;
            if (Guid.TryParse(claim, out var tenantId))
            {
                if (tenantProvider is BookFiy.Application.Services.TenantProvider providerImpl)
                {
                    providerImpl.SetTenant(tenantId);
                }

                dbContext.CurrentTenantId = tenantId;
            }
            await _next(context);
        }
    }
}

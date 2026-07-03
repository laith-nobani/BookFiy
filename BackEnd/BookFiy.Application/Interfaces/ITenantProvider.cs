using System;

namespace BookFiy.Application.Interfaces
{
    public interface ITenantProvider
    {
        Guid TenantId { get; }
        bool HasTenant { get; }
        void SetTenant(Guid tenantId);
    }
}

using BookFiy.Application.Interfaces;
using System;

namespace BookFiy.Application.Services
{
    public class TenantProvider : ITenantProvider
    {
        private Guid _tenantId = Guid.Empty;
        public Guid TenantId => _tenantId;
        public bool HasTenant => _tenantId != Guid.Empty;
        public void SetTenant(Guid tenantId) => _tenantId = tenantId;
    }
}

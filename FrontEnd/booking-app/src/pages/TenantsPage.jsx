import React from "react";
import ResourcePage from "./ResourcePage";

// Matches CreateTenantDto { name, slug } / TenantDto { id, name, slug, createdAt }
export default function TenantsPage() {
  return (
    <ResourcePage
      title="Tenants"
      path="/Tenant"
      fields={[{ name: "name", label: "Tenant name" }, { name: "slug", label: "Slug" }]}
      columns={[{ key: "name", label: "Name" }, { key: "slug", label: "Slug" }]}
    />
  );
}

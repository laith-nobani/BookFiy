using BookFiy.Application.Dtos.Service;
using BookFiy.Application.Interfaces;
using BookFiy.Domain.Entites;


namespace BookFiy.Application.Services
{
    public class ServiceService : IServiceService
    {
        private readonly BookFiy.Domain.IRepositories.IServiceRepository _repo;
        private readonly IRedisService _redis;

        public ServiceService(BookFiy.Domain.IRepositories.IServiceRepository repo,IRedisService redis)
        {
            _repo = repo;
            _redis = redis;
        }

        public async Task<ServiceDto> CreateServiceAsync(CreateServiceDto dto, Guid tenantId)
        {
            var service = new Service
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = dto.Name,
                Description = dto.Description,
                DurationMinutes = dto.DurationMinutes,
                Price = dto.Price,
                EmployeeId = dto.EmployeeId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _repo.CreateAsync(service);

            await _redis.RemoveAsync($"services_{tenantId}");

            return new ServiceDto
            {
                Id = service.Id,
                Name = service.Name,
                Description = service.Description,
                DurationMinutes = service.DurationMinutes,
                Price = service.Price
            };
        }

        public async Task DeleteServiceAsync(Guid id, Guid tenantId)
        {
            var service = await _repo.GetByIdAsync(id, tenantId);
            if (service == null) 
                throw new KeyNotFoundException("Service not found");

            await _repo.DeleteAsync(id, tenantId);
            await _redis.RemoveAsync($"services_{tenantId}");
        }

        public async Task<List<ServiceDto>> GetAllAsync(Guid tenantId, Guid Employeeid)
        {
            var list = await _repo.GetAllAsync(tenantId,Employeeid);
            return list.Select(s => new ServiceDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                DurationMinutes = s.DurationMinutes,
                Price = s.Price
            }).ToList();
        }

        public async Task<List<ServiceDto>> GetAllAsync(Guid tenantId)
        {
            var key = $"services_{tenantId}";

            var cachedServices = await _redis.GetAsync<List<ServiceDto>>(key);


            if (cachedServices != null)
            {
                return cachedServices;
            }


            var list = await _repo.GetServicesAsync(tenantId);

            var result = list.Select(s => new ServiceDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                DurationMinutes = s.DurationMinutes,
                Price = s.Price
            }).ToList();

            await _redis.SetAsync(key,result, TimeSpan.FromMinutes(5));

            return result;

        }

        public async Task<ServiceDto> GetByIdAsync(Guid id, Guid tenantId)
        {
            var s = await _repo.GetByIdAsync(id, tenantId);
            if (s == null) return null!;
            return new ServiceDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                DurationMinutes = s.DurationMinutes,
                Price = s.Price
            };
        }

        public async Task UpdateServiceAsync(Guid id, UpdateServiceDto dto, Guid tenantId)
        {
            var s = await _repo.GetByIdAsync(id, tenantId);
            if (s == null) throw new KeyNotFoundException("Service not found");
            if (string.IsNullOrEmpty(dto.Name))
            {
                s.Name = dto.Name;

            }
            if (dto.DurationMinutes <= 0)
            {
                throw new ArgumentException("Duration must be greater than zero.");
            }
            if (dto.Price < 0)
            {
                throw new ArgumentException("Price cannot be negative.");
            }
            if (string.IsNullOrEmpty(dto.Description))
            {
                s.Description = dto.Description;
            }

            if(dto.DurationMinutes!=s.DurationMinutes)
               s.DurationMinutes = dto.DurationMinutes;
            if(dto.Price!=s.Price)
                s.Price = dto.Price;
            s.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(s);

            await _redis.RemoveAsync(
             $"services:{tenantId}");
        }
    }
}

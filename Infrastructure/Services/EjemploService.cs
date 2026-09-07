using Application.Dtos.Ejemplo;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class EjemploService : IEjemploService
    {
        private readonly AppDbContext _context;

        public EjemploService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Devuelve todos los ejemplos.
        /// </summary>
        public async Task<IEnumerable<EjemploReadDto>> GetAllAsync()
        {
            return await _context.Ejemplos
                .AsNoTracking()
                .Select(e => new EjemploReadDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Date = e.Date
                })
                .ToListAsync();
        }

        /// <summary>
        /// Devuelve un ejemplo por su id, o null si no existe.
        /// </summary>
        public async Task<EjemploReadDto?> GetByIdAsync(Guid id)
        {
            var ejemplo = await _context.Ejemplos
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ejemplo is null)
            {
                return null;
            }

            return new EjemploReadDto
            {
                Id = ejemplo.Id,
                Name = ejemplo.Name,
                Date = ejemplo.Date
            };
        }

        /// <summary>
        /// Agrega un ejemplo y devuelve el id generado.
        /// </summary>
        public async Task<Guid> AddAsync(EjemploCreateDto dto)
        {
            var ejemplo = new Ejemplo
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Date = dto.Date
            };

            _context.Ejemplos.Add(ejemplo);
            await _context.SaveChangesAsync();

            return ejemplo.Id;
        }

        /// <summary>
        /// Actualiza un ejemplo por id. Devuelve false si no existe.
        /// </summary>
        public async Task<bool> UpdateAsync(Guid id, EjemploUpdateDto dto)
        {
            var ejemplo = await _context.Ejemplos.FirstOrDefaultAsync(e => e.Id == id);
            if (ejemplo is null)
            {
                return false;
            }

            if (dto.Name is not null)
            {
                ejemplo.Name = dto.Name;
            }

            if (dto.Date.HasValue)
            {
                ejemplo.Date = dto.Date.Value;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Elimina un ejemplo por id. Devuelve false si no existe.
        /// </summary>
        public async Task<bool> DeleteAsync(Guid id)
        {
            var ejemplo = await _context.Ejemplos.FirstOrDefaultAsync(e => e.Id == id);
            if (ejemplo is null)
            {
                return false;
            }

            _context.Ejemplos.Remove(ejemplo);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

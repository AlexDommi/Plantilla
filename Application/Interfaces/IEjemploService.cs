using Application.Dtos.Ejemplo;

namespace Application.Interfaces
{
    public interface IEjemploService : IGenericService<EjemploCreateDto, EjemploReadDto, EjemploUpdateDto>
    {
    }
}

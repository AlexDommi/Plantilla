namespace Application.Interfaces
{
    public interface IGenericService<TCreateDTO, TReadDTO, TUpdateDTO>
        where TCreateDTO : class
        where TReadDTO : class, new()
        where TUpdateDTO : class
    {
        /// <summary>
        /// Devuelve todos los registros como TReadDTO.
        /// </summary>
        Task<IEnumerable<TReadDTO>> GetAllAsync();

        /// <summary>
        /// Devuelve un TReadDTO por su id, o null si no existe.
        /// </summary>
        Task<TReadDTO?> GetByIdAsync(Guid id);

        /// <summary>
        /// Agrega un TCreateDTO y devuelve el id del registro creado.
        /// </summary>
        Task<Guid> AddAsync(TCreateDTO dto);

        /// <summary>
        /// Actualiza el registro indicado y devuelve true si la actualizacion fue exitosa.
        /// </summary>
        Task<bool> UpdateAsync(Guid id, TUpdateDTO dto);

        /// <summary>
        /// Elimina el registro indicado y devuelve true si existia.
        /// </summary>
        Task<bool> DeleteAsync(Guid id);
    }
}

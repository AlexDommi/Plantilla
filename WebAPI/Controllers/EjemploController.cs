using Application.Dtos.Ejemplo;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EjemploController : ControllerBase
    {
        private readonly IEjemploService _ejemploService;

        public EjemploController(IEjemploService ejemploService)
        {
            _ejemploService = ejemploService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var ejemplos = await _ejemploService.GetAllAsync();
            return Ok(ejemplos);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var ejemplo = await _ejemploService.GetByIdAsync(id);
            if (ejemplo is null)
            {
                return NotFound();
            }

            return Ok(ejemplo);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EjemploCreateDto dto)
        {
            var id = await _ejemploService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] EjemploUpdateDto dto)
        {
            var actualizado = await _ejemploService.UpdateAsync(id, dto);
            if (!actualizado)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var eliminado = await _ejemploService.DeleteAsync(id);
            if (!eliminado)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}

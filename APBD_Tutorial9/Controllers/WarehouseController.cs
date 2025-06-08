using Microsoft.AspNetCore.Mvc;
using APBD_Tutorial9.Model;
using APBD_Tutorial9.Services;

namespace APBD_Tutorial9.Controllers;

    [ApiController]
    [Route("api/[controller]")]
    public class WarehouseController : ControllerBase
    {
        private readonly IWarehouseService _service;

        public WarehouseController(IWarehouseService service)
        {
            _service = service;
        }

        [HttpPost("manual")]
        public async Task<IActionResult> AddProductManually([FromBody] ProductWarehouseRequest request)
        {
            try
            {
                int newId = await _service.AddProductManually(request);
                return Created($"api/warehouse/manual/{newId}", newId);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("procedure")]
        public async Task<IActionResult> AddProductWithProcedure([FromBody] ProductWarehouseRequest request)
        {
            try
            {
                int newId = await _service.AddProductWithProcedure(request);
                return Created($"api/warehouse/procedure/{newId}", newId);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

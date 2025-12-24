using AutoMapper;
using MagicVilla_API.Data;
using MagicVilla_API.Models;
using MagicVilla_API.Models.DTO;
using MagicVilla_API.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MagicVilla_API.Controllers.v1
{
    //[Route("api/[controller]")]
    //[Route("api/VillaAPI")]
    [Route("api/v{version:apiVersion}/VillaAPI")]
    [ApiVersion("1.0")]
    [ApiController]
    public class VillaAPIController : ControllerBase
    {
        private readonly ILogger<VillaAPIController> _logger;
        private readonly IMapper _mapper;
        private readonly IVillaRepository _dbVilla;
        protected APIResponse _response;
        public VillaAPIController(ILogger<VillaAPIController> logger, ApplicationDbContext db, IMapper mapper, IVillaRepository dbVilla)
        {
            _logger = logger;
            _mapper = mapper;
            _dbVilla = dbVilla;
            _response = new APIResponse();
        }

        //[MapToApiVersion("1.0")]
        [Authorize]
        [HttpGet(Name = "GetAllVillas")]
        //[ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(IEnumerable<APIResponse>))]
        [ProducesResponseType(statusCode: StatusCodes.Status200OK)]
        [ProducesResponseType(statusCode: StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse>> GetAllVillas()
        {
            try
            {
                var villas = await _dbVilla.GetAllAsync();
                if (villas == null)
                {
                    _logger.LogError("Getting All Villas With Error");
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                _logger.LogInformation("Get All Villas");
                _response.Result = _mapper.Map<List<VillaDTO>>(villas);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
            }
            return _response;
        }

        [Authorize]
        [HttpGet("{id=int}", Name = "GetVilla")]
        [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest)]
        [ProducesResponseType(statusCode: StatusCodes.Status404NotFound)]
        [ProducesResponseType(statusCode: StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse>> GetVilla(int id)
        {
            try
            {
                if (id == 0)
                {
                    _logger.LogError("Getting All Villas With Error And Id:" + id);
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);

                }

                var Villa = await _dbVilla.GetAsync(u => u.Id == id);

                if (Villa == null)
                {
                    _logger.LogError("Getting All Villas With Error And Id:" + id);
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                _logger.LogInformation("Get Villa By Id:" + id);
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = _mapper.Map<VillaDTO>(Villa);
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
            }
            return _response;
        }

        [Authorize]
        [HttpPost(Name = "CreateVilla")]
        [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest)]
        [ProducesResponseType(statusCode: StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(statusCode: StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse>> CreateVilla([FromBody] VillaCreateDTO createDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    //return BadRequest(ModelState);

                    _response.StatusCode = HttpStatusCode.BadRequest;
                    //_response.Errors = ModelState
                    return BadRequest(_response);
                }
                if (createDTO == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                if (await _dbVilla.GetAsync(u => u.Name.ToLower() == createDTO.Name.ToLower()) != null)
                {
                    ModelState.AddModelError("ErrorMessages", "Villa already Exists!");
                    //return BadRequest(ModelState);


                    _response.StatusCode = HttpStatusCode.BadRequest;
                    //_response.Errors = ModelState
                    return BadRequest(_response);
                }
                //everything Is Ok
                Villa villa = _mapper.Map<Villa>(createDTO);
                await _dbVilla.CreateAsync(villa);


                //return Ok(villaDTO);
                //return CreatedAtRoute("GetVilla", new { id = villa.Id }, villa);

                _response.StatusCode = HttpStatusCode.Created;
                _response.IsSuccess = true;
                return CreatedAtRoute("GetVilla", new { id = villa.Id }, _response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
            }
            return _response;
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id=int}", Name = "DeleteVilla")]
        [ProducesResponseType(statusCode: StatusCodes.Status204NoContent)]
        [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest)]
        [ProducesResponseType(statusCode: StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse>> DeleteVilla(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                Villa villa = await _dbVilla.GetAsync(u => u.Id == id);
                if (villa == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                await _dbVilla.RemoveAsync(villa);
                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
            }
            return _response;
        }

        [Authorize]
        [HttpPut("{id=int}", Name = "UpdateVilla")]
        [ProducesResponseType(statusCode: StatusCodes.Status204NoContent)]
        [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse>> UpdateVilla(int id, [FromBody] VillaUpdateDTO updateDTO)
        {
            try
            {


                if (id != updateDTO.Id || updateDTO.Id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                Villa model = _mapper.Map<Villa>(updateDTO);
                _dbVilla.UpdateAsync(model);
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
            }
            return _response;
        }

        [Authorize]
        [HttpPatch("{id=int}", Name = "UpdatePartialVilla")]
        [ProducesResponseType(statusCode: StatusCodes.Status204NoContent)]
        [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest)]
        [ProducesResponseType(statusCode: StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdatePartialVilla(int id, [FromBody] JsonPatchDocument<VillaUpdateDTO> patchDTO)
        {
            if (id == 0 || patchDTO == null)
            {
                return BadRequest();
            }
            var villa = await _dbVilla.GetAsync(v => v.Id == id, false);
            VillaUpdateDTO villaDTO = _mapper.Map<VillaUpdateDTO>(villa);

            if (villa == null)
            {
                return NotFound();
            }

            patchDTO.ApplyTo(villaDTO, ModelState);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // every thing is ok
            Villa model = _mapper.Map<Villa>(villaDTO);
            _dbVilla.UpdateAsync(model);


            return NoContent();

        }
    }
}

using AutoMapper;
using MagicVilla_API.Data;
using MagicVilla_API.Models;
using MagicVilla_API.Models.DTO;
using MagicVilla_API.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace MagicVilla_API.Controllers.v2
{
    //[Route("api/[controller]")]
    //[Route("api/VillaAPI")]
    [Route("api/v{version:apiVersion}/VillaAPI")]
    [ApiVersion("2.0")]
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

      

        //[MapToApiVersion("2.0")]
      
        [HttpGet(Name = "GetAllVillas")]
        //[ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(IEnumerable<APIResponse>))]
        [ProducesResponseType(statusCode: StatusCodes.Status200OK)]
        [ProducesResponseType(statusCode: StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //[ResponseCache(CacheProfileName = "CacheProfile1")]
        public async Task<ActionResult<APIResponse>> GetAllVillasV2([FromQuery(Name = "filterOccupancy")] int? occupancy
            , [FromQuery(Name ="PageSize")] int pageSize =0 ,int pageNumber = 1)
        {
            try
            {
                IEnumerable<Villa> villas = null;
                if (occupancy != null)
                {
                     villas = await _dbVilla.GetAllAsync(u=>u.Occupancy == occupancy,pageSize:pageSize,pageNumber:pageNumber);
                }
                else
                {
                     villas = await _dbVilla.GetAllAsync(pageSize: pageSize, pageNumber: pageNumber);
                }
                if (villas == null)
                {
                    _logger.LogError("Getting All Villas With Error");
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                Pagination pagination = new Pagination()
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                };

                //Add To Headers
                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(pagination));



                _logger.LogInformation("Get All Villas");
                _response.Result = _mapper.Map<List<VillaDTO>>(villas).OrderBy(u=>u.Sqft).ToList();
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
        public async Task<ActionResult<APIResponse>> CreateVilla([FromForm] VillaCreateDTO createDTO)
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

                //Save Image From FormFile
                /* *********************************************************************************************** */
                if (createDTO.Image != null)
                {
                    string fileName = villa.Id + Path.GetExtension(createDTO.Image.FileName);
                    string filePath = @"wwwroot\ProductImage\" + fileName;

                    var directoryLocation = Path.Combine(Directory.GetCurrentDirectory(), filePath);

                    FileInfo file = new FileInfo(directoryLocation);

                    if (file.Exists)
                    {
                        file.Delete();
                    }

                    using (var fileStream = new FileStream(directoryLocation, FileMode.Create))
                    {
                        createDTO.Image.CopyTo(fileStream);
                    }

                    var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                    villa.ImageUrl = baseUrl + "/ProductImage/" + fileName;
                    villa.ImageLocalPath = filePath;

                }
                else
                {
                    villa.ImageUrl = "https://placehold.co/600x400";
                }

                await _dbVilla.UpdateAsync(villa);
                _response.Result = _mapper.Map<VillaDTO>(villa);
                /* *********************************************************************************************** */



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

                /* ************************************************************************************************************************************* */
                // remove image
                if (!string.IsNullOrEmpty(villa.ImageLocalPath))
                {
                    var oldFilePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), villa.ImageLocalPath);
                    FileInfo file = new FileInfo(oldFilePathDirectory);

                    if (file.Exists)
                    {
                        file.Delete();
                    }
                }
                /* ************************************************************************************************************************************* */

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
        public async Task<ActionResult<APIResponse>> UpdateVilla(int id, [FromForm] VillaUpdateDTO updateDTO)
        {
            try
            {


                if (id != updateDTO.Id || updateDTO.Id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                Villa model = _mapper.Map<Villa>(updateDTO);
                /* ************************************************************************************************************************************* */

                if (updateDTO.Image != null)
                {
                    if (!string.IsNullOrEmpty(model.ImageLocalPath))
                    {
                        var oldFilePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), model.ImageLocalPath);
                        FileInfo file = new FileInfo(oldFilePathDirectory);

                        if (file.Exists)
                        {
                            file.Delete();
                        }
                    }

                    string fileName = updateDTO.Id + Path.GetExtension(updateDTO.Image.FileName);
                    string filePath = @"wwwroot\ProductImage\" + fileName;

                    var directoryLocation = Path.Combine(Directory.GetCurrentDirectory(), filePath);

                    using (var fileStream = new FileStream(directoryLocation, FileMode.Create))
                    {
                        updateDTO.Image.CopyTo(fileStream);
                    }

                    var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                    model.ImageUrl = baseUrl + "/ProductImage/" + fileName;
                    model.ImageLocalPath = filePath;

                }
                else
                {
                    model.ImageUrl = "https://placehold.co/600x400";
                }
                /* ************************************************************************************************************************************* */


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

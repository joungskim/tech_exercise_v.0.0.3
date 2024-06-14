using MediatR;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;
using System.Net;

namespace StargateAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AstronautDutyController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AstronautDutyController> _logger;

        public AstronautDutyController(IMediator mediator, ILogger<AstronautDutyController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves astronaut duties by person's name.
        /// </summary>
        /// <param name="name">The name of the person.</param>
        /// <returns>A list of astronaut duties if duties exist.</returns>
        [HttpGet("{name}")]
        public async Task<IActionResult> GetAstronautDutiesByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("Invalid name: is null or white space");
                return BadRequest(new BaseResponse
                {
                    Success = false,
                    Message = "Name cannot be empty.",
                    ResponseCode = (int)HttpStatusCode.BadRequest
                });
            }

            try
            {
                var result = await _mediator.Send(new GetAstronautDutiesByName
                {
                    Name = name
                });
                if (result.Person == null)
                {
                    _logger.LogWarning($"No astronaut duties found for person: {name}");
                    return NotFound(new BaseResponse
                    {
                        Success = false,
                        Message = $"Person with name {name} not found.",
                        ResponseCode = (int)HttpStatusCode.NotFound
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving astronaut duties for {name}.");
                return StatusCode((int)HttpStatusCode.InternalServerError, new BaseResponse
                {
                    Message = ex.Message,
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

        /// <summary>
        /// Creates a new astronaut duty.
        /// </summary>
        /// <param name="request">The request containing the new astronaut duty details.</param>
        /// <returns>The result of the creation operation.</returns>
        [HttpPost("")]
        public async Task<IActionResult> CreateAstronautDuty([FromBody] CreateAstronautDuty request)
        {
            if (request == null)
            {
                _logger.LogWarning("Invalid request body.");
                return BadRequest(new BaseResponse
                {
                    Success = false,
                    Message = "Request body cannot be empty.",
                    ResponseCode = (int)HttpStatusCode.BadRequest
                });
            }

            try
            {
                var result = await _mediator.Send(request);
                if (!result.Success)
                {
                    _logger.LogWarning($"Failed to create astronaut duty for {request.Name}: {result.Message}");
                    return BadRequest(new BaseResponse
                    {
                        Success = false,
                        Message = result.Message,
                        ResponseCode = (int)HttpStatusCode.BadRequest
                    });
                }

                _logger.LogInformation($"Successfully created astronaut duty for {request.Name}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while creating astronaut duty for {request.Name}.");
                return StatusCode((int)HttpStatusCode.InternalServerError, new BaseResponse
                {
                    Message = ex.Message,
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }
    }
}

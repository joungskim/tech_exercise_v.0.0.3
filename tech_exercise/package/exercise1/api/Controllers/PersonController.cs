using MediatR;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;
using System.Net;

namespace StargateAPI.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class PersonController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PersonController> _logger;

        public PersonController(IMediator mediator, ILogger<PersonController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all people.
        /// </summary>
        /// <returns>A list of people.</returns>
        [HttpGet("")]
        public async Task<IActionResult> GetPeople()
        {
            try
            {
                var result = await _mediator.Send(new GetPeople());

                _logger.LogInformation("Successfully retrieved people.");

                return this.GetResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while retrieving people: {ex.Message}");

                return this.GetResponse(new BaseResponse()
                {
                    Message = ex.Message,
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

        /// <summary>
        /// Retrieves a person by name.
        /// </summary>
        /// <param name="name">The name of the person.</param>
        /// <returns>The person details.</returns>
        [HttpGet("{name}")]
        public async Task<IActionResult> GetPersonByName(string name)
        {
            try
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

                var result = await _mediator.Send(new GetPersonByName()
                {
                    Name = name
                });
                if (!result.Success)
                {
                    _logger.LogWarning($"Person not found: {name}");
                    return NotFound(result.Message);
                }

                _logger.LogInformation($"Successfully retrieved person: {name}");

                return this.GetResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while retrieving person {name}: {ex.Message}");

                return this.GetResponse(new BaseResponse()
                {
                    Message = ex.Message,
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

        /// <summary>
        /// Creates or updates a person.
        /// </summary>
        /// <param name="name">The name of the person.</param>
        /// <returns>The result of the creation/update operation.</returns>
        [HttpPost("")]
        public async Task<IActionResult> CreatePerson([FromBody] string name)
        {
            try
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

                var result = await _mediator.Send(new CreatePerson()
                {
                    Name = name
                });
                if (!result.Success)
                {
                    _logger.LogWarning($"Failed to create/update person: {name}");
                    return BadRequest(result.Message);
                }

                _logger.LogInformation($"Successfully created/updated person: {name}");

                return this.GetResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while creating/updating person {name}: {ex.Message}");

                return this.GetResponse(new BaseResponse()
                {
                    Message = ex.Message,
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }
    }
}
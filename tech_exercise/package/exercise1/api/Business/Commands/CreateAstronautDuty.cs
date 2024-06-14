using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Services;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Commands
{
    public class CreateAstronautDuty : IRequest<CreateAstronautDutyResult>
    {
        public required string Name { get; set; }
        public required string Rank { get; set; }
        public required string DutyTitle { get; set; }
        public DateTime DutyStartDate { get; set; }
    }

    public class CreateAstronautDutyPreProcessor : IRequestPreProcessor<CreateAstronautDuty>
    {
        private readonly StargateContext _context;

        public CreateAstronautDutyPreProcessor(StargateContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task Process(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Rank) || string.IsNullOrEmpty(request.DutyTitle))
            {
                throw new BadHttpRequestException("Name, Rank, and DutyTitle are required", StatusCodes.Status400BadRequest);
            }

            var person = await _context.People.FirstOrDefaultAsync(p => p.Name == request.Name);
            if (person != null)
            {
                var verifyNoPreviousDuty = await _context.AstronautDuties.AsNoTracking()
                    .FirstOrDefaultAsync(ad => ad.DutyTitle == request.DutyTitle && ad.DutyStartDate == request.DutyStartDate);
                if (verifyNoPreviousDuty != null)
                {
                    throw new BadHttpRequestException("A duty with the same title and start date already exists", StatusCodes.Status409Conflict);
                }
            }
            else
            {
                throw new BadHttpRequestException("Person not found", StatusCodes.Status400BadRequest);
            }
        }
    }

    public class CreateAstronautDutyHandler : IRequestHandler<CreateAstronautDuty, CreateAstronautDutyResult>
    {
        private readonly StargateContext _context;
        private readonly IAstronautDutyService _dutyService;

        public CreateAstronautDutyHandler(StargateContext context, IAstronautDutyService astronautDutyService)
        {
            _context = context;
            _dutyService = astronautDutyService;
        }

        public async Task<CreateAstronautDutyResult> Handle(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            var person = await _context.People.FirstOrDefaultAsync(p => p.Name == request.Name, cancellationToken: cancellationToken);
            if (person != null)
            {
                if (request.DutyTitle.ToUpperInvariant() == "RETIRED")
                {
                    await _dutyService.HandleRetiredDutyAsync(person, request.DutyStartDate, request.Rank);
                }
                else
                {
                    await _dutyService.UpdatePreviousDutyEndDateAsync(person.Id, request.DutyStartDate);
                    var newAstronautDuty = new AstronautDuty
                    {
                        PersonId = person.Id,
                        Rank = request.Rank,
                        DutyTitle = request.DutyTitle,
                        DutyStartDate = request.DutyStartDate.Date,
                        DutyEndDate = null, // Rule 4: A Person's Current Duty will not have a Duty End Date
                        Person = person
                    };

                    await _context.AstronautDuties.AddAsync(newAstronautDuty);
                    await _dutyService.CreateOrUpdateAstronautDetailAsync(person, request.DutyTitle, request.Rank, request.DutyStartDate);
                }

                await _context.SaveChangesAsync();

                return new CreateAstronautDutyResult();
            }

            throw new BadHttpRequestException("Person not found", StatusCodes.Status400BadRequest);
        }
    }

    // Command result object
    public class CreateAstronautDutyResult : BaseResponse
    {
        public int? Id { get; set; }
    }
}

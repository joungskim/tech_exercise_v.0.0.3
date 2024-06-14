using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;

namespace StargateAPI.Business.Services
{
    public class AstronautDutyService : IAstronautDutyService
    {
        private readonly StargateContext _context;

        public AstronautDutyService(StargateContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<AstronautDuty?> GetLatestAstronautDutyAsync(int personId)
        {
            return await _context.AstronautDuties
                .OrderByDescending(ad => ad.DutyStartDate)
                .FirstOrDefaultAsync(ad => ad.PersonId == personId && ad.DutyEndDate == null);
        }

        // Rule 5: A Person's Previous Duty End Date is set to the day before the New Astronaut Duty Start Date
        public async Task<AstronautDuty?> UpdatePreviousDutyEndDateAsync(int personId, DateTime dutyStartDate)
        {
            var previousAstronautDuty = await GetLatestAstronautDutyAsync(personId);
            if (previousAstronautDuty != null)
            {
                ValidateDutyStartDate(previousAstronautDuty.DutyStartDate, dutyStartDate);

                previousAstronautDuty.DutyEndDate = dutyStartDate.Date.AddDays(-1);
                _context.AstronautDuties.Update(previousAstronautDuty);
            }
            return previousAstronautDuty;
        }

        // Rule 3: A Person will only ever hold one current Astronaut Duty Title, Start Date, and Rank at a time.
        public async Task<AstronautDetail> CreateOrUpdateAstronautDetailAsync(Person person, string dutyTitle, string rank, DateTime dutyStartDate)
        {
            ValidatePerson(person);
            ValidateStringParameter(dutyTitle, nameof(dutyTitle));
            ValidateStringParameter(rank, nameof(rank));

            var astronautDetail = await _context.AstronautDetails.FirstOrDefaultAsync(ad => ad.PersonId == person.Id);
            if (astronautDetail == null)
            {
                astronautDetail = new AstronautDetail
                {
                    PersonId = person.Id,
                    CurrentDutyTitle = dutyTitle,
                    CurrentRank = rank,
                    CareerStartDate = dutyStartDate,
                    CareerEndDate = null,
                    Person = person
                };
                await _context.AstronautDetails.AddAsync(astronautDetail);
            }
            else
            {
                DateTime careerStartDate = await _context.AstronautDuties
                    .Where(ad => ad.PersonId == person.Id)
                    .MinAsync(ad => ad.DutyStartDate);
                astronautDetail.CurrentDutyTitle = dutyTitle;
                astronautDetail.CurrentRank = rank;
                astronautDetail.CareerStartDate = careerStartDate;
                astronautDetail.CareerEndDate = null;
                _context.AstronautDetails.Update(astronautDetail);
            }

            return astronautDetail;
        }

        // Rule 6: A Person is classified as 'Retired' when a Duty Title is 'RETIRED'.
        // Rule 7: A Person's Career End Date is one day before the Retired Duty Start Date.
        public async Task<AstronautDuty?> HandleRetiredDutyAsync(Person person, DateTime dutyStartDate, string rank)
        {
            ValidatePerson(person);
            ValidateStringParameter(rank, nameof(rank));

            var currentAstronautDetail = await _context.AstronautDetails.FirstOrDefaultAsync(ad => ad.PersonId == person.Id);
            if (currentAstronautDetail != null)
            {
                currentAstronautDetail.CurrentDutyTitle = "RETIRED";
                currentAstronautDetail.CareerEndDate = dutyStartDate.Date.AddDays(-1);
                currentAstronautDetail.CurrentRank = rank;
                _context.AstronautDetails.Update(currentAstronautDetail);

                var previousDuty = await UpdatePreviousDutyEndDateAsync(person.Id, dutyStartDate);
                var newAstronautDuty = new AstronautDuty
                {
                    PersonId = person.Id,
                    Rank = rank,
                    DutyTitle = "RETIRED",
                    DutyStartDate = dutyStartDate.Date,
                    DutyEndDate = null,
                    Person = person
                };
                await _context.AstronautDuties.AddAsync(newAstronautDuty);

                return newAstronautDuty;
            }
            else
            {
                throw new BadHttpRequestException("No active astronaut duty found to retire", StatusCodes.Status400BadRequest);
            }
        }

        private void ValidatePerson(Person person)
        {
            if (person == null)
            {
                throw new ArgumentNullException(nameof(person));
            }
            if (person.Id <= 0)
            {
                throw new ArgumentException("Person Id must be greater than zero", nameof(person));
            }
        }

        private void ValidateStringParameter(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"'{parameterName}' cannot be null or empty", nameof(value));
            }
        }

        private void ValidateDutyStartDate(DateTime previousDutyStartDate, DateTime dutyStartDate)
        {
            if (previousDutyStartDate >= dutyStartDate)
            {
                throw new BadHttpRequestException("Duty start date must be after the end date of the previous duty start date", StatusCodes.Status400BadRequest);
            }
        }
    }
}

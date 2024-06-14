using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using StargateAPI.Business.Data;
using StargateAPI.Business.Services;

namespace StargateApi.Test
{
    [TestFixture]
    public class AstronautDutyServiceTests
    {
        private StargateContext _context;
        private AstronautDutyService _service;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<StargateContext>()
                .UseInMemoryDatabase(databaseName: "StargateTestDb")
                .Options;

            _context = new StargateContext(options);
            _service = new AstronautDutyService(_context);
        }

        [TearDown]
        public void Teardown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task GetLatestAstronautDutyAsync_ReturnsCorrectDuty()
        {
            var personId = 1;
            var duties = new List<AstronautDuty>
        {
            new AstronautDuty { Id = 1, PersonId = personId, DutyStartDate = new DateTime(2023, 1, 1) },
            new AstronautDuty { Id = 2, PersonId = personId, DutyStartDate = new DateTime(2023, 2, 1), DutyEndDate = null }
        };

            _context.AstronautDuties.AddRange(duties);
            await _context.SaveChangesAsync();

            var result = await _service.GetLatestAstronautDutyAsync(personId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(2));
        }

        [Test]
        public async Task UpdatePreviousDutyEndDateAsync_UpdatesEndDate()
        {
            var personId = 1;
            var dutyStartDate = new DateTime(2023, 3, 1);
            var duties = new List<AstronautDuty>
        {
            new AstronautDuty { Id = 1, PersonId = personId, DutyStartDate = new DateTime(2023, 1, 1), DutyEndDate = null }
        };

            _context.AstronautDuties.AddRange(duties);
            await _context.SaveChangesAsync();

            var previousDuty = await _service.UpdatePreviousDutyEndDateAsync(personId, dutyStartDate);
            await _context.SaveChangesAsync(); // Save the changes
            var updatedDuty = await _context.AstronautDuties.FindAsync(1);

            Assert.That(updatedDuty.DutyEndDate, Is.EqualTo(dutyStartDate.AddDays(-1)));
        }

        [Test]
        public void UpdatePreviousDutyEndDateAsync_InvalidDates_ThrowsException()
        {
            var personId = 1;
            var dutyStartDate = new DateTime(2023, 1, 1);
            var duties = new List<AstronautDuty>
        {
            new AstronautDuty { Id = 1, PersonId = personId, DutyStartDate = new DateTime(2023, 2, 1), DutyEndDate = null }
        };

            _context.AstronautDuties.AddRange(duties);
            _context.SaveChanges();
 & Assert
            var ex = Assert.ThrowsAsync<BadHttpRequestException>(() => _service.UpdatePreviousDutyEndDateAsync(personId, dutyStartDate));
            Assert.That(ex.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
            Assert.That(ex.Message, Is.EqualTo("Duty start date must be after the end date of the previous duty start date"));
        }

        [Test]
        public async Task CreateOrUpdateAstronautDetailAsync_CreatesDetail()
        {
            var person = new Person { Id = 1, Name = "John Doe" };
            var dutyTitle = "Commander";
            var rank = "Captain";
            var dutyStartDate = new DateTime(2023, 1, 1);

            var detail = await _service.CreateOrUpdateAstronautDetailAsync(person, dutyTitle, rank, dutyStartDate);
            await _context.SaveChangesAsync(); // Save the changes
            detail = await _context.AstronautDetails.FirstOrDefaultAsync(ad => ad.PersonId == person.Id);

            Assert.That(detail, Is.Not.Null);
            Assert.That(detail.CurrentDutyTitle, Is.EqualTo(dutyTitle));
            Assert.That(detail.CurrentRank, Is.EqualTo(rank));
            Assert.That(detail.CareerStartDate, Is.EqualTo(dutyStartDate));
        }

        [Test]
        public async Task CreateOrUpdateAstronautDetailAsync_UpdatesDetail()
        {
            var person = new Person { Id = 1, Name = "John Doe" };
            var dutyTitle = "Commander";
            var rank = "Captain";
            var dutyStartDate = new DateTime(2023, 1, 1);
            var duties = new List<AstronautDuty>
        {
            new AstronautDuty { Id = 1, PersonId = person.Id, DutyStartDate = new DateTime(2022, 1, 1) }
        };

            _context.AstronautDetails.Add(new AstronautDetail { Id = 1, PersonId = person.Id, CurrentDutyTitle = "Previous", CurrentRank = "Lieutenant", CareerStartDate = new DateTime(2022, 1, 1), Person = person });
            _context.AstronautDuties.AddRange(duties);
            await _context.SaveChangesAsync();

            var detail = await _service.CreateOrUpdateAstronautDetailAsync(person, dutyTitle, rank, dutyStartDate);
            await _context.SaveChangesAsync(); // Save the changes
            detail = await _context.AstronautDetails.FirstOrDefaultAsync(ad => ad.PersonId == person.Id);

            Assert.That(detail, Is.Not.Null);
            Assert.That(detail.CurrentDutyTitle, Is.EqualTo(dutyTitle));
            Assert.That(detail.CurrentRank, Is.EqualTo(rank));
            Assert.That(detail.CareerStartDate, Is.EqualTo(new DateTime(2022, 1, 1))); // The earliest duty start date
        }

        [Test]
        public async Task HandleRetiredDutyAsync_UpdatesAstronautDetail()
        {
            var person = new Person { Id = 1, Name = "John Doe" };
            var dutyStartDate = new DateTime(2023, 1, 1);
            var rank = "Captain";

            _context.AstronautDetails.Add(new AstronautDetail { Id = 1, PersonId = person.Id, CurrentDutyTitle = "Commander", CurrentRank = "Captain", CareerStartDate = new DateTime(2022, 1, 1), Person = person });
            await _context.SaveChangesAsync();

            var newDuty = await _service.HandleRetiredDutyAsync(person, dutyStartDate, rank);
            await _context.SaveChangesAsync(); // Save the changes
            var detail = await _context.AstronautDetails.FirstOrDefaultAsync(ad => ad.PersonId == person.Id);

            Assert.That(newDuty, Is.Not.Null);
            Assert.That(newDuty.DutyTitle, Is.EqualTo("RETIRED"));
            Assert.That(detail, Is.Not.Null);
            Assert.That(detail.CurrentDutyTitle, Is.EqualTo("RETIRED"));
            Assert.That(detail.CareerEndDate, Is.EqualTo(dutyStartDate.Date.AddDays(-1)));
        }

        // TODO: Add more paths to test for better coverage
    }

}
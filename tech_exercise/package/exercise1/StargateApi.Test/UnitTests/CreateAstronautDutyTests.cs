using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Business.Services;
using Microsoft.AspNetCore.Http;

namespace StargateApi.Tests
{
    [TestFixture]
    public class CreateAstronautDutyTests
    {
        private StargateContext _context;
        private Mock<IAstronautDutyService> _mockDutyService;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<StargateContext>()
                .UseInMemoryDatabase(databaseName: "StargateTestDb")
                .Options;

            _context = new StargateContext(options);
            _mockDutyService = new Mock<IAstronautDutyService>();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // Tests for CreateAstronautDutyHandler

        [Test]
        public async Task Handle_ValidRequest_AddsNewAstronautDuty()
        {
            var request = new CreateAstronautDuty
            {
                Name = "John Doe",
                Rank = "Commander",
                DutyTitle = "Mission Specialist",
                DutyStartDate = DateTime.Now.Date
            };

            var existingPerson = new Person { Id = 1, Name = "John Doe" };
            _context.People.Add(existingPerson);
            await _context.SaveChangesAsync();

            var mockAstronautDetail = new AstronautDetail
            {
                PersonId = existingPerson.Id,
                CurrentDutyTitle = request.DutyTitle,
                CurrentRank = request.Rank,
                CareerStartDate = request.DutyStartDate,
                CareerEndDate = null,
                Person = existingPerson
            };

            _mockDutyService.Setup(s => s.UpdatePreviousDutyEndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new AstronautDuty());

            _mockDutyService.Setup(s => s.CreateOrUpdateAstronautDetailAsync(It.IsAny<Person>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new AstronautDetail());

            var handler = new CreateAstronautDutyHandler(_context, _mockDutyService.Object);

            var result = await handler.Handle(request, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.Not.Null);

            var astronautDuty = await _context.AstronautDuties.FirstOrDefaultAsync(ad => ad.PersonId == existingPerson.Id);
            Assert.That(astronautDuty, Is.Not.Null);
            Assert.That(astronautDuty.DutyTitle, Is.EqualTo(request.DutyTitle));
        }

        [Test]
        public async Task Handle_RetiredDuty_CallsHandleRetiredDutyAsync()
        {
            var request = new CreateAstronautDuty
            {
                Name = "John Doe",
                Rank = "Commander",
                DutyTitle = "RETIRED",
                DutyStartDate = DateTime.Now.Date
            };

            var existingPerson = new Person { Id = 1, Name = "John Doe" };
            _context.People.Add(existingPerson);
            await _context.SaveChangesAsync();

            _mockDutyService.Setup(s => s.HandleRetiredDutyAsync(existingPerson, request.DutyStartDate, request.Rank))
                            .ReturnsAsync(new AstronautDuty
                            {
                                PersonId = existingPerson.Id,
                                Rank = request.Rank,
                                DutyTitle = "RETIRED",
                                DutyStartDate = request.DutyStartDate.Date,
                                DutyEndDate = null,
                                Person = existingPerson
                            });

            var handler = new CreateAstronautDutyHandler(_context, _mockDutyService.Object);

            var result = await handler.Handle(request, CancellationToken.None);

            Assert.That(result, Is.Not.Null);

            _mockDutyService.Verify(s => s.HandleRetiredDutyAsync(existingPerson, request.DutyStartDate, request.Rank), Times.Once);
        }

        [Test]
        public void Handle_PersonNotFound_ThrowsBadHttpRequestException()
        {
            var request = new CreateAstronautDuty
            {
                Name = "Nonexistent Person",
                Rank = "Commander",
                DutyTitle = "Mission Specialist",
                DutyStartDate = DateTime.Now.Date
            };

            var handler = new CreateAstronautDutyHandler(_context, _mockDutyService.Object);

            var ex = Assert.ThrowsAsync<BadHttpRequestException>(async () => await handler.Handle(request, CancellationToken.None));
            Assert.That(ex.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
            Assert.That(ex.Message, Is.EqualTo("Person not found"));
        }


        [Test]
        [TestCase("", "", "", StatusCodes.Status400BadRequest, "Name, Rank, and DutyTitle are required")]
        [TestCase("John Doe", "", "", StatusCodes.Status400BadRequest, "Name, Rank, and DutyTitle are required")]
        [TestCase("", "Commander", "", StatusCodes.Status400BadRequest, "Name, Rank, and DutyTitle are required")]
        [TestCase("", "", "Mission Specialist", StatusCodes.Status400BadRequest, "Name, Rank, and DutyTitle are required")]
        public void Process_InvalidRequest_ThrowsBadHttpRequestException(string name, string rank, string dutyTitle, int expectedStatusCode, string expectedMessage)
        {
            var request = new CreateAstronautDuty
            {
                Name = name,
                Rank = rank,
                DutyTitle = dutyTitle
            };

            var preProcessor = new CreateAstronautDutyPreProcessor(_context);

            var ex = Assert.ThrowsAsync<BadHttpRequestException>(async () => await preProcessor.Process(request, CancellationToken.None));
            Assert.That(ex.StatusCode, Is.EqualTo(expectedStatusCode));
            Assert.That(ex.Message, Is.EqualTo(expectedMessage));
        }

        [Test]
        public void Process_PersonNotFound_ThrowsBadHttpRequestException()
        {
            var request = new CreateAstronautDuty
            {
                Name = "Nonexistent Person",
                Rank = "Commander",
                DutyTitle = "Mission Specialist"
            };

            var preProcessor = new CreateAstronautDutyPreProcessor(_context);

            var ex = Assert.ThrowsAsync<BadHttpRequestException>(async () => await preProcessor.Process(request, CancellationToken.None));
            Assert.That(ex.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
            Assert.That(ex.Message, Is.EqualTo("Person not found"));
        }

        [Test]
        public async Task Process_DuplicateDuty_ThrowsBadHttpRequestException()
        {
            var request = new CreateAstronautDuty
            {
                Name = "John Doe",
                Rank = "Commander",
                DutyTitle = "Mission Specialist",
                DutyStartDate = DateTime.Now.Date
            };

            var existingPerson = new Person { Id = 1, Name = "John Doe" };
            var existingDuty = new AstronautDuty { Id = 1, DutyTitle = "Mission Specialist", DutyStartDate = request.DutyStartDate };

            _context.People.Add(existingPerson);
            _context.People.Add(existingPerson);
            _context.AstronautDuties.Add(existingDuty);
            await _context.SaveChangesAsync();

            var preProcessor = new CreateAstronautDutyPreProcessor(_context);

            var ex = Assert.ThrowsAsync<BadHttpRequestException>(async () => await preProcessor.Process(request, CancellationToken.None));
            Assert.That(ex.StatusCode, Is.EqualTo(StatusCodes.Status409Conflict));
            Assert.That(ex.Message, Is.EqualTo("A duty with the same title and start date already exists"));
        }
        // TODO: Add more paths to test for better coverage
    }
}

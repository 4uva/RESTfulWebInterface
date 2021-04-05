using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using RESTfulWebInterface.Controllers;
using RESTfulWebInterface.Models;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Xunit;

namespace RESTfulWebInterface.Tests
{
    public class UnitTestPersonController
    {
        PersonsController controller;

        public UnitTestPersonController()
        {
            var repo = SampleData.BuildFakeRepository();
            ILogger<PersonsController> logger = new MockLogger<PersonsController>();
            controller = new PersonsController(repo, logger);
        }

        [Fact]
        public async Task GettingAllProducesAllSampleDataSucceedsAndGetsCorrectResult()
        {
            IEnumerable<Person> obtainedPersons = await controller.Get();
            IReadOnlyCollection<Person> expectedPersons = SampleData.SamplePersons;
            obtainedPersons.Should().BeEquivalentTo(expectedPersons);
        }

        [Fact]
        public async Task GettingByExistingIdSucceedsAndGetsCorrectResult()
        {
            ActionResult<Person> obtainedResult = await controller.Get(1);
            Person obtainedPerson = obtainedResult.Value;
            Person expectedPerson = SampleData.SamplePersons.Single(p => p.Id == 1);
            obtainedPerson.Should().BeEquivalentTo(expectedPerson);
        }

        [Fact]
        public async Task GettingByNonExistingIdReturnsNotFoundResult()
        {
            var nonExistingId = SampleData.SamplePersons.Max(p => p.Id) + 1;
            var obtained = await controller.Get(nonExistingId);
            obtained.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GettingByExistingColorSucceedsAndBringsCorrectResult()
        {
            Color existingColor = SampleData.SamplePersons.First().Color;
            ActionResult<IEnumerable<Person>> obtained = await controller.GetByColorId(existingColor);
            IEnumerable<Person> expectedPersons = SampleData.SamplePersons.Where(p => p.Color == existingColor);
            obtained.Result.Should().BeOfType<OkObjectResult>();
            var typedResult = (OkObjectResult)obtained.Result;
            typedResult.Value.Should().BeEquivalentTo(expectedPersons);
        }
        
        [Fact]
        public async Task GettingByNonExistingButValidColorSucceedsAndBringsEmptyList()
        {
            int nonExistingColorNo = SampleData.SamplePersons.Max(p => (int)p.Color) + 1;
            Color color = (Color)nonExistingColorNo;
            if (!Enum.IsDefined(color))
                throw new InvalidOperationException("Shouldn't happen: sample data cover all colors");
            ActionResult<IEnumerable<Person>> obtained = await controller.GetByColorId(color);
            obtained.Result.Should().BeOfType<OkObjectResult>();
            var typedResult = (OkObjectResult)obtained.Result;
            typedResult.Value.Should().BeAssignableTo<IEnumerable<Person>>();
            var typedValue = (IEnumerable<Person>)typedResult.Value;
            typedValue.Should().BeEmpty();
        }
        
        [Fact]
        public async Task GettingByInvalidColorFails()
        {
            var invalidColor = Enum.GetValues<Color>().Max() + 1;
            if (Enum.IsDefined(invalidColor))
                throw new InvalidOperationException("Shouldn't happen: color must be invalid");
            var obtained = await controller.GetByColorId(invalidColor);
            obtained.Result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public async Task GettingByExistingColorByStringSucceedsAndBringsCorrectResult()
        {
            var existingColor = SampleData.SamplePersons.First().Color;
            ActionResult<IEnumerable<Person>> obtained = await controller.GetByColorName(existingColor.ToDisplayString());
            IEnumerable<Person> expectedPersons = SampleData.SamplePersons.Where(p => p.Color == existingColor);
            obtained.Result.Should().BeOfType<OkObjectResult>();
            var typedResult = (OkObjectResult)obtained.Result;
            typedResult.Value.Should().BeEquivalentTo(expectedPersons);
        }

        [Fact]
        public async Task GettingByNonExistingButValidColorByStringSucceedsAndBringsEmpty()
        {
            int nonExistingColorNo = SampleData.SamplePersons.Max(p => (int)p.Color) + 1;
            Color color = (Color)nonExistingColorNo;
            if (!Enum.IsDefined(color))
                throw new InvalidOperationException("Shouldn't happen: sample data cover all colors");
            ActionResult<IEnumerable<Person>> obtained = await controller.GetByColorName(color.ToDisplayString());
            obtained.Result.Should().BeOfType<OkObjectResult>();
            var typedResult = (OkObjectResult)obtained.Result;
            typedResult.Value.Should().BeAssignableTo<IEnumerable<Person>>();
            var typedValue = (IEnumerable<Person>)typedResult.Value;
            typedValue.Should().BeEmpty();
        }

        [Fact]
        public async Task GettingByInvalidColorStringFails()
        {
            var invalidColor = "nonexistingcolor";
            var obtained = await controller.GetByColorName(invalidColor);
            obtained.Result.Should().BeOfType<BadRequestResult>();
        }
    }
}

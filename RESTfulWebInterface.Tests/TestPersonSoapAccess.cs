using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using RESTfulWebInterface.Models;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Options;

using Xunit;

namespace RESTfulWebInterface.Tests
{
    public class TestPersonSoapAccess : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        readonly CustomWebApplicationFactory<Startup> factory;
        readonly HttpClient client;
        readonly JsonSerializerOptions jsonOptions;

        public TestPersonSoapAccess(CustomWebApplicationFactory<Startup> factory)
        {
            this.factory = factory;
            var opt = (IOptions<JsonOptions>)factory.Services.GetService(typeof(IOptions<JsonOptions>));
            jsonOptions = opt.Value.JsonSerializerOptions;
            this.client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task GettingAllProducesAllSampleDataSucceedsAndGetsCorrectResult()
        {
            var response = await client.GetAsync("persons");
            response.IsSuccessStatusCode.Should().BeTrue();
            var contentStream = await response.Content.ReadAsStreamAsync();
            var obtainedPersons = await JsonSerializer.DeserializeAsync<List<Person>>(contentStream, jsonOptions);
            var expectedPersons = SampleData.SamplePersons;
            obtainedPersons.Should().BeEquivalentTo(expectedPersons);
        }

        [Fact]
        public async Task GettingByExistingIdSucceedsAndGetsCorrectResult()
        {
            var response = await client.GetAsync("persons/1");
            response.IsSuccessStatusCode.Should().BeTrue();
            var contentStream = await response.Content.ReadAsStreamAsync();
            var obtainedPerson = await JsonSerializer.DeserializeAsync<Person>(contentStream, jsonOptions);
            var expectedPerson = SampleData.SamplePersons.Single(p => p.Id == 1);
            obtainedPerson.Should().BeEquivalentTo(expectedPerson);
        }

        [Fact]
        public async Task GettingByNonExistingIdFailsWith404()
        {
            var nonExistingId = SampleData.SamplePersons.Max(p => p.Id) + 1;
            var response = await client.GetAsync($"persons/{nonExistingId}");
            response.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task GettingByExistingColorSucceedsAndBringsCorrectResult()
        {
            var existingColor = SampleData.SamplePersons.First().Color;
            var response = await client.GetAsync($"persons/color/{(int)existingColor}");
            response.IsSuccessStatusCode.Should().BeTrue();
            var contentStream = await response.Content.ReadAsStreamAsync();
            var obtainedPersons = await JsonSerializer.DeserializeAsync<List<Person>>(contentStream, jsonOptions);
            var expectedPersons = SampleData.SamplePersons.Where(p => p.Color == existingColor);
            obtainedPersons.Should().BeEquivalentTo(expectedPersons);
        }

        [Fact]
        public async Task GettingByNonExistingButValidColorSucceedsAndBringsEmptyList()
        {
            var nonExistingColorNo = SampleData.SamplePersons.Max(p => (int)p.Color) + 1;
            var color = (Color)nonExistingColorNo;
            if (!Enum.IsDefined(color))
                throw new InvalidOperationException("Shouldn't happen: sample data cover all colors");
            var response = await client.GetAsync($"persons/color/{nonExistingColorNo}");
            response.IsSuccessStatusCode.Should().BeTrue();
            var contentStream = await response.Content.ReadAsStreamAsync();
            var obtainedPersons = await JsonSerializer.DeserializeAsync<List<Person>>(contentStream, jsonOptions);
            obtainedPersons.Should().BeEmpty();
        }

        [Fact]
        public async Task GettingByInvalidColorFails()
        {
            var invalidColor = Enum.GetValues<Color>().Max() + 1;
            if (Enum.IsDefined(invalidColor))
                throw new InvalidOperationException("Shouldn't happen: color must be invalid");
            var response = await client.GetAsync($"persons/color/{(int)invalidColor}");
            response.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task GettingByExistingColorByStringSucceedsAndBringsCorrectResult()
        {
            var existingColor = SampleData.SamplePersons.First().Color;
            var response = await client.GetAsync($"persons/color/{existingColor.ToDisplayString()}");
            response.IsSuccessStatusCode.Should().BeTrue();
            var contentStream = await response.Content.ReadAsStreamAsync();
            var obtainedPersons = await JsonSerializer.DeserializeAsync<List<Person>>(contentStream, jsonOptions);
            var expectedPersons = SampleData.SamplePersons.Where(p => p.Color == existingColor);
            obtainedPersons.Should().BeEquivalentTo(expectedPersons);
        }

        [Fact]
        public async Task GettingByNonExistingButValidColorByStringSucceedsAndBringsEmpty()
        {
            var nonExistingColorNo = SampleData.SamplePersons.Max(p => (int)p.Color) + 1;
            var color = (Color)nonExistingColorNo;
            if (!Enum.IsDefined(color))
                throw new InvalidOperationException("Shouldn't happen: sample data cover all colors");
            var response = await client.GetAsync($"persons/color/{color.ToDisplayString()}");
            response.IsSuccessStatusCode.Should().BeTrue();
            var contentStream = await response.Content.ReadAsStreamAsync();
            var obtainedPersons = await JsonSerializer.DeserializeAsync<List<Person>>(contentStream, jsonOptions);
            obtainedPersons.Should().BeEmpty();
        }

        [Fact]
        public async Task GettingByInvalidColorStringFails()
        {
            var invalidColor = "nonexistingcolor";
            var response = await client.GetAsync($"persons/color/{invalidColor}");
            response.StatusCode.Should().Be(400);
        }
    }
}

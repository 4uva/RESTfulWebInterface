using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using RESTfulWebInterface.Models;
using RESTfulWebInterface.Persistence;
using RESTfulWebInterface.Persistence.InMemory;

namespace RESTfulWebInterface.Tests
{
    public static class SampleData
    {
        public static IReadOnlyCollection<Person> SamplePersons { get; } =
            new List<Person>()
            {
                new Person()
                {
                    Id = 1,
                    LastName = "LastName1",
                    Name = "Name1",
                    City = "City1",
                    ZipCode = "1",
                    Color = Color.Blue
                },
                new Person()
                {
                    Id = 2,
                    LastName = "LastName2",
                    Name = "Name2",
                    City = "City2",
                    ZipCode = "2",
                    Color = Color.Blue
                },
                new Person()
                {
                    Id = 3,
                    LastName = "LastName3",
                    Name = "Name3",
                    City = "City3",
                    ZipCode = "3",
                    Color = Color.Green
                },
            }.AsReadOnly();

        public static IUowRepository BuildFakeRepository()
        {
            var repo = new InMemoryRepository(SamplePersons);
            return new InMemoryContext(repo);
        }
    }
}

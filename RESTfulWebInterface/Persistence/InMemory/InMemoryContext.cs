using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using RESTfulWebInterface.Models;

namespace RESTfulWebInterface.Persistence.InMemory
{
    public class InMemoryContext : IUowRepository
    {
        readonly InMemoryRepository repo;
        readonly object mutex = new object();
        List<Person>? addedPersons = null;

        public InMemoryContext(InMemoryRepository repo)
        {
            this.repo = repo;
        }

        public Task<IReadOnlyCollection<Person>> GetAllPersons()
        {
            return Task.FromResult(repo.GetAllPersons());
        }

        public Task<Person?> TryGetPersonsById(int id)
        {
            return Task.FromResult(repo.TryGetPersonsById(id));
        }

        public Task<IReadOnlyCollection<Person>> GetPersonsByColor(Color color)
        {
            return Task.FromResult(repo.GetPersonsByColor(color));
        }

        public void AddPerson(Person person)
        {
            lock (mutex)
            {
                if (addedPersons == null)
                    addedPersons = new List<Person>();
                addedPersons.Add(person);
            }
        }

        public Task SaveChangesAsync()
        {
            lock (mutex)
            {
                if (addedPersons != null)
                    repo.AddPersons(addedPersons);
                addedPersons = null;
            }
            return Task.CompletedTask;
        }
    }
}

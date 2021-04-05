using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using RESTfulWebInterface.Models;

namespace RESTfulWebInterface.Persistence.InMemory
{
    public class InMemoryRepository
    {
        readonly List<Person> allPersons;
        readonly object mutex = new object();
        int freeId = 0;

        public InMemoryRepository(IEnumerable<Person> persons)
        {
            allPersons = persons.ToList();
            if (allPersons.Any())
                freeId = allPersons.Max(person => person.Id) + 1;
        }

        public IReadOnlyCollection<Person> GetAllPersons()
        {
            lock (mutex)
                return allPersons;
        }

        public IReadOnlyCollection<Person> GetPersonsByColor(Color color)
        {
            lock (mutex)
                return allPersons.Where(p => p.Color == color).ToList();
        }

        public Person? TryGetPersonsById(int id)
        {
            lock (mutex)
                return allPersons.FirstOrDefault(p => p.Id == id);
        }

        public void AddPersons(IEnumerable<Person> persons)
        {
            lock (mutex)
            {
                foreach (var person in persons)
                {
                    person.Id = freeId;
                    freeId++;
                    allPersons.Add(person);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using RESTfulWebInterface.Models;

namespace RESTfulWebInterface.Persistence
{
    public interface IUowRepository
    {
        Task<IReadOnlyCollection<Person>> GetAllPersons();
        Task<Person?> TryGetPersonsById(int id);
        Task<IReadOnlyCollection<Person>> GetPersonsByColor(Color color);
        void AddPerson(Person person);
        Task SaveChangesAsync();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using RESTfulWebInterface.Models;

using Microsoft.EntityFrameworkCore;

namespace RESTfulWebInterface.Persistence.EF
{
    public class PersonsContext : DbContext, IUowRepository
    {
        public PersonsContext(DbContextOptions<PersonsContext> options) : base(options)
        {
        }

        public DbSet<Person> Persons { get; set; } = null!; // will be set by the base constructor

        public async Task<IReadOnlyCollection<Person>> GetAllPersons()
        {
            return await Persons.ToListAsync();
        }

        public async Task<Person?> TryGetPersonsById(int id)
        {
            return await Persons.FindAsync(id);
        }

        public async Task<IReadOnlyCollection<Person>> GetPersonsByColor(Color color)
        {
            return await Persons.Where(p => p.Color == color).ToListAsync();
        }

        public void AddPerson(Person person)
        {
            Persons.Add(person);
        }

        public async Task SaveChangesAsync()
        {
            await SaveChangesAsync(CancellationToken.None);
        }
    }
}

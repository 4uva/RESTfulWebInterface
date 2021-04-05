using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using RESTfulWebInterface.Models;
using RESTfulWebInterface.Persistence;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace RESTfulWebInterface.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PersonsController : ControllerBase
    {
        readonly IUowRepository repo;
        readonly ILogger<PersonsController> logger;

        public PersonsController(IUowRepository repo, ILogger<PersonsController> logger)
        {
            this.repo = repo ?? throw new ArgumentNullException(nameof(repo));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: <PersonController>
        [HttpGet]
        public async Task<IEnumerable<Person>> Get()
        {
            logger.LogInformation("Getting all persons");
            var persons = await repo.GetAllPersons();
            logger.LogInformation("Getting succeeded");
            return persons;
        }

        // GET <PersonController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Person>> Get(int id)
        {
            logger.LogInformation("Getting person by id = {Id}", id);
            var person = await repo.TryGetPersonsById(id);
            if (person == null)
            {
                logger.LogInformation("Person not found by id = {Id}", id);
                return NotFound();
            }
            logger.LogInformation("Person found by id = {Id}", id);
            return person;
        }

        // GET: <PersonController>/color/1
        [HttpGet("color/{color:int}")]
        public async Task<ActionResult<IEnumerable<Person>>> GetByColorId(Color color)
        {
            logger.LogInformation("Getting persons by numeric color = {Color}", color);
            if (!Enum.IsDefined(color))
            {
                logger.LogWarning("Invalid numeric color {Color}", color);
                return BadRequest();
            }
            var persons = await repo.GetPersonsByColor(color);
            logger.LogInformation("Getting persons by numeric color = {Color} succeeded, obtained {Count} persons", color, persons.Count);
            return Ok(persons);
        }

        // GET: <PersonController>/color/blau
        [HttpGet("color/{colorName}")]
        public async Task<ActionResult<IEnumerable<Person>>> GetByColorName(string colorName)
        {
            logger.LogInformation("Getting persons by string color name = {ColorName}", colorName);
            Color? color = ColorHelper.TryParseEnumName(colorName);
            if (color == null)
            {
                logger.LogWarning("Invalid color name {ColorName}", colorName);
                return BadRequest();
            }
            var persons = await repo.GetPersonsByColor(color.Value);
            logger.LogInformation("Getting persons by color name = {ColorName} succeeded, obtained {Count} persons", colorName, persons.Count);
            return Ok(persons);
        }

        // POST api/<PersonsController>
        [HttpPost]
        public async Task<ActionResult<Person>> Post(Person person)
        {
            repo.AddPerson(person);
            await repo.SaveChangesAsync();

            return CreatedAtAction("Get", new { id = person.Id }, person);
        }
    }
}

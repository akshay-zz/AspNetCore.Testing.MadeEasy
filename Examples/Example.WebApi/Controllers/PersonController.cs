using Example.WebApi.Model;
using Microsoft.AspNetCore.Mvc;

namespace Example.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PersonController : ControllerBase
    {
        private readonly DatabaseContext context;

        public PersonController(DatabaseContext database)
        {
            this.context = database;
        }

        [HttpGet]
        public IEnumerable<Person> Get()
        {
            return context.Person!
                .Where(x => x.Status == Status.Active).ToArray();
        }

        [HttpPost]
        public bool Post(int number = 1, int status = 1)
        {
            try
            {
                var person = new Person
                {
                    Name = $"Person {number}",
                    Age = 22,
                    Status = (Status)status
                };

                context.Person!.Add(person);
                context.SaveChanges();
                return true;

            }
            catch (Exception)
            {

                return false;
            }
        }
    }
}
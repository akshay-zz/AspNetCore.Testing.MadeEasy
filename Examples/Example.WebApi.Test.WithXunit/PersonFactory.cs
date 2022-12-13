using Example.WebApi.Model;

namespace Example.WebApi.Test.WithXunit
{
	internal class PersonFactory
	{
		public static List<Person> GetPersons(int count, int startIdFrom = 1, Status status = Status.Active)
		{
			var persons = new List<Person>();

			for(int i = startIdFrom; i < count + startIdFrom; i++)
			{
				var person = new Person
				{
					Id = i,
					Name = $"Person {i}",
					Age = 20 + i,
					Status = status
				};

				persons.Add(person);
			}

			return persons;
		}

		public static Person GetPerson(int withId = 1, Status status = Status.Active)
		{
			return GetPersons(1, withId, status).First();
		}
	}
}

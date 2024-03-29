using AspNetCore.Testing.MadeEasy.EfCore.Moq;
using Example.WebApi.Controllers;
using Example.WebApi.Model;
using Moq;

namespace Example.WebApi.Test.WithXunit;

public class PersonControllerUnitTest
{
    [Fact]
    public void Get_should_return_valid_persons()
    {
        var data = PersonFactory.GetPersons(10);
        var data2 = PersonFactory.GetPersons(2, status: Status.Inactive);

        data2.AddRange(data);

        var mockContext = new Mock<DatabaseContext>();

        // using lib
        var dbset = data2.CreateDbSet<Person>();

        mockContext.Setup(x => x.Person).Returns(dbset.Object);
        var controller = new PersonController(mockContext.Object);
        var result = controller.Get();

        Assert.Equal(data, result);
    }

    [Fact]
    public void Post_should_save_persons()
    {
        var person = new Person
        {
            Name = $"Person 2",
            Age = 22,
            Status = Status.Active
        };

        var mockContext = new Mock<DatabaseContext>();

        // using lib
        var dbset = new List<Person>().CreateDbSet<Person>();
        mockContext.Setup(x => x.Person).Returns(dbset.Object);

        var controller = new PersonController(mockContext.Object);
        controller.Post(2, 1);

        mockContext.Verify(
            x => x.Person!.Add(It.Is<Person>(
                p => p.Name == person.Name &&
                p.Age == person.Age)));
        Assert.Single(mockContext.Object.Person!.ToList());
        mockContext.Verify(x => x.SaveChanges());
    }
}

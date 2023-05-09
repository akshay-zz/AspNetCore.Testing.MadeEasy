using AspNetCore.Testing.MadeEasy.EfCore.Moq;
using AspNetCore.Testing.MadeEasy.Extensions;

namespace AspNetCore.Testing.MadeEasy.Test.Extensions;

public class DatabaseRelatedExtensionsTests
{
    [Fact]
    public void Clear_should_remove_all_data()
    {
        var blog = new Blog();
        var blog2 = new Blog();
        var data = new List<Blog> { blog, blog2, new Blog() };
        var set = data.CreateDbSet<Blog>();

        set.Object.Clear();

        var result = set.Object.ToList();

        Assert.Empty(result);
    }
}


using AspNetCore.Testing.MadeEasy.EfCore.Moq;
using AspNetCore.Testing.MadeEasy.EfCore.Test;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Testing.MadeEasy.Test.UnitTest;

public class QueryTests
{
    [Fact]
    public void Can_enumerate_set()
    {
        var data = new List<Blog> { new Blog { }, new Blog { } };

        var set = data.CreateDbSet();

        var count = 0;
        foreach (var item in set.Object)
        {
            count++;
        }

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task Can_enumerate_set_async()
    {
        var data = new List<Blog> { new Blog(), new Blog() };

        var set = data.CreateDbSet();

        var count = 0;
        await set.Object.ForEachAsync(b => count++);

        Assert.Equal(2, count);
    }

    [Fact]
    public void Can_use_linq_materializer_directly_on_set()
    {
        var data = new List<Blog> { new Blog(), new Blog() };

        var set = data.CreateDbSet();

        var result = set.Object.ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Can_use_linq_materializer_directly_on_set_async()
    {
        var data = new List<Blog> { new Blog(), new Blog() };

        var set = data.CreateDbSet();

        var result = await set.Object.ToListAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Can_use_linq_opeartors()
    {
        var data = new List<Blog>
            {
                new Blog { BlogId = 1 },
                new Blog { BlogId = 2 },
                new Blog { BlogId = 3}
            };

        var set = data.CreateDbSet();

        var result = set.Object
            .Where(b => b.BlogId > 1)
            .OrderByDescending(b => b.BlogId)
            .ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(3, result[0].BlogId);
        Assert.Equal(2, result[1].BlogId);
    }

    [Fact]
    public async Task Can_use_linq_opeartors_async()
    {
        var data = new List<Blog>
            {
                new Blog { BlogId = 1 },
                new Blog { BlogId = 2 },
                new Blog { BlogId = 3}
            };

        var set = data.CreateDbSet();

        var result = await set.Object
            .Where(b => b.BlogId > 1)
            .OrderByDescending(b => b.BlogId)
            .ToListAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal(3, result[0].BlogId);
        Assert.Equal(2, result[1].BlogId);
    }

    [Fact]
    public void Can_use_include_directly_on_set()
    {
        var data = new List<Blog> { new Blog(), new Blog() };

        var set = data.CreateDbSet();

        var result = set.Object
            .Include(b => b.Posts)
            .ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Can_use_include_after_linq_operator()
    {
        var data = new List<Blog> { new Blog(), new Blog() };

        var set = data.CreateDbSet();

        var result = set.Object
            .OrderBy(b => b.BlogId)
            .Include(b => b.Posts)
            .ToList();

        Assert.Equal(2, result.Count);
    }
}

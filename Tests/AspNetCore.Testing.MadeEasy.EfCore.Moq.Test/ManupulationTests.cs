using AspNetCore.Testing.MadeEasy.EfCore.Test;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Testing.MadeEasy.EfCore.Moq.Test;


public class ManupulationTests
{
    [Fact]
    public void Can_specify_asNoTracking()
    {
        var set = (new List<Blog> { new Blog() }).CreateDbSet();

        var result = set.Object
            .AsNoTracking()
            .ToList();

        Assert.Single(result);
    }

    [Fact]
    public void Can_find_set()
    {
        var data = new List<Blog>
            {
                new Blog { BlogId = 1 },
                new Blog { BlogId = 2 },
                new Blog { BlogId = 3 }
            };

        var set = data.CreateDbSet(objs => data.FirstOrDefault(b => b.BlogId == (int)objs.First()));

        var result = set.Object
            .Find(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.BlogId);
    }

    [Fact]
    public async Task Can_find_set_async()
    {
        var data = new List<Blog>
            {
                new Blog { BlogId = 1 },
                new Blog { BlogId = 2 },
                new Blog { BlogId = 3 }
            };

        var set = data.CreateDbSet(objs => data.FirstOrDefault(b => b.BlogId == (int)objs.First()));

        var result = await set.Object
            .FindAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.BlogId);
    }

    [Fact]
    public async Task Can_find_set_async_with_cancellation_token()
    {
        var data = new List<Blog>
            {
                new Blog { BlogId = 1 },
                new Blog { BlogId = 2 },
                new Blog { BlogId = 3 }
            };

        var set = data.CreateDbSet(objs => data.FirstOrDefault(b => b.BlogId == (int)objs.First()));

        var result = await set.Object
            .FindAsync(1, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(1, result.BlogId);
    }

    [Fact]
    public void Can_add_set()
    {
        var blog = new Blog();
        var data = new List<Blog> { };

        var set = data.CreateDbSet();

        set.Object.Add(blog);

        var result = set.Object.ToList();

        Assert.Single(result);
    }

    [Fact]
    public void Can_add_set_twice()
    {
        var blog = new Blog();
        var data = new List<Blog> { };

        var set = data.CreateDbSet();

        set.Object.Add(blog);
        set.Object.Add(blog);

        var result = set.Object.ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Can_add_set_async()
    {
        var blog = new Blog();
        var data = new List<Blog> { };

        var set = data.CreateDbSet();

        await set.Object.AddAsync(blog);

        var result = set.Object.ToList();

        Assert.Single(result);
    }

    [Fact]
    public void Can_attach_set()
    {
        var blog = new Blog();
        var data = new List<Blog> { };

        var set = data.CreateDbSet();

        set.Object.Attach(blog);

        var result = set.Object.ToList();

        Assert.Single(result);
    }

    [Fact]
    public void Can_remove_set()
    {
        var blog = new Blog();
        var data = new List<Blog> { blog };

        var set = data.CreateDbSet();

        set.Object.Remove(blog);

        var result = set.Object.ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void Can_addRange_sets()
    {
        var data = new List<Blog> { new Blog(), new Blog() };

        var set = new List<Blog> { new Blog() }.CreateDbSet();

        set.Object.AddRange(data);

        var result = set.Object.ToList();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task Can_addRange_sets_async_with_params()
    {
        var set = new List<Blog> { new Blog() }.CreateDbSet();

        await set.Object.AddRangeAsync(new Blog(), new Blog());

        var result = set.Object.ToList();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void Can_attach_range_sets_async_with_params()
    {
        var set = new List<Blog> { new Blog() }.CreateDbSet();

        set.Object.AttachRange(new Blog(), new Blog());

        var result = set.Object.ToList();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void Can_remove_range_sets_async_with_params()
    {
        var blog = new Blog();
        var blog2 = new Blog();
        var range = new List<Blog> { blog, blog2 };
        var data = new List<Blog> { blog, blog2, new Blog() };

        var set = data.CreateDbSet();

        set.Object.RemoveRange(blog, blog2);

        var result = set.Object.ToList();

        Assert.Single(result);
    }

    [Fact]
    public void Can_addRange_sets_with_params()
    {
        var set = new List<Blog> { new Blog() }.CreateDbSet();

        set.Object.AddRange(new Blog(), new Blog());

        var result = set.Object.ToList();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task Can_addRange_sets_async()
    {
        var set = new List<Blog> { new Blog() }.CreateDbSet();

        await set.Object.AddRangeAsync(new List<Blog> { new Blog(), new Blog() });

        var result = set.Object.ToList();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void Can_attachRange_sets()
    {
        var set = new List<Blog> { new Blog() }.CreateDbSet();

        set.Object.AttachRange(new List<Blog> { new Blog(), new Blog() });

        var result = set.Object.ToList();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void Can_remove_range_sets()
    {
        var blog = new Blog();
        var blog2 = new Blog();
        var range = new List<Blog> { blog, blog2 };
        var data = new List<Blog> { blog, blog2, new Blog() };

        var set = data.CreateDbSet();

        set.Object.RemoveRange(range);

        var result = set.Object.ToList();

        Assert.Single(result);
    }

    [Fact]
    public void Can_toList_twice()
    {
        var set = new List<Blog> { new Blog() }.CreateDbSet();

        _ = set.Object.ToList();

        var result2 = set.Object.ToList();

        Assert.Single(result2);
    }
}

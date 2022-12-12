using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AspNetCore.Testing.MadeEasy.UnitTest;

public class MockDb
{
    public static Mock<DbSet<TEntity>> CreateDbSet<TEntity>(IQueryable<TEntity> entities) where TEntity : class
    {
        var dbSetMock = new Mock<DbSet<TEntity>>();

        var mockSet = new Mock<DbSet<TEntity>>();
        mockSet.As<IAsyncEnumerable<TEntity>>()
            .Setup(m => m.GetAsyncEnumerator(CancellationToken.None))
            .Returns(new InMemoryDbAsyncEnumerator<TEntity>(entities.GetEnumerator()));

        mockSet.As<IQueryable<TEntity>>()
           .Setup(m => m.Provider)
           .Returns(new InMemoryAsyncQueryProvider<TEntity>(entities.Provider));

        mockSet.As<IQueryable<TEntity>>().Setup(m => m.Expression).Returns(entities.Expression);
        mockSet.As<IQueryable<TEntity>>().Setup(m => m.ElementType).Returns(entities.ElementType);
        mockSet.As<IQueryable<TEntity>>().Setup(m => m.GetEnumerator()).Returns(() => entities.GetEnumerator());

        return mockSet;
    }
}

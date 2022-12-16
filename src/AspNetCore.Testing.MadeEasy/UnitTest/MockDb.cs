using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AspNetCore.Testing.MadeEasy.UnitTest;

/// <summary>
/// Create mock db set
/// </summary>
public class MockDb
{
    /// <summary>
    /// Generate <see cref="Mock"/> of <see cref="DbSet{TEntity}"/>
    /// </summary>
    /// <typeparam name="TEntity">type of entity</typeparam>
    /// <param name="data">data</param>
    /// <returns></returns>
    public static Mock<DbSet<TEntity>> CreateDbSet<TEntity>(ICollection<TEntity> data) where TEntity : class
    {
        var query = data.AsQueryable();

        var dbSetMock = new Mock<DbSet<TEntity>>();

        var mockSet = new Mock<DbSet<TEntity>>();
        mockSet.As<IAsyncEnumerable<TEntity>>()
            .Setup(m => m.GetAsyncEnumerator(CancellationToken.None))
            .Returns(new InMemoryDbAsyncEnumerator<TEntity>(query.GetEnumerator()));

        mockSet.As<IQueryable<TEntity>>()
           .Setup(m => m.Provider)
           .Returns(new InMemoryAsyncQueryProvider<TEntity>(query.Provider));

        mockSet.As<IQueryable<TEntity>>().Setup(m => m.Expression).Returns(query.Expression);
        mockSet.As<IQueryable<TEntity>>().Setup(m => m.ElementType).Returns(query.ElementType);
        mockSet.As<IQueryable<TEntity>>().Setup(m => m.GetEnumerator()).Returns(() => query.GetEnumerator());

        mockSet.Setup(x => x.RemoveRange(It.IsAny<IEnumerable<TEntity>>()))
            .Callback<IEnumerable<TEntity>>(entities =>
            {
                foreach (var entity in entities.ToList())
                {
                    data.Remove(entity);
                }
            });

        return mockSet;
    }
}

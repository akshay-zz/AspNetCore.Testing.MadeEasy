using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
    /// <param name="data">seed data</param>
    /// <param name="find">find action</param>
    /// <returns></returns>
    public static Mock<DbSet<TEntity>> CreateDbSet<TEntity>(ICollection<TEntity> data, Func<object[], TEntity> find = default)
        where TEntity : class
    {
        var query = data.AsQueryable();
        find ??= (o => null);

        var dbSetMock = new Mock<DbSet<TEntity>>();

        var mock = new Mock<DbSet<TEntity>>();
        mock.As<IAsyncEnumerable<TEntity>>()
            .Setup(m => m.GetAsyncEnumerator(CancellationToken.None))
            .Returns(new InMemoryDbAsyncEnumerator<TEntity>(query.GetEnumerator()));

        mock.As<IQueryable<TEntity>>()
           .Setup(m => m.Provider)
           .Returns(new InMemoryAsyncQueryProvider<TEntity>(query.Provider));

        mock.As<IQueryable<TEntity>>().Setup(m => m.Expression).Returns(query.Expression);
        mock.As<IQueryable<TEntity>>().Setup(m => m.ElementType).Returns(query.ElementType);
        mock.As<IQueryable<TEntity>>().Setup(m => m.GetEnumerator()).Returns(() => query.GetEnumerator());

        // We are aware its a internal Api. But as suggested in the issue 
        // https://github.com/dotnet/efcore/issues/27110#issuecomment-1009000699 for the time being we are mocking it.
        // Latter if will find any better approach will implement that.

        var internalEntityEntry = new InternalEntityEntry(
            new Mock<IStateManager>().Object,
            new RuntimeEntityType("T", typeof(TEntity), false, null, null, null, ChangeTrackingStrategy.Snapshot, null, false),
            data);

        var entityEntry = new Mock<EntityEntry<TEntity>>(internalEntityEntry);

        mock.Setup(m => m.Find(It.IsAny<object[]>())).Returns(find);

        mock.Setup(m => m.FindAsync(It.IsAny<object[]>()))
            .Returns<object[]>(objs => new ValueTask<TEntity>(find(objs)));

        mock.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns<object[], CancellationToken>((objs, tocken) => new ValueTask<TEntity>(find(objs)));

        // Add
        mock.Setup(m => m.Add(It.IsAny<TEntity>()))
            .Callback<TEntity>(entity => { data.Add(entity); })
            .Returns(() => entityEntry.Object);

        // AddAsync
        mock.Setup(d => d.AddAsync(It.IsAny<TEntity>(), It.IsAny<CancellationToken>()))
            .Callback<TEntity, CancellationToken>((entity, token) => { data.Add(entity); })
            .Returns(() => new ValueTask<EntityEntry<TEntity>>(entityEntry.Object));

        // Attach
        mock.Setup(m => m.Attach(It.IsAny<TEntity>()))
            .Callback<TEntity>(entity => { data.Add(entity); })
            .Returns(() => entityEntry.Object);


        //Remove
        mock.Setup(m => m.Remove(It.IsAny<TEntity>()))
            .Callback<TEntity>(entity => { data.Remove(entity); });

        // Update missing
        // public virtual EntityEntry<TEntity> Update(TEntity entity)

        // AddRange
        mock.Setup(m => m.AddRange(It.IsAny<IEnumerable<TEntity>>()))
            .Callback<IEnumerable<TEntity>>(entities =>
            {
                foreach (var entity in entities.ToList())
                {
                    data.Add(entity);
                }
            });


        // AddRangeAsync
        mock.Setup(m => m.AddRangeAsync(It.IsAny<TEntity[]>()))
            .Callback<TEntity[]>(entities =>
            {
                foreach (var entity in entities.ToList())
                {
                    data.Add(entity);
                }
            })
            .Returns(() => Task.CompletedTask);

        // AttachRange
        mock.Setup(m => m.AttachRange(It.IsAny<TEntity[]>()))
            .Callback<TEntity[]>(entities =>
            {
                foreach (var entity in entities.ToList())
                {
                    data.Add(entity);
                }
            });

        mock.Setup(x => x.RemoveRange(It.IsAny<TEntity[]>()))
            .Callback<TEntity[]>(entities =>
            {
                foreach (var entity in entities.ToList())
                {
                    data.Remove(entity);
                }
            });

        // UpdateRange missing
        // public virtual void UpdateRange(params TEntity[] entities)

        mock.Setup(m => m.AddRange(It.IsAny<TEntity[]>()))
            .Callback<TEntity[]>(entities =>
            {
                foreach (var entity in entities.ToList())
                {
                    data.Add(entity);
                }
            });

        mock.Setup(m => m.AddRangeAsync(It.IsAny<IEnumerable<TEntity>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<TEntity>, CancellationToken>((entities, token) =>
            {
                foreach (var entity in entities.ToList())
                {
                    data.Add(entity);
                }
            })
            .Returns(Task.CompletedTask);

        // AttachRange
        mock.Setup(m => m.AttachRange(It.IsAny<IEnumerable<TEntity>>()))
            .Callback<IEnumerable<TEntity>>(entities =>
            {
                foreach (var entity in entities.ToList())
                {
                    data.Add(entity);
                }
            });

        // RemoveRange
        mock.Setup(x => x.RemoveRange(It.IsAny<IEnumerable<TEntity>>()))
            .Callback<IEnumerable<TEntity>>(entities =>
            {
                foreach (var entity in entities.ToList())
                {
                    data.Remove(entity);
                }
            });

        return mock;
    }
}

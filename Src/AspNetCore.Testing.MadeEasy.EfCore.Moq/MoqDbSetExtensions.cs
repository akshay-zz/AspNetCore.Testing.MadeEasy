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

namespace AspNetCore.Testing.MadeEasy.EfCore.Moq;

public static class MoqDbSetExtensions
{

    /// <summary>
    /// Generate <see cref="Mock"/> of <see cref="DbSet{TEntity}"/>
    /// </summary>
    /// <typeparam name="TEntity">type of entity</typeparam>
    /// <param name="data">seed data</param>
    /// <param name="find">find action</param>
    /// <param name="entityEntry"> Entity entry</param>
    /// <returns></returns>
    public static Mock<DbSet<TEntity>> CreateDbSet<TEntity>(
        this ICollection<TEntity> data, Func<object[], TEntity>? find = default,
        EntityEntry<TEntity>? entityEntry = default)
        where TEntity : class
    {
        var query = data.AsQueryable();
        find ??= (o => null);

        // We are aware its a internal Api. But as suggested in the issue 
        // https://github.com/dotnet/efcore/issues/27110#issuecomment-1009000699 for the time being we are mocking it.
        // Latter if will find any better approach will implement that.

#pragma warning disable EF1001 // Internal EF Core API usage.
        var internalEntityEntry = new InternalEntityEntry(
            new Mock<IStateManager>().Object,
            new RuntimeEntityType("T", typeof(TEntity), false, null, null, null, ChangeTrackingStrategy.Snapshot, null, false),
            data);
#pragma warning restore EF1001 // Internal EF Core API usage.

        entityEntry ??= new Mock<EntityEntry<TEntity>>(internalEntityEntry).Object;

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

        //Find
        mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns(find);

        mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
            .Returns<object[]>(objs => new ValueTask<TEntity?>(find(objs)));

        mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns<object[], CancellationToken>((objs, tocken) => new ValueTask<TEntity?>(find(objs)));

        // Add
        mockSet.Setup(m => m.Add(It.IsAny<TEntity>()))
            .Callback<TEntity>(entity => { data.Add(entity); })
            .Returns(() => entityEntry);

        // AddAsync
        mockSet.Setup(d => d.AddAsync(It.IsAny<TEntity>(), It.IsAny<CancellationToken>()))
            .Callback<TEntity, CancellationToken>((entity, token) => { data.Add(entity); })
            .Returns(() => new ValueTask<EntityEntry<TEntity>>(entityEntry));

        // Attach
        mockSet.Setup(m => m.Attach(It.IsAny<TEntity>()))
            .Callback<TEntity>(entity => { data.Add(entity); })
            .Returns(() => entityEntry);


        //Remove
        mockSet.Setup(m => m.Remove(It.IsAny<TEntity>()))
            .Callback<TEntity>(entity => { data.Remove(entity); });

        // Update missing
        // public virtual EntityEntry<TEntity> Update(TEntity entity)

        // AddRange
        mockSet.Setup(m => m.AddRange(It.IsAny<IEnumerable<TEntity>>()))
            .Callback<IEnumerable<TEntity>>(entities =>
            {
                foreach (var entity in entities.ToList())
                {
                    data.Add(entity);
                }
            });


        // AddRangeAsync
        mockSet.Setup(m => m.AddRangeAsync(It.IsAny<TEntity[]>()))
            .Callback<TEntity[]>(entities =>
            {
                foreach (var entity in entities.ToList())
                {
                    data.Add(entity);
                }
            })
            .Returns(() => Task.CompletedTask);

        // AttachRange
        mockSet.Setup(m => m.AttachRange(It.IsAny<TEntity[]>()))
            .Callback<TEntity[]>(entities =>
            {
                foreach (var entity in entities.ToList())
                {
                    data.Add(entity);
                }
            });

        mockSet.Setup(x => x.RemoveRange(It.IsAny<TEntity[]>()))
            .Callback<TEntity[]>(entities =>
            {
                foreach (var entity in entities.ToList())
                {
                    data.Remove(entity);
                }
            });

        // UpdateRange missing
        // public virtual void UpdateRange(params TEntity[] entities)

        mockSet.Setup(m => m.AddRange(It.IsAny<TEntity[]>()))
            .Callback<TEntity[]>(entities =>
            {
                foreach (var entity in entities.ToList())
                {
                    data.Add(entity);
                }
            });

        mockSet.Setup(m => m.AddRangeAsync(It.IsAny<IEnumerable<TEntity>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<TEntity>, CancellationToken>((entities, token) =>
            {
                foreach (var entity in entities.ToList())
                {
                    data.Add(entity);
                }
            })
            .Returns(Task.CompletedTask);

        // AttachRange
        mockSet.Setup(m => m.AttachRange(It.IsAny<IEnumerable<TEntity>>()))
            .Callback<IEnumerable<TEntity>>(entities =>
            {
                foreach (var entity in entities.ToList())
                {
                    data.Add(entity);
                }
            });

        // RemoveRange
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

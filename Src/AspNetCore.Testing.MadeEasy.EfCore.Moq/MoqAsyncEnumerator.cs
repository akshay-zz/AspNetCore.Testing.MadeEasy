using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace AspNetCore.Testing.MadeEasy.EfCore.Moq;

internal class InMemoryAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider innerQueryProvider;

    public InMemoryAsyncQueryProvider(IQueryProvider innerQueryProvider)
    {
        this.innerQueryProvider = innerQueryProvider;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new InMemoryAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new InMemoryAsyncEnumerable<TElement>(expression);
    }

    public object Execute(Expression expression)
    {
        return this.innerQueryProvider.Execute(expression)!;
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return this.innerQueryProvider.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = new CancellationToken())
    {
        var result = Execute(expression);

        var expectedResultType = typeof(TResult).GetGenericArguments()?.FirstOrDefault();
        if (expectedResultType == null)
        {
            return default(TResult);
        }

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
            ?.MakeGenericMethod(expectedResultType)!
            .Invoke(null, new[] { result })!;
    }

    public Task<object> ExecuteAsync(Expression expression, CancellationToken cancellationToken)
    {
        return Task.FromResult(this.Execute(expression));
    }
}

internal class InMemoryAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public InMemoryAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    {
    }

    public InMemoryAsyncEnumerable(Expression expression)
        : base(expression)
    {
    }

    IQueryProvider IQueryable.Provider => new InMemoryAsyncQueryProvider<T>(this);

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
    {
        return new InMemoryDbAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    public IAsyncEnumerator<T> GetEnumerator()
    {
        return this.GetAsyncEnumerator();
    }
}

internal class InMemoryDbAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> innerEnumerator;
    private bool disposed = false;

    public InMemoryDbAsyncEnumerator(IEnumerator<T> enumerator)
    {
        this.innerEnumerator = enumerator;
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return new ValueTask();
    }

    public Task<bool> MoveNext(CancellationToken cancellationToken)
    {
        return Task.FromResult(this.innerEnumerator.MoveNext());
    }

    public ValueTask<bool> MoveNextAsync()
    {
        return new ValueTask<bool>(Task.FromResult(this.innerEnumerator.MoveNext()));
    }

    public T Current => this.innerEnumerator.Current;

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposed)
        {
            if (disposing)
            {
                // Dispose managed resources.
                this.innerEnumerator.Dispose();
            }

            this.disposed = true;
        }
    }
}


using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Testing.MadeEasy.Extensions;

public static class DatabaseRelatedExtensions
{
    public static void Clear<T>(this DbSet<T> dbSet) where T : class
    {
        dbSet.RemoveRange(dbSet);
    }
}


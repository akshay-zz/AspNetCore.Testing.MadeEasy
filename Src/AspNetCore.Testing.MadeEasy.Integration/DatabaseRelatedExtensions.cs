using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Testing.MadeEasy.Integration;

/// <summary>
/// Extensions related to database operations
/// </summary>
public static class DatabaseRelatedExtensions
{
    /// <summary>
    /// Clears a entity data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dbSet"></param>
    public static void Clear<T>(this DbSet<T> dbSet) where T : class
    {
        dbSet.RemoveRange(dbSet);
    }
}


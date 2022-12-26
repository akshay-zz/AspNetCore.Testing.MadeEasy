﻿using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Testing.MadeEasy.Extensions;

/// <summary>
/// Extensions related to database operations
/// </summary>
public static class DatabaseRelatedExtensions
{
    /// <summary>
    /// Clears a entity data
    /// </summary>
    /// <typeparam name="T">Entity</typeparam>
    /// <param name="dbSet"></param>
    public static void Clear<T>(this DbSet<T> dbSet) where T : class
    {
        dbSet.RemoveRange(dbSet);
    }
}


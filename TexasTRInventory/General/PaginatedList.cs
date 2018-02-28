﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TexasTRInventory
{
    //https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/sort-filter-page
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            this.AddRange(items);
        }

        public bool HasPreviousPage
        {
            get
            {
                return (PageIndex > 1);
            }
        }

        public bool HasNextPage
        {
            get
            {
                return (PageIndex < TotalPages);
            }
        }

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<Task<T>> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            var tasks = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            var items = new List<T>();
            foreach(Task<T> task in tasks)
            {
                items.Add(await task);
            }
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
}
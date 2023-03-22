using FileShare.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FileShare.Repository
{
    /// <summary>
    /// Base para um DbSet
    /// </summary>
    /// <typeparam name="TEntity">TEntity</typeparam>
    /// <typeparam name="TKey">TKey</typeparam>
    public class Repository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class
    {
        /// <summary>
        /// DbContext
        /// </summary>
        protected ApplicationDbContext Context { get; }

        /// <summary>
        /// DbSet of the Table
        /// </summary>
        public DbSet<TEntity> Table { get; private set; }

        /// <summary>
        /// Primary Key Name
        /// </summary>
        public string PrimaryKeyName { get; }

        /// <summary>
        /// IEntityType
        /// </summary>
        public IEntityType EntityType { get; private set; }

        /// <summary>
        /// Properties
        /// </summary>
        public IEnumerable<IProperty> Properties { get; private set; }

        /// <summary>
        /// IModel
        /// </summary>
        public IModel Model { get; private set; }

        /// <summary>
        /// Construtor com o RepositoryDbContext
        /// </summary>
        /// <param name="context"></param>
        public Repository(ApplicationDbContext context)
        {
            Context = context;
            Table = Context.Set<TEntity>();

            Model = Context.Model;
            EntityType = Model.FindEntityType(typeof(TEntity));
            Properties = EntityType.GetProperties();
            PrimaryKeyName = EntityType.FindPrimaryKey().Properties.First().Name;
        }

        private Repository()
        {
        }

        /// <summary>
        /// Add
        /// </summary>
        public virtual async Task<int> AddAsync(TEntity entity)
        {
            await Table.AddAsync(entity);
            return await Context.SaveChangesAsync();
        }

        /// <summary>
        /// add
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task<int> AddRangeAsync(IEnumerable<TEntity> entity)
        {
            await Table.AddRangeAsync(entity);
            return await Context.SaveChangesAsync();
        }

        /// <summary>
        /// Get
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual Task<TEntity> GetByIdAsync(TKey id) => Table.FirstOrDefaultAsync(e => id.Equals((TKey)e.GetType().GetProperty(PrimaryKeyName).GetValue(e)));

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public virtual async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter = null) => (filter != null ? await Table.FirstOrDefaultAsync(filter) : await Table.FirstOrDefaultAsync());

        /// <summary>
        /// delete
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual Task<int> DeleteAsync(TEntity entity)
        {
            if (Context.Entry(entity).State == EntityState.Detached)
            {
                Table.Attach(entity);
            }

            Table.Remove(entity);
            return Context.SaveChangesAsync();
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<int> DeleteByIdAsync(TKey id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Id: {id} not found");
            Table.Remove(entity);
            return await Context.SaveChangesAsync();
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public virtual async Task<int> DeleteRangeAsync(Expression<Func<TEntity, bool>> filter)
        {
            Table.RemoveRange(Table.Where(filter));
            return await Context.SaveChangesAsync();
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task<int> DeleteRangeAsync(IEnumerable<TEntity> entity)
        {
            Table.RemoveRange(entity);
            return await Context.SaveChangesAsync();
        }

        /// <summary>
        /// Udapte
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task<int> UpdateAsync(TEntity entity)
        {
            var entry = Context.Entry(entity);
            var pkey = entity.GetType().GetProperty(PrimaryKeyName)?.GetValue(entity);

            if (entry.State == EntityState.Detached)
            {
                var set = Context.Set<TEntity>();
                TEntity attachedEntity = await set.FindAsync(pkey); // access the key
                if (attachedEntity != null)
                {
                    var attachedEntry = Context.Entry(attachedEntity);
                    attachedEntry.CurrentValues.SetValues(entity);
                }
                else
                {
                    entry.State = EntityState.Modified; // attach the entity
                }
            }
            else
            {
                entry.State = EntityState.Modified;
            }

            return await Context.SaveChangesAsync();
        }

        /// <summary>
        /// Update Range Async
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task<int> UpdateRangeAsync(IEnumerable<TEntity> entity)
        {
            Parallel.ForEach(entity, async (ent) =>
            {
                var entry = Context.Entry(ent);
                var pkey = ent.GetType().GetProperty(PrimaryKeyName)?.GetValue(ent);

                if (entry.State == EntityState.Detached)
                {
                    var set = Context.Set<TEntity>();
                    TEntity attachedEntity = await set.FindAsync(pkey); // access the key
                    if (attachedEntity != null)
                    {
                        var attachedEntry = Context.Entry(attachedEntity);
                        attachedEntry.CurrentValues.SetValues(ent);
                    }
                    else
                    {
                        entry.State = EntityState.Modified; // attach the entity
                    }
                }
                else
                {
                    entry.State = EntityState.Modified;
                }
            });

            return await Context.SaveChangesAsync();
        }

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<int> UpdateByIdAsync(TEntity entity, TKey id)
        {
            TEntity attachedEntity = await GetByIdAsync(id) ?? entity;

            if (entity == null)
                throw new KeyNotFoundException($"Id: {id} not found");

            var attachedEntry = Context.Entry(attachedEntity);
            attachedEntry.CurrentValues.SetValues(entity);
            attachedEntry.State = EntityState.Modified;

            return await Context.SaveChangesAsync();
        }

        /// <summary>
        /// get
        /// </summary>
        /// <returns></returns>
        public virtual Task<List<TEntity>> GetAsync() => Table.ToListAsync();

        /// <summary>
        /// GetAsync
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        public virtual Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> filter,
             Expression<Func<TEntity, object>> orderBy = null) => orderBy != null
            ? Table.OrderBy(orderBy).Where(filter).ToListAsync()
            : Table.Where(filter).ToListAsync();

        /// <summary>
        /// GetPagination
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="page"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task<KeyValuePair<int, List<TEntity>>> GetPagination(Expression<Func<TEntity, bool>> filter,
            int page = 1,
            int count = 10)
        {
            KeyValuePair<int, List<TEntity>> keys = new KeyValuePair<int, List<TEntity>>(Table.Count(), await Table
                                                                                                        .Where(filter)
                                                                                                        .Skip((page - 1) * count)
                                                                                                        .Take(count)
                                                                                                        .ToListAsync());

            return keys;
        }

        /// <summary>
        /// GetPagination
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="orderByDescending"></param>
        /// <param name="page"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task<KeyValuePair<int, List<TEntity>>> GetPagination(Expression<Func<TEntity, bool>> filter,
            Expression<Func<TEntity, object>> orderBy,
            Expression<Func<TEntity, object>> orderByDescending,
            int page = 1,
            int count = 10)
        {
            List<TEntity> itens;
            if (orderBy != null)
            {
                itens = await Table
                          .Where(filter)
                          .OrderBy(orderBy)
                          .Skip((page - 1) * count)
                          .Take(count)
                          .ToListAsync();
            }
            else if (orderByDescending != null)
            {
                itens = await Table
                          .Where(filter)
                          .OrderByDescending(orderByDescending)
                          .Skip((page - 1) * count)
                          .Take(count)
                          .ToListAsync();
            }
            else
            {
                itens = await Table
                         .Where(filter)
                         .Skip((page - 1) * count)
                         .Take(count)
                         .ToListAsync();
            }

            KeyValuePair<int, List<TEntity>> keys = new KeyValuePair<int, List<TEntity>>(Table.Count(), itens);
            return keys;
        }
    }
}
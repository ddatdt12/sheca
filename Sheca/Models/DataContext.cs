﻿using Sheca.Extensions;
using Microsoft.EntityFrameworkCore;
using Sheca.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Sheca.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
            this.ChangeTracker.LazyLoadingEnabled = false;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(new List<User>
            {
                new User{ Id = new Guid("d0ee9b2a-71cd-4d32-a778-0461ca0f64ff"), Email = "test@gmail.com", Password = "123123123"},
                new User{ Id = new Guid("077f0ae7-b699-40a3-b22e-1f065705b8e3"), Email = "test2@gmail.com", Password = "123123123"},
            });

            //ChangeToUtcDate(modelBuilder);
        }
        public DbSet<User> Users => Set<User>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Course> Courses => Set<Course>();

        public override int SaveChanges()
        {
            AddTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            AddTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }
        public void ChangeToUtcDate(ModelBuilder builder)
        {
            var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
              v => v.ToUniversalTime(),
              v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? v.Value.ToUniversalTime() : v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (entityType.IsKeyless)
                {
                    continue;
                }
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(dateTimeConverter);
                    }
                    else if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(nullableDateTimeConverter);
                    }
                }
            }
        }
        private void AddTimestamps()
        {
            var entities = ChangeTracker.Entries()
                .Where(x => x.Entity is AuditEntity && (x.State == EntityState.Added || x.State == EntityState.Modified));

            foreach (var entity in entities)
            {
                var now = DateTime.UtcNow; // current datetime

                if (entity.State == EntityState.Added)
                {
                    ((AuditEntity)entity.Entity).CreatedAt = now;
                }
                ((AuditEntity)entity.Entity).UpdatedAt = now;
            }
        }
    }
}

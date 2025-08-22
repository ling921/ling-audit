using Microsoft.EntityFrameworkCore;

namespace Ling.Audit.EntityFrameworkCore.Tests.Interceptors
{
    public class AuditSaveChangesInterceptorTests
    {
        [Fact]
        public void DisableAuditingSwitch_WhenTrue_SkipsAuditing()
        {
            // Arrange
            AppContext.SetSwitch(AuditDefaults.DisableAuditingSwitch, true);
            var interceptor = new AuditSaveChangesInterceptor();
            var context = CreateDbContextWithEntities();

            // Act
            interceptor.SaveChangesBefore(context);

            // Assert 验证没有进行任何审计字段的修改
            var entry = context.ChangeTracker.Entries().First();
            Assert.Null(entry.Property("CreatedAt").CurrentValue);
            Assert.Null(entry.Property("CreatedBy").CurrentValue);

            // Cleanup
            AppContext.SetSwitch(AuditDefaults.DisableAuditingSwitch, false);
        }

        [Fact]
        public void SaveChangesBefore_ForNewEntity_SetsCreationAuditFields()
        {
            // Arrange
            var interceptor = new AuditSaveChangesInterceptor();
            var context = CreateDbContextWithEntities();
            var entity = new TestAuditEntity();
            context.Add(entity);

            // Act
            interceptor.SaveChangesBefore(context);

            // Assert
            var entry = context.ChangeTracker.Entries().First();
            Assert.NotNull(entry.Property("CreatedAt").CurrentValue);
            Assert.Equal("system", entry.Property("CreatedBy").CurrentValue);
        }

        [Fact]
        public void SaveChangesBefore_ForModifiedEntity_SetsModificationAuditFields()
        {
            // Arrange
            var interceptor = new AuditSaveChangesInterceptor();
            var context = CreateDbContextWithEntities();
            var entity = new TestAuditEntity { Id = 1 };
            context.Attach(entity);
            context.Entry(entity).State = EntityState.Modified;

            // Act
            interceptor.SaveChangesBefore(context);

            // Assert
            var entry = context.ChangeTracker.Entries().First();
            Assert.NotNull(entry.Property("LastModifiedAt").CurrentValue);
            Assert.Equal("system", entry.Property("LastModifiedBy").CurrentValue);
        }

        [Fact]
        public void SaveChangesBefore_ForSoftDeletedEntity_SetsDeletionAuditFields()
        {
            // Arrange
            var interceptor = new AuditSaveChangesInterceptor();
            var context = CreateDbContextWithEntities();
            var entity = new TestAuditEntity { Id = 1 };
            context.Attach(entity);
            entity.IsDeleted = true;

            // Act
            interceptor.SaveChangesBefore(context);

            // Assert
            var entry = context.ChangeTracker.Entries().First();
            Assert.NotNull(entry.Property("DeletedAt").CurrentValue);
            Assert.Equal("system", entry.Property("DeletedBy").CurrentValue);
        }

        private DbContext CreateDbContextWithEntities()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new TestDbContext(options);
        }
    }
}

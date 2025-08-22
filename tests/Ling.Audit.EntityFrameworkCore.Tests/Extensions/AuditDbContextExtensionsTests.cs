using Microsoft.EntityFrameworkCore;

namespace Ling.Audit.EntityFrameworkCore.Tests.Extensions
{
    public class AuditDbContextExtensionsTests
    {
        [Fact]
        public void AddAudit_AddsInterceptor()
        {
            // Arrange
            var builder = new DbContextOptionsBuilder<TestDbContext>();

            // Act
            builder.AddAudit();

            // Assert
            var options = builder.Options;
            var interceptors = options.FindExtension<CoreOptionsExtension>()?.Interceptors;
            Assert.Contains(interceptors, i => i is AuditSaveChangesInterceptor);
        }

        [Fact]
        public void UseAudit_ConfiguresAuditOptions()
        {
            // Arrange
            var builder = new DbContextOptionsBuilder<TestDbContext>();

            // Act
            builder.UseAudit(options =>
            {
                options.CurrentUserProvider = () => "test-user";
                options.CurrentTimeProvider = () => DateTimeOffset.Parse("2023-01-01");
            });

            // Assert
            var auditOptions = builder.Options.GetExtension<AuditOptionsExtension>();
            Assert.Equal("test-user", auditOptions.CurrentUserProvider());
            Assert.Equal(DateTimeOffset.Parse("2023-01-01"), auditOptions.CurrentTimeProvider());
        }
    }
}

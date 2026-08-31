namespace Ling.Audit.EntityFrameworkCore.Tests;

public class AuditEntityChangeLogTests
{
    [Fact]
    public void AuditEntityChangeLog_ShouldInitializeWithDefaultValues()
    {
        // Act
        var log = new AuditEntityChangeLog<Guid>();

        // Assert
        log.Details.Should().NotBeNull();
        log.Details.Should().BeEmpty();
        log.EventTime.Should().Be(default);
        log.EventType.Should().Be(AuditEventType.None);
    }

    [Fact]
    public void AuditEntityChangeLog_ShouldTrackFieldChanges()
    {
        // Arrange
        var log = new AuditEntityChangeLog<Guid>();
        var fieldChange = new AuditFieldChangeLog
        {
            FieldName = "Name",
            ValueType = "string",
            OriginalValue = "OldName",
            NewValue = "NewName"
        };

        // Act
        log.Details.Add(fieldChange);

        // Assert
        log.Details.Should().ContainSingle();
        log.Details.First().Should().BeEquivalentTo(fieldChange);
    }

    [Fact]
    public void AuditEntityChangeLog_ShouldHandleMultipleFieldChanges()
    {
        // Arrange
        var log = new AuditEntityChangeLog<Guid>();
        var changes = new[]
        {
            new AuditFieldChangeLog { FieldName = "Name", ValueType = "string", OriginalValue = "Old", NewValue = "New" },
            new AuditFieldChangeLog { FieldName = "Age", ValueType = "int", OriginalValue = "20", NewValue = "21" }
        };

        // Act
        foreach (var change in changes)
        {
            log.Details.Add(change);
        }

        // Assert
        log.Details.Should().HaveCount(2);
        log.Details.Should().BeEquivalentTo(changes);
    }
}

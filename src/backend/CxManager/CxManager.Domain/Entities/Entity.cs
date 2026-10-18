namespace CxManager.Domain.Entities;

public abstract class Entity(
    long id,
    DateTimeOffset createdAt,
    DateTimeOffset? deletedAt)
{
    public long Id { get; protected set; } = id;
    public DateTimeOffset CreatedAt { get; protected set; } = createdAt;
    public DateTimeOffset? DeletedAt { get; protected set; } = deletedAt;

    public bool IsDeleted
        => DeletedAt.HasValue;

    public virtual void SoftDelete()
        => DeletedAt = DateTimeOffset.UtcNow;
}

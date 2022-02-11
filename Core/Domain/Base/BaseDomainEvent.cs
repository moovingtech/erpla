using System;

namespace Core.Domain.Base
{
    public abstract class BaseDomainEvent
    {
        public BaseDomainEvent()
        {
            EventId = Guid.NewGuid();
            CreatedOn = DateTime.UtcNow;
        }

        public virtual Guid EventId { get; init; }
        public virtual DateTime CreatedOn { get; init; }
    }
}
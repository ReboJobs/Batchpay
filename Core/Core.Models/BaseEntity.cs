using System;

namespace Core
{
    public abstract class BaseEntity
    {
        public long Id { get; set; }
        public DateTime DateCreated { get; set; }
    }
}

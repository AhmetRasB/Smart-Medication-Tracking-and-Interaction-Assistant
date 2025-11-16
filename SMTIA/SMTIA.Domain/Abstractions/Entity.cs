namespace SMTIA.Domain.Abstractions
{
    public abstract class Entity
    {
        public Guid Id { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        
        protected Entity()
        {
            Id = Guid.NewGuid();
        }
    }
}

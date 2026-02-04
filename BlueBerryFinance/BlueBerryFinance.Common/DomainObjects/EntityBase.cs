namespace BlueBerryFinance.Commom.Entities
{
    public abstract class EntityBase : Entity
    {
        public DateTime InsertionDate { get; protected set; }
        public DateTime LastModification { get; protected set; }
        public bool IsDeleted { get; private set; }
        public DateTime? DeletionDate { get; private set; }

        public void SetInsertionDate(DateTime insertionDate) =>
            InsertionDate = insertionDate;

        public void SetLastModification(DateTime lastModification) =>
            LastModification = lastModification;

        public virtual void Delete()
        {
            IsDeleted = true;
            DeletionDate = DateTime.Now;
        }
    }
}

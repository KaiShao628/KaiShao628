namespace DatabaseCommon.EntityBase
{
    public abstract class Entity
    {
        /// <summary>
        /// The unique ID for the entity
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The create time for the entity.
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// The update time for the entity.
        /// </summary>
        public DateTime UpdateTime { get; set; }

        public string CreatorId { get; set; }

        public string UpdaterId { get; set; }


        /// <summary>
        /// Gets or sets if this entity is deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        public Entity(string id = "")
        {
            Id = string.IsNullOrEmpty(id) ? Guid.NewGuid().ToString() : id;
        }
    }
}

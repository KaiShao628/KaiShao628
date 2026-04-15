using System.ComponentModel.DataAnnotations;

namespace FamilyLedgeManagement.Dtos.BaseDtos
{
    public class EntityBaseDto
    {
        [Key]
        public string Id { get; set; } = "";

        /// <summary>
        /// 创建用户Id
        /// </summary>
        [Display(Name = "创建者")]
        public string CreatorId { get; set; } = "";

        /// <summary>
        /// 更新用户Id
        /// </summary>
        [Display(Name = "更新者")]
        public string UpdaterId { get; set; } = "";

        /// <summary>
        /// The create time for the entity.
        /// </summary>
        [Display(Name = "创建时间")]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// The update time for the entity.
        /// </summary>

        /// <summary>
        /// The create time for the entity.
        /// </summary>
        [Display(Name = "更新时间")]
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// Gets or sets if this entiry is deleted.
        /// </summary>
        [Display(Name = "是否删除")]
        public bool IsDeleted { get; set; }
    }
}

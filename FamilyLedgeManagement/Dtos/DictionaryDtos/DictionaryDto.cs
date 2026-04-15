using FamilyLedgeManagement.Dtos.BaseDtos;
using System.ComponentModel.DataAnnotations;

namespace FamilyLedgeManagement.Dtos.DictionaryDtos
{
    public class DictionaryDto : EntityBaseDto
    {
        [Required(ErrorMessage = "{0}不可为空")]
        [Display(Name = "字典Code")]
        public string DictionaryCode { get; set; } = "";

        [Required(ErrorMessage = "{0}不可为空")]
        [Display(Name = "字典名称")]
        public string DictionaryName { get; set; } = "";

        public bool IsSystem { get; set; } = false;
    }

    public class DictionaryValueDto : EntityBaseDto
    {
        [Required(ErrorMessage = "{0}不可为空")]
        [Display(Name = "Code")]
        public string Code { get; set; } = "";

        [Required(ErrorMessage = "{0}不可为空")]
        [Display(Name = "值")]
        public string Value { get; set; } = "";

        [Required(ErrorMessage = "{0}不可为空")]
        [Display(Name = "中文值")]
        public string CValue { get; set; } = "";

        public bool IsUsed { get; set; } = true;
        public bool IsSystem { get; set; } = false;

        [Required(ErrorMessage = "{0}不可为空")]
        [Display(Name = "所属字典")]
        public string DictionaryId { get; set; } = "";
        public string DictionaryName { get; set; } = "";

    }
}

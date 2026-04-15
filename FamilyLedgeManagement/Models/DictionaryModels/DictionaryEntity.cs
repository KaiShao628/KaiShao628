using DatabaseCommon.EntityBase;

namespace FamilyLedgeManagement.Models.DictionaryModels
{
    public class DictionaryEntity : Entity
    {
        public string DictionaryCode { get; set; } = "";
        public string DictionaryName { get; set; } = "";

        public bool IsSystem { get; set; } = false;
    }

    public class DictionaryValueEntity : Entity
    {
        public string Code { get; set; } = "";
        public string Value { get; set; } = "";

        public string CValue { get; set; } = "";

        public bool IsUsed { get; set; } = true;
        public bool IsSystem { get; set; } = false;

        public string DictionaryId { get; set; } = "";
    }
}

using DatabaseCommon.MongoDB;

namespace FamilyLedgeManagement.Database
{
    public class FamilyLedgeMongoDB : MongoDbClientBase
    {
        public static FamilyLedgeMongoDB Instance { get; set; } = new FamilyLedgeMongoDB();

        protected override void RegisterEntities()
        {

        }
    }
}

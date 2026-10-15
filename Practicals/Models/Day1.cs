namespace Practicals.Models
{
    public class Day1
    {
        public static readonly List<Unit> Units = new()
        {
            new Unit(1, "1st Armoured Brigade", "Jodhpur"),
            new Unit(2, "5th Signal Regiment", "Pune")
        };

        public static readonly List<Personnel> PersonnelList = new()
        {
            new Personnel(1, 1, "Ravi Kumar", "Major"),
            new Personnel(2, 1, "Anil Singh", "Captain"),
            new Personnel(3, 2, "Sunil Rao", "Lieutenant")
        };

        public static readonly List<Equipment> EquipmentList = new()
        {
            new Equipment(1, 1, "Tank T-90", "Active"),
            new Equipment(2, 2, "Radio Set R-5", "Under Repair")
        };

    }

    #region HTTP & REST

    public record Unit(int Id, string Name, string Location);
    public record Personnel(int Id, int UnitId, string Name, string Rank);
    public record Equipment(int Id, int UnitId, string Name, string Status);

    #endregion
}

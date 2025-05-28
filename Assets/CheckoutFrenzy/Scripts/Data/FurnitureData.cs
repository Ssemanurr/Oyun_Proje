using System.Collections.Generic;

namespace CryingSnow.CheckoutFrenzy
{
    [System.Serializable]
    public class FurnitureData
    {
        public int FurnitureID { get; set; }
        public string Name { get; set; }
        public Location Location { get; set; }
        public Orientation Orientation { get; set; }
        public bool WasMoving { get; set; }
        public List<ShelfData> SavedShelves { get; set; }
        public List<RackData> SavedRacks { get; set; }

        public FurnitureData(Furniture furniture)
        {
            FurnitureID = furniture.FurnitureID;
            Name = furniture.Name;
            Location = new Location(furniture.transform.position);
            Orientation = new Orientation(furniture.transform.rotation);
            WasMoving = furniture.IsMoving;

            if (furniture is ShelvingUnit shelvingUnit)
            {
                SavedShelves = new List<ShelfData>();

                foreach (var shelf in shelvingUnit.Shelves)
                {
                    SavedShelves.Add(new ShelfData(shelf));
                }
            }
            else if (furniture is StorageRack storageRack)
            {
                SavedRacks = new List<RackData>();

                foreach (var rack in storageRack.Racks)
                {
                    SavedRacks.Add(new RackData(rack));
                }
            }
        }
    }
}

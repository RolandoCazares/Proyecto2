namespace proyecto.Models
{
    public class GeneralTableObject
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public GeneralTableObject(int ID, string Name)
        {
            this.ID = ID;
            this.Name = Name;
        }
    }
}

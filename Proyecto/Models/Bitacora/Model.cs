using System.ComponentModel.DataAnnotations.Schema;

namespace proyecto.Models
{
    public class Model
    {
        public string ModelNumber { get; set; }        
        public Family Family { get; set; }
    }
}

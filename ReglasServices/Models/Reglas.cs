using System;
using System.Collections.Generic;

namespace ReglasServices.Models
{
    public partial class Reglas
    {
        public int Id { get; set; }
        public string Propiedad { get; set; }
        public string Operador { get; set; }
        public string ValorComparacion { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace NetFlox.DAL
{
    public class Utilisateur
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string AdresseEmail { get; set; }
        public DateTime DateInscription { get; set; }

        public virtual ICollection<Favori> Favoris { get; set; } = new HashSet<Favori>();
    }
}

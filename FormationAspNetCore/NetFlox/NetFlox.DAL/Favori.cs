using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NetFlox.DAL
{
    public class Favori
    {
        public int UtilisateurId { get; set; }
        public int FilmId { get; set; }

        public virtual Utilisateur Utilisateur { get; set; }
        public virtual Film Film { get; set; }

        public DateTime DateAjout { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace NetFlox.DAL
{
    public class Film
    {
        public int Id { get; set; }
        public string Titre { get; set; }
        public string Description { get; set; }
        public int? Annee { get; set; }
        public string Pays { get; set; }


        public string UrlAffiche { get; set; }
        //public byte[] Affiche { get; set; }

        public virtual ICollection<RoleCelebriteFilm> RoleCelebriteFilms { get; set; } = new HashSet<RoleCelebriteFilm>();
        public virtual ICollection<Favori> Favoris { get; set; } = new HashSet<Favori>();
    }
}

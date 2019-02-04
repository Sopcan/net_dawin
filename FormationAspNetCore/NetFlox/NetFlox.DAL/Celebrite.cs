using System;
using System.Collections.Generic;
using System.Text;

namespace NetFlox.DAL
{
    public class Celebrite
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        //public string Prenom { get; set; }
        public DateTime? DateNaissance { get; set; }

        public string UrlPhoto { get; set; }
        //public byte[] Photo { get; set; }

        public virtual ICollection<RoleCelebriteFilm> RoleCelebriteFilms { get; set; } = new HashSet<RoleCelebriteFilm>();
    }
}

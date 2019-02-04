using System;
using System.Collections.Generic;
using System.Text;

namespace NetFlox.DAL
{
    public class RoleCelebriteFilm
    {
        public int Id { get; set; }

        public string NomPersonnage { get; set; }

        public int CelebriteId { get; set; }
        public int FilmId { get; set; }
        public int RoleId { get; set; }

        public virtual Celebrite Celebrite { get; set; }
        public virtual Film Film { get; set; }
        public virtual Role Role { get; set; }
    }
}

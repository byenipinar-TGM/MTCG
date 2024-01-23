using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace byenipinar_MTCG.GameClasses
{
    public class User
    {
        public int Losses { get; set; }
        public int Wins { get; set; }
        public int Elo { get; set; }
        public string Bio { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}

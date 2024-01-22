using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace byenipinar_MTCG.GameClasses
{
    public class PackageOfCards
    {
        public int PackageId { get; set; }
        public bool Bought { get; set; }
        public List<Card> Cards { get; set; }

        public PackageOfCards()
        {
            Cards = new List<Card>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astramus.Worker
{
    public class Joke
    {
        public Joke(string setup, string punchline) { 
            Setup = setup;
            Punchline = punchline;
        }

        public string Setup { get; set; }
        public string Punchline { get; set; }
    }
}

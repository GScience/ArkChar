using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arkchar
{
    public class CharData
    {
        public string FilePath { get; }
        public string Name { get; }

        public CharData(string name, string file)
        {
            FilePath = file;
            Name = name;
        }
    }
}

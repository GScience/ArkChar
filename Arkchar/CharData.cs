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
        public bool HasFightAnim { get; }

        public CharData(string name, string file, bool hasFightAnim)
        {
            FilePath = file;
            Name = name;
            HasFightAnim = hasFightAnim;
        }
    }
}

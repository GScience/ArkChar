using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arkchar
{
    public interface ICharUpdater
    {
        void Init(SpineWindow spineWindow);
        void OnMousePress(SpineWindow spineWindow);
        void Update(SpineWindow spineWindow);
    }
}

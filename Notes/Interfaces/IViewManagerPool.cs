using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Interfaces
{
    public interface IViewManagerPool : IEnumerable<IViewManager>
    {
        T GetInstance<T>();
    }
}

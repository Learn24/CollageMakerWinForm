using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adsophic.Common.Util.Operations.Undo
{
    /// <summary>
    /// Implemented by classes that host child objects that 
    /// add and remove undo operations
    /// </summary>
    public interface IUndoOperationsContainer<T> 
    {
        void AddOperation(UndoableOperation<T> operation); 
    }
}

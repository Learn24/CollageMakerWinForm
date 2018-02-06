using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adsophic.Common.Util.Operations.Undo
{
    /// <summary>
    /// Implementator of the undo command.
    /// Called by the IUndoOperationsContainer to process an undo command
    /// </summary>
    public interface IUndoCommandImplementor<T>
    {
        bool Undo(T operation); 
    }
}

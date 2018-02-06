using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adsophic.Common.Util.DataStructures;
using Adsophic.Common.Util.ComponentModel;

namespace Adsophic.Common.Util.Operations.Undo
{
    public class UndoOperationsImplementor<TClass> : IUndoOperationsContainer<TClass>
    {
        private ThreadSafeStack<UndoableOperation<TClass>> _undoStack
            = new ThreadSafeStack<UndoableOperation<TClass>>();

        public event EventHandler<GenericEventArgs<UndoableOperation<TClass>>> Added;

        public event EventHandler<GenericEventArgs<UndoableOperation<TClass>>> Undone;

        #region IUndoOperationsContainer<TClass> Members

        public void AddOperation(UndoableOperation<TClass> operation)
        {
            Added(this, new GenericEventArgs<UndoableOperation<TClass>>(operation));
            _undoStack.Push(operation); 
        }

        #endregion

        public void UndoLastOperation()
        {
            if (!_undoStack.IsEmpty)
            {
                UndoableOperation<TClass> undoableOperation = _undoStack.Pop();
                Undone(this, new GenericEventArgs<UndoableOperation<TClass>>(undoableOperation));
                undoableOperation.Implementor.Undo(undoableOperation.Command);
            }
        }
    }


}

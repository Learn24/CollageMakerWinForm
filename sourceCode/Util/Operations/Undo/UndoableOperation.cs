using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adsophic.Common.Util.Operations.Undo
{
    /// <summary>
    /// Class that contains an undoable operation
    /// </summary>
    public class UndoableOperation<T>
    {
        private T _command;
        private IUndoCommandImplementor<T> _commandImplementor;

        /// <summary>
        /// creates a new Undoable Operation
        /// </summary>
        /// <param name="command">Undoable command</param>
        public UndoableOperation(T command, IUndoCommandImplementor<T> commandImplementor)
        {
            _command = command;
            _commandImplementor = commandImplementor;
        }

        /// <summary>
        /// Returns the undoable command. 
        /// </summary>
        public T Command { get { return _command; } }

        /// <summary>
        /// returns the command implementor
        /// </summary>
        public IUndoCommandImplementor<T> Implementor { get { return _commandImplementor; } }
    }
}

// HS Live Dev Editor (c) 2025 Happenstance Games LLC - MIT License

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Happenstance.SE.DevEditor.Core
{
    public class EditorHistory
    {
        private readonly Stack<IEditorCommand> _undoStack = new Stack<IEditorCommand>();
        private readonly Stack<IEditorCommand> _redoStack = new Stack<IEditorCommand>();

        public int MaxHistorySize { get; set; } = 50;

        // Events for UI updates
        public event Action<bool> OnUndoAvailabilityChanged;
        public event Action<bool> OnRedoAvailabilityChanged;

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public int UndoCount => _undoStack.Count;
        public int RedoCount => _redoStack.Count;

        public void StoreChange(IEditorCommand command)
        {
            if (command == null) return;

            // Execute the command
            command.Execute();

            // Add to undo stack
            _undoStack.Push(command);

            // Clear redo stack (new action breaks redo chain)
            if (_redoStack.Count > 0)
            {
                _redoStack.Clear();
                OnRedoAvailabilityChanged?.Invoke(false);
            }

            // Trim history if needed
            TrimHistoryIfNeeded();

            // Notify availability change
            OnUndoAvailabilityChanged?.Invoke(true);
        }

        public bool UndoChange()
        {
            if (!CanUndo) return false;

            var command = _undoStack.Pop();
            command.Undo();

            // Move to redo stack
            _redoStack.Push(command);

            // Update availability
            OnUndoAvailabilityChanged?.Invoke(CanUndo);
            OnRedoAvailabilityChanged?.Invoke(true);

            return true;
        }

        public bool RedoChange()
        {
            if (!CanRedo) return false;

            var command = _redoStack.Pop();
            command.Execute();

            // Move back to undo stack
            _undoStack.Push(command);

            // Update availability
            OnRedoAvailabilityChanged?.Invoke(CanRedo);
            OnUndoAvailabilityChanged?.Invoke(true);

            return true;
        }

        public void ClearAll()
        {
            bool hadUndo = CanUndo;
            bool hadRedo = CanRedo;

            _undoStack.Clear();
            _redoStack.Clear();

            if (hadUndo) OnUndoAvailabilityChanged?.Invoke(false);
            if (hadRedo) OnRedoAvailabilityChanged?.Invoke(false);
        }

        public string GetUndoDescription()
        {
            return CanUndo ? _undoStack.Peek().Description : "";
        }

        public string GetRedoDescription()
        {
            return CanRedo ? _redoStack.Peek().Description : "";
        }

        private void TrimHistoryIfNeeded()
        {
            while (_undoStack.Count > MaxHistorySize)
            {
                // Convert to array to remove from bottom
                var commands = new IEditorCommand[_undoStack.Count];
                _undoStack.CopyTo(commands, 0);

                _undoStack.Clear();

                // Add back all but the oldest
                for (int i = commands.Length - 1; i > 0; i--)
                {
                    _undoStack.Push(commands[i]);
                }
            }
        }

        // Debug helper
        public void PrintHistory()
        {
            Console.WriteLine($"=== Editor History (Max: {MaxHistorySize}) ===");
            Console.WriteLine($"Undo Stack ({_undoStack.Count}):");

            var undoArray = _undoStack.ToArray();
            for (int i = 0; i < undoArray.Length; i++)
            {
                Console.WriteLine($"  {i}: {undoArray[i].Description} -> {undoArray[i].TargetEntity?.Name}");
            }

            Console.WriteLine($"Redo Stack ({_redoStack.Count}):");
            var redoArray = _redoStack.ToArray();
            for (int i = 0; i < redoArray.Length; i++)
            {
                Console.WriteLine($"  {i}: {redoArray[i].Description} -> {redoArray[i].TargetEntity?.Name}");
            }
        }
    }
}

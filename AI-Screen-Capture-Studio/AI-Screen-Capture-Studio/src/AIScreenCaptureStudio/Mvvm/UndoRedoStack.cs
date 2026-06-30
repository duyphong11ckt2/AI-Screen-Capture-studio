namespace AIScreenCaptureStudio.Mvvm;

/// <summary>A single reversible edit expressed as undo/redo delegates.</summary>
public sealed class UndoableAction
{
    public required Action Do { get; init; }
    public required Action Undo { get; init; }
    public string Label { get; init; } = string.Empty;
}

/// <summary>Lightweight linear undo/redo history.</summary>
public sealed class UndoRedoStack
{
    private readonly Stack<UndoableAction> _undo = new();
    private readonly Stack<UndoableAction> _redo = new();

    public bool CanUndo => _undo.Count > 0;
    public bool CanRedo => _redo.Count > 0;

    /// <summary>Execute an action and record it for undo.</summary>
    public void Push(UndoableAction action)
    {
        action.Do();
        _undo.Push(action);
        _redo.Clear();
    }

    /// <summary>Record an already-applied action without re-executing it.</summary>
    public void Record(UndoableAction action)
    {
        _undo.Push(action);
        _redo.Clear();
    }

    public void Undo()
    {
        if (!CanUndo) return;
        var a = _undo.Pop();
        a.Undo();
        _redo.Push(a);
    }

    public void Redo()
    {
        if (!CanRedo) return;
        var a = _redo.Pop();
        a.Do();
        _undo.Push(a);
    }

    public void Clear() { _undo.Clear(); _redo.Clear(); }
}

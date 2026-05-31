namespace PhysX.Models;

public sealed class LessonPage
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Subtitle { get; init; }
    public required string Body { get; init; }
    public required string FormulaTitle { get; init; }
    public required IReadOnlyList<FormulaItem> Formulas { get; init; }
    public required string KeyPoint { get; init; }
    public required string ExampleTitle { get; init; }
    public required string Example { get; init; }
    public required string VisualKind { get; init; }
}

public sealed class FormulaItem
{
    public string Kind { get; init; } = "text";
    public string Equation { get; init; } = string.Empty;
    public IReadOnlyList<FormulaPart> Parts { get; init; } = Array.Empty<FormulaPart>();
    public string Left { get; init; } = string.Empty;
    public IReadOnlyList<FormulaPart> LeftParts { get; init; } = Array.Empty<FormulaPart>();
    public string Numerator { get; init; } = string.Empty;
    public IReadOnlyList<FormulaPart> NumeratorParts { get; init; } = Array.Empty<FormulaPart>();
    public string Denominator { get; init; } = string.Empty;
    public IReadOnlyList<FormulaPart> DenominatorParts { get; init; } = Array.Empty<FormulaPart>();
    public string Right { get; init; } = string.Empty;
    public string Explanation { get; init; } = string.Empty;
}

public sealed class FormulaPart
{
    public required string Text { get; init; }
    public bool IsVector { get; init; }
}

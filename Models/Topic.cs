using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace PhysX.Models;

public sealed class Topic
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Kicker { get; init; }
    public required string Description { get; init; }
    public required string Status { get; init; }
    public required IReadOnlyList<string> Modules { get; init; }
    public required Rect TileViewbox { get; init; }
    public required Brush AccentBrush { get; init; }
    public required Brush ChipBrush { get; init; }
    public required Brush ChipTextBrush { get; init; }
}

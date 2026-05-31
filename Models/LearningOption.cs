using System.Windows.Media;
using System.Windows;

namespace PhysX.Models;

public sealed class LearningOption
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Status { get; init; }
    public required bool IsAvailable { get; init; }
    public required Rect TileViewbox { get; init; }
    public required Brush AccentBrush { get; init; }
    public required Brush ChipBrush { get; init; }
    public required Brush ChipTextBrush { get; init; }
}

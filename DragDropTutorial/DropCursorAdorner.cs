using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace DragDropTutorial;

public class DropCursorAdorner : Adorner
{
    private double _yPos;
    public double YPos
    {
        get { return _yPos; }
        set
        {
            _yPos = value;
            InvalidateVisual();
        }
    }

    // Ctor
    public DropCursorAdorner(UIElement adornedElement) : base(adornedElement)
    {
        IsHitTestVisible = false; // So that drops can happen when mouse over adorner
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        // draw the cursor line
        Pen p = new Pen(Brushes.SteelBlue, 1);
        drawingContext.DrawLine(p, new Point(0, YPos), new Point(DesiredSize.Width, YPos));

        // draw the cursor left triangle
        Point lstart = new Point(5, YPos);
        LineSegment[] lsegments = new[]
        {
           new LineSegment(new Point(0, YPos - 5), true),
           new LineSegment(new Point(0, YPos + 5), true)
        };
        PathFigure lfigure = new PathFigure(lstart, lsegments, true);
        PathGeometry leftTriangle = new PathGeometry(new[] { lfigure });
        drawingContext.DrawGeometry(Brushes.SteelBlue, null, leftTriangle);

        // draw the cursor right triangle
        Point rstart = new Point(DesiredSize.Width - 5, YPos);
        LineSegment[] rsegments = new[]
        {
           new LineSegment(new Point(DesiredSize.Width, YPos - 5), true),
           new LineSegment(new Point(DesiredSize.Width, YPos + 5), true)
        };
        PathFigure rfigure = new PathFigure(rstart, rsegments, true);
        PathGeometry rightTriangle = new PathGeometry(new[] { rfigure });
        drawingContext.DrawGeometry(Brushes.SteelBlue, null, rightTriangle);
    }
}

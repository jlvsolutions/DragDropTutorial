using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DragDropTutorial;

public class DragVisualAdorner : Adorner
{
    private readonly Rectangle _child;

    private double _offsetLeft = 0;
    public double OffsetLeft
    {
        get { return _offsetLeft; }
        set 
        { 
            _offsetLeft = value;
            UpdateLocation();
        }
    }

    private double _offsetTop = 0;
    public double OffsetTop
    {
        get { return _offsetTop; }
        set
        {
            _offsetTop = value;
            UpdateLocation();
        }
    }

    // Ctor
    public DragVisualAdorner(UIElement adornedElement, Size size, Brush brush) 
        : base(adornedElement)
    {
        _child = new Rectangle()
        {
            Fill = brush,
            Width = size.Width,
            Height = size.Height,
            IsHitTestVisible = false
        };
    }

    /// <summary>
    /// Updates the location of the adorner in one atomic operation.
    /// </summary>
    public void SetOffsets(double left, double top)
    {
        _offsetLeft = left;
        _offsetTop = top;
        UpdateLocation();
    }

    public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
    {
        GeneralTransformGroup result = new GeneralTransformGroup();
        result.Children.Add(base.GetDesiredTransform(transform));
        result.Children.Add(new TranslateTransform(_offsetLeft, _offsetTop));
        return result;
    }

    protected override Size MeasureOverride(Size constraint)
    {
        _child.Measure(constraint);
        return _child.DesiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _child.Arrange(new Rect(finalSize));
        return finalSize;
    }

    protected override Visual GetVisualChild(int index)
    {
        return _child;
    }

    protected override int VisualChildrenCount
    {
        get { return 1; }
    }

    private void UpdateLocation()
    {
        if (Parent is AdornerLayer adornerLayer)
            adornerLayer.Update(AdornedElement);
    }
}

using GraphicsEditor.Business.Domain.Models;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

public class Leaf : ComponentBase
{
    private Shape shape;

    public Leaf(Shape shape)
    {
        this.shape = shape;
        this.SelectionArea = new Rectangle();

        SelectionArea.Margin = shape.Margin;
        SelectionArea.Width = shape.Width;
        SelectionArea.Height = shape.Height;
        SelectionArea.Fill = Brushes.Transparent;
        SelectionArea.StrokeDashArray = new DoubleCollection() { 5, 5 };
    }

    public override void Add(IComponent Component)
    {
        throw new NotImplementedException();
    }

    public override void Remove(IComponent Component)
    {
        throw new NotImplementedException();
    }

    public override void Move(Vector translation)
    {
        Point oldPosition = new Point(this.shape.Margin.Left, this.shape.Margin.Top);
        Point newPosition = Point.Add(oldPosition, translation);

        var newMargin = new Thickness(newPosition.X, newPosition.Y, 0, 0);

        this.shape.Margin = newMargin;
        this.SelectionArea.Margin = newMargin;
    }

    public override void Resize(Vector translation)
    {
        Point oldBottomRightRel = new Point(this.shape.Width, this.shape.Height);
        Point newBottomRightRel = Point.Add(oldBottomRightRel, translation);

        // prevent negative width or height
        if (newBottomRightRel.X < 0) newBottomRightRel.X = 0;
        if (newBottomRightRel.Y < 0) newBottomRightRel.Y = 0;

        this.shape.Width = newBottomRightRel.X;
        this.shape.Height = newBottomRightRel.Y;

        this.SelectionArea.Width = newBottomRightRel.X;
        this.SelectionArea.Height = newBottomRightRel.Y;
    }
}


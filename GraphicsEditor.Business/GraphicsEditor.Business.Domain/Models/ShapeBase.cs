using System;
using System.Windows;
using System.Windows.Shapes;

public class ShapeBase : IComponent
{
    protected Shape shape;

    public ShapeBase(Shape shape)
    {
        this.shape = shape;
    }

    public Shape Shape
    {
        get
        {
            return shape;
        }
    }

    public void Add(IComponent Component)
    {
        throw new NotImplementedException();
    }

    public void Remove(IComponent Component)
    {
        throw new NotImplementedException();
    }

    public void Move(Vector translation)
    {
        Point oldPosition = new Point(this.shape.Margin.Left, this.shape.Margin.Top);
        Point newPosition = Point.Add(oldPosition, translation);

        this.shape.Margin = new Thickness(newPosition.X, newPosition.Y, 0, 0);
    }

    public void Resize(Vector translation)
    {
        Point oldBottomRightRel = new Point(this.shape.Width, this.shape.Height);
        Point newBottomRightRel = Point.Add(oldBottomRightRel, translation);

        this.shape.Width = newBottomRightRel.X;
        this.shape.Height = newBottomRightRel.Y;
    }
}


using GraphicsEditor.Business.Domain.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

public class Composite : ComponentBase
{
    public Composite()
    {
        Children = new List<IComponent>();

        this.SelectionArea = new Rectangle();

        SelectionArea.Fill = Brushes.Transparent;
        SelectionArea.StrokeDashArray = new DoubleCollection() { 5, 5 };
    }

    public List<IComponent> Children
    {
        get;
        private set;
    }

    public override void Add(IComponent component)
    {
        Children.Add(component);
        this.calculateSelectionArea();
    }

    public override void Remove(IComponent component)
    {
        Children.Remove(component);
        this.calculateSelectionArea();
    }

    public override void Move(Vector translation)
    {
        // maybe use parallels here
        foreach(var child in Children)
        {
            child.Move(translation);
        }
        this.calculateSelectionArea();
    }

    // resizing is difficult, because some objects need to be moved as well
    public override void Resize(Vector translation)
    {
        throw new NotImplementedException();
    }

    private void calculateSelectionArea()
    {
        // TODO: this is inefficient

        List<Point> points = new List<Point>();

        foreach(var child in Children)
        {
            var component = child as ComponentBase;

            var childLeft = component.SelectionArea.Margin.Left;
            var childTop = component.SelectionArea.Margin.Top;
            var childWidth = component.SelectionArea.Width;
            var childHeight = component.SelectionArea.Height;

            points.Add(new Point(childLeft, childTop));
            points.Add(new Point(childLeft + childWidth, childTop));
            points.Add(new Point(childLeft, childTop + childHeight));
            points.Add(new Point(childLeft + childWidth, childTop + childHeight));
        }

        var minX = points.Min(p => p.X);
        var minY = points.Min(p => p.Y);
        var maxX = points.Max(p => p.X);
        var maxY = points.Max(p => p.Y);

        this.SelectionArea.Width = maxX - minX;
        this.SelectionArea.Height = maxY - minY;
        this.SelectionArea.Margin = new Thickness(minX, minY, 0, 0);
    }
}


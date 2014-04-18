using GraphicsEditor.Business.Domain.Models;
using System;
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
        var left = double.MaxValue;
        var top = double.MaxValue;
        var width = double.MinValue;
        var height = double.MinValue;

        foreach(var child in Children)
        {
            var component = child as ComponentBase;

            if (component.SelectionArea.Margin.Left < left)
            {
                left = component.SelectionArea.Margin.Left;
            }

            if (component.SelectionArea.Margin.Top < top)
            {
                top = component.SelectionArea.Margin.Top;
            }

            if (component.SelectionArea.Margin.Left + component.SelectionArea.Width > left + width)
            {
                width = component.SelectionArea.Margin.Left + component.SelectionArea.Width - left;
            }

            if (component.SelectionArea.Margin.Top + component.SelectionArea.Height > top + height)
            {
                height = component.SelectionArea.Margin.Top + component.SelectionArea.Height - top;
            }
        }

        this.SelectionArea.Width = width;
        this.SelectionArea.Height = height;
        this.SelectionArea.Margin = new Thickness(left, top, 0, 0);
    }
}


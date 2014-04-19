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
    }

    public List<IComponent> Children
    {
        get;
        private set;
    }

    public override void Add(IComponent component)
    {
        Children.Add(component);

        this.updateSelectionArea();
        this.updateResizeRectangle();
    }

    public override void Remove(IComponent component)
    {
        Children.Remove(component);

        this.updateSelectionArea();
        this.updateResizeRectangle();
    }

    public override void Move(Vector translation)
    {
        // maybe use parallels here
        foreach(var child in Children)
        {
            child.Move(translation);
        }

        this.updateSelectionArea();
        this.updateResizeRectangle();
    }

    // resizing is difficult, because some objects need to be moved as well
    public override void Resize(Vector translation)
    {
        throw new NotImplementedException();
    }

    private void updateSelectionArea()
    {
        double minX, minY, maxX, maxY;
        minX = minY = double.MaxValue;
        maxX = maxY = double.MinValue;

        foreach (var child in Children)
        {
            var component = child as ComponentBase;

            minX = Math.Min(minX, component.SelectionArea.Margin.Left);
            maxX = Math.Max(maxX, component.SelectionArea.Margin.Left + component.SelectionArea.Width);
            minY = Math.Min(minY, component.SelectionArea.Margin.Top);
            maxY = Math.Max(maxY, component.SelectionArea.Margin.Top + component.SelectionArea.Height);
        }

        this.SelectionArea.Margin = new Thickness(minX, minY, 0, 0);
        this.SelectionArea.Width = maxX - minX;
        this.SelectionArea.Height = maxY - minY;
    }

    private void updateResizeRectangle()
    {
        var bottomRightX = this.SelectionArea.Margin.Left + this.SelectionArea.Width;
        var bottomRightY = this.SelectionArea.Margin.Top + this.SelectionArea.Height;

        this.ResizeRectangle.Margin = new Thickness(bottomRightX - 10, bottomRightY - 10, 0, 0);
    }
}


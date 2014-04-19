using GraphicsEditor.Business.Domain.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

public class Composite : ComponentBase
{
    public Composite()
    {
        this.Children = new ObservableCollection<IComponent>();
        this.Children.CollectionChanged += Children_CollectionChanged;
    }

    public ObservableCollection<IComponent> Children
    {
        get;
        private set;
    }

    public override void Add(IComponent component)
    {
        Children.Add(component);
        (component as ComponentBase).HideSelectionArea();
    }

    public override void Remove(IComponent component)
    {
        Children.Remove(component);
    }

    public override void Move(Vector translation)
    {
        // maybe use parallels here
        foreach(var child in Children)
        {
            child.Move(translation);
        }

        this.Children_CollectionChanged(null, null);
    }

    // resizing is difficult, because some objects need to be moved as well
    public override void Resize(Vector translation)
    {
        double scaleX = (this.SelectionArea.Width + translation.X) / this.SelectionArea.Width;
        double scaleY = (this.SelectionArea.Height + translation.Y) / this.SelectionArea.Height;

        Vector scale = new Vector(scaleX, scaleY);
        Point selectionOrigin = new Point(this.SelectionArea.Margin.Left, this.SelectionArea.Margin.Top);

        foreach(var child in this.Children)
        {
            var component = child as ComponentBase;

            Point oldPoint = new Point(component.SelectionArea.Margin.Left, component.SelectionArea.Margin.Top);
            Point newPoint = new Point(oldPoint.X * scale.X - selectionOrigin.X * scale.X + selectionOrigin.X, oldPoint.Y * scale.Y - selectionOrigin.Y * scale.Y + selectionOrigin.Y);

            Vector moveVector = Point.Subtract(newPoint, oldPoint);
            Vector resizeVector = new Vector((scale.X - 1) * component.SelectionArea.Width, (scale.Y - 1) * component.SelectionArea.Height);

            child.Move(moveVector);
            child.Resize(resizeVector);
        }

        this.Children_CollectionChanged(null, null);
    }

    void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (this.Children.Count == 0)
        {
            this.HideSelectionArea();
            return;
        }

        this.DisplaySelectionArea();

        double minX, minY, maxX, maxY;
        minX = minY = double.MaxValue;
        maxX = maxY = double.MinValue;

        foreach (var child in this.Children)
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

        var bottomRightX = this.SelectionArea.Margin.Left + this.SelectionArea.Width;
        var bottomRightY = this.SelectionArea.Margin.Top + this.SelectionArea.Height;

        this.ResizeRectangle.Margin = new Thickness(bottomRightX - 10, bottomRightY - 10, 0, 0);
    }
}


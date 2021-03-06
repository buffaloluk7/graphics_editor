﻿using GraphicsEditor.Business.Domain.Models;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

public class Leaf : ComponentBase
{
    public Leaf(Shape shape)
    {
        this.Shape = shape;

        this.updateSelectionArea();
        this.updateResizeRectangle();
    }

    public Shape Shape
    {
        get;
        private set;
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
        Point oldPosition = new Point(this.Shape.Margin.Left, this.Shape.Margin.Top);
        Point newPosition = Point.Add(oldPosition, translation);

        var newMargin = new Thickness(newPosition.X, newPosition.Y, 0, 0);

        this.Shape.Margin = newMargin;

        this.updateSelectionArea();
        this.updateResizeRectangle();
    }

    public override void Resize(Vector translation)
    {
        Point oldBottomRightRel = new Point(this.Shape.Width, this.Shape.Height);
        Point newBottomRightRel = Point.Add(oldBottomRightRel, translation);

        // prevent negative width or height
        if (newBottomRightRel.X < 0) newBottomRightRel.X = 0;
        if (newBottomRightRel.Y < 0) newBottomRightRel.Y = 0;

        this.Shape.Width = newBottomRightRel.X;
        this.Shape.Height = newBottomRightRel.Y;

        this.updateSelectionArea();
        this.updateResizeRectangle();
    }

    private void updateSelectionArea()
    {
        this.SelectionArea.Margin = this.Shape.Margin;
        this.SelectionArea.Width = this.Shape.Width;
        this.SelectionArea.Height = this.Shape.Height;
    }

    private void updateResizeRectangle()
    {
        var bottomRightX = this.SelectionArea.Margin.Left + this.SelectionArea.Width;
        var bottomRightY = this.SelectionArea.Margin.Top + this.SelectionArea.Height;

        this.ResizeArea.Margin = new Thickness(bottomRightX - 10, bottomRightY - 10, 0, 0);
    }
}


﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool
//     Changes to this file will be lost if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Shapes;
using System.Windows;

public abstract class GeometryObjectBase : ICloneable, IComponent, IDrawable
{
    protected Shape shape;

	public virtual ICloneable Clone()
	{
        return this.MemberwiseClone() as ICloneable;
	}

	public abstract void Add(IComponent component);

	public abstract void Remove(IComponent component);

	public abstract void Draw();

    public void Move(Point newPosition)
    {
        shape.Margin = new Thickness(newPosition.X, newPosition.Y, 0, 0);
    }

    public void Resize(int newWidth, int newHeight)
    {
        shape.Width = newWidth;
        shape.Height = newHeight;
    }
}

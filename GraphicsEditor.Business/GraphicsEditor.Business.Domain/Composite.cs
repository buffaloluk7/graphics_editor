using System;
using System.Collections.Generic;
using System.Windows;

public class Composite : IComponent
{
    private List<IComponent> children;

    public Composite()
    {
        children = new List<IComponent>();
    }

    public virtual void Add(IComponent component)
    {
        children.Add(component);
    }

    public virtual void Remove(IComponent component)
    {
        children.Remove(component);
    }

    public void Move(Point location)
    {
        foreach (var component in this.children)
        {
            component.Move(location);
        }
    }

    public void Resize(Point size)
    {
        throw new NotImplementedException();
    }
}


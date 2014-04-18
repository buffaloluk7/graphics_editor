using GraphicsEditor.Business.Domain.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;

public class Composite : ComponentBase
{
    private List<IComponent> children;

    public Composite()
    {
        children = new List<IComponent>();
    }

    public override void Add(IComponent component)
    {
        children.Add(component);
    }

    public override void Remove(IComponent component)
    {
        children.Remove(component);
    }

    public override void Move(Vector translation)
    {
        // maybe use parallels here
        foreach(var child in children)
        {
            child.Move(translation);
        }
    }

    // resizing is difficult, because some objects need to be moved as well
    public override void Resize(Vector translation)
    {
        throw new NotImplementedException();
    }
}


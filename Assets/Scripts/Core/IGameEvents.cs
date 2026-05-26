using System;
using System.Collections.Generic;
using System.Text;

public interface IGameEvents
{
    public event Action OnExploreEntered;
    public event Action OnPlanEntered;
    public event Action OnExecuteEntered;
}
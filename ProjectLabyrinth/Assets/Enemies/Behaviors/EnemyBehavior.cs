using UnityEngine;

public abstract class EnemyBehavior : MonoBehaviour
{
    protected EnemyController controller;

    public virtual void Initialize(EnemyController controller)
    {
        this.controller = controller;
    }

    public abstract void OnUpdate();
}

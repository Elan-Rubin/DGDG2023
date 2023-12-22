using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    private Transform player;
    private List<AIPath> pathfinders = new();
    [SerializeField] private AstarPath grid;
    void Start()
    {
        StartCoroutine(nameof(DoubleLateStartCoroutine));
        InvokeRepeating(nameof(UpdateTargets), 0.25f, 1f);
    }
    private IEnumerator DoubleLateStartCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        /*yield return null;
        yield return null;*/
        DoubleLateStart();
    }
    private void DoubleLateStart()
    {
        grid.Scan();

        player = PlayerMovement.Instance.GetComponent<Transform>();
        var p = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var x in p)
            pathfinders.Add(x.GetComponent<AIPath>());
    }

    void UpdateTargets()
    {
        foreach (var p in pathfinders)
        {
            var newDest = Vector2.zero;
            var e = p.GetComponent<Enemy>();

            switch (e.Behavior)
            {
                case EnemyBehavior.Still:
                    newDest = p.transform.position;
                    break;
                case EnemyBehavior.Wander:
                    newDest = p.transform.position + DistractionVector(e.Distraction);
                    break;
                case EnemyBehavior.Follow:
                    newDest = player.position + DistractionVector(e.Distraction);
                    break;
                case EnemyBehavior.Charge:
                    newDest = player.position;
                    break;
            }
            p.destination = newDest;
        }
    }
    private Vector3 DistractionVector(int amount)
    {
        return new Vector2(Random.Range((float)-amount, amount), Random.Range((float)-amount, amount));
    }
}

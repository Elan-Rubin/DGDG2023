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
        InvokeRepeating(nameof(UpdateTargets), 0f, 0.25f);
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
            p.destination = player.position + new Vector3(Random.Range(-1f,1),Random.Range(-1f,1));
    }
}

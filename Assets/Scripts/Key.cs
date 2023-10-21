using System.Collections;
using System.Collections.Generic;
using UnityEditor.MemoryProfiler;
using UnityEngine;

public class Key : MonoBehaviour
{
    [SerializeField] private Door door;
    void Start()
    {

    }

    void Update()
    {

    }
    public void Collect() => StartCoroutine(nameof(CollectCoroutine));
    private IEnumerator CollectCoroutine()
    {
        var origin = transform.position;
        var destination = door.gameObject.transform.position;
        var counter = 0f;
        while (Vector2.Distance(transform.position, destination) > 0.1f)
        {
            transform.position = Vector3.Slerp(origin, destination, counter += Time.deltaTime);
            yield return null;
        }
        door.Unlock();
        gameObject.SetActive(false);
    }
}

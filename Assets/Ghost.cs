using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    private Vector2 currentPosition, targetPosition;
    float timer = 0f;
    float timer2 = 0f;
    bool caught;


    void Start()
    {
        currentPosition = targetPosition = transform.position;
        timer2 = Random.Range(0, 1);
    }

    void Update()
    {
        if (caught) return;
        timer -= Time.deltaTime;
        timer2 += Time.deltaTime / 3f;
        if (timer < 0)
        {
            //var dif = PlayerMovement.Instance.PlayerPosition - currentPosition;
            targetPosition = Vector2.MoveTowards(currentPosition,PlayerMovement.Instance.PlayerPosition,-1);
            targetPosition += new Vector2(Random.Range(-1, 1), Random.Range(-1, 1)) / 2f;
            transform.GetChild(0).GetComponent<SpriteRenderer>().flipX = targetPosition.x < currentPosition.x;
            timer = Random.Range(1f,3f);
        }

        transform.position = currentPosition = Vector2.Lerp(currentPosition, targetPosition, Time.deltaTime * 3f);
        transform.GetChild(0).transform.localPosition = Vector2.up * Mathf.Sin(timer2 * 360f * Mathf.Deg2Rad);
        if (Input.GetMouseButtonDown(0) && Vector2.Distance(transform.position, CameraManager.Instance.LaggedMousePos) < 1f) StartCoroutine(nameof(Caught));
    }
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(nameof(Caught));
        }
    }
    private IEnumerator Caught()
    {
        caught = true;
        transform.DOMove(PlayerMovement.Instance.PlayerPosition, 1f);
        yield return null;
    }
}

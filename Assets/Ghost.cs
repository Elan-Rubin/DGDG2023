using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Ghost : MonoBehaviour
{
    private Vector2 currentPosition, targetPosition, prevTargetPosition;
    float timer, timer2, timer3;
    Image circle;
    TextMeshProUGUI text;
    bool caught;


    void Start()
    {
        currentPosition = targetPosition = transform.position;
        timer2 = Random.Range(0, 1);
        circle = transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>();
        text = transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        timer -= Time.deltaTime;
        timer2 += Time.deltaTime / 3f;
        if (timer < 0 && !caught)
        {
            //var dif = PlayerMovement.Instance.PlayerPosition - currentPosition;
            targetPosition = Vector2.MoveTowards(currentPosition, PlayerMovement.Instance.PlayerPosition, -1);
            targetPosition += new Vector2(Random.Range(-1, 1), Random.Range(-1, 1)) / 2f;
            transform.GetChild(0).GetComponent<SpriteRenderer>().flipX = targetPosition.x < currentPosition.x;
            timer = Random.Range(1f, 3f);
        }

        text.gameObject.SetActive(caught || timer3 > 0);
        if (!caught) transform.position = currentPosition = Vector2.Lerp(currentPosition, targetPosition, Time.deltaTime * 3f);
        if (!caught) transform.GetChild(0).transform.localPosition = Vector2.up * Mathf.Sin(timer2 * 360f * Mathf.Deg2Rad);
        if (!caught&&timer3>0) timer3 -= Time.deltaTime;
        if (Vector2.Distance(transform.position, CameraManager.Instance.LaggedMousePos) < 1f)
        {
            if (caught)
            {
                if (timer3 < 1.5f) timer3 += Time.deltaTime;
                circle.fillAmount = timer3 / 1.5f;
                text.text = $"{(int)(timer3 / 1.5f * 100)}";

                //targetPosition = PlayerMovement.Instance.PlayerPosition;
            }
            else
            {
                prevTargetPosition = targetPosition;
            }
            caught = true;
        }
        else if (caught)
        {
            caught = false;
            targetPosition = prevTargetPosition;
        }
    }
    private void OnMouseOver()
    {
        caught = true;
        /*if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(nameof(Caught));
        }*/
    }
    private void OnMouseExit()
    {
        caught = false;
        targetPosition = prevTargetPosition;
    }
    /*private IEnumerator Caught()
    {
        caught = true;
        transform.DOMove(PlayerMovement.Instance.PlayerPosition, 1f);
        yield return null;
    }*/
}

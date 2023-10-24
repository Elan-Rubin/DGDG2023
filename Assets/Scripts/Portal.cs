using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Portal : MonoBehaviour
{
    private bool turningOn, on;
    

    void Update()
    {
        if (RevivalScript.Instance.Dead) return;
        if(!turningOn&&!on&&Vector2.Distance(PlayerMovement.Instance.PlayerPosition, transform.position) < 2)
        {
            StartCoroutine(nameof(TurnOnPortal));
        } 
        else if(on && Vector2.Distance(PlayerMovement.Instance.PlayerPosition, transform.position) < 1)
        {
            GameManager.Instance.NextLevel();
        }
    }
    private IEnumerator TurnOnPortal()
    {
        GetComponent<Animator>().SetTrigger("activate");
        turningOn = true;
        yield return new WaitForSeconds(2);
        on = true;
    }
}

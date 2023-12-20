using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

public class Portal : MonoBehaviour
{
    private bool turningOn, on, entered;

    void Update()
    {
        if (RevivalScript.Instance.Dead) return;

        if (!turningOn && !on && Vector2.Distance(PlayerMovement.Instance.PlayerPosition, transform.position) < 2)
        {
            StartCoroutine(nameof(TurnOnPortal));
        }
        else if (!entered && on && Vector2.Distance(PlayerMovement.Instance.PlayerPosition, transform.position) < 1)
        {
            GameManager.Instance.NextLevel();
            SoundManager.Instance.PlaySoundEffect("portalenter");
            entered = true;
        }
    }

    private IEnumerator TurnOnPortal()
    {
        if (LevelGenerator.Instance.AllEnemiesDead())
        {
            SoundManager.Instance.PlaySoundEffect("portalactivate");

            GetComponent<Animator>().SetTrigger("activate");
            turningOn = true;
            yield return new WaitForSeconds(2);
            on = true;
        }
    }
}

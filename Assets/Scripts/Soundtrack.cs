using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soundtrack : MonoBehaviour
{
    void Start()
    {
        GameManager.Instance.PlayerDeath += SwitchToDead;
        GameManager.Instance.PlayerReborn += SwitchToAlive;
    }
    private void SwitchToDead()
    {
        GetComponent<AudioSource>().DOFade(0, 2f);
        transform.GetChild(0).GetComponent<AudioSource>().DOFade(1, 2f);
    }
    private void SwitchToAlive()
    {
        GetComponent<AudioSource>().DOFade(1, 2f);
        transform.GetChild(0).GetComponent<AudioSource>().DOFade(0, 2f);
    }
}

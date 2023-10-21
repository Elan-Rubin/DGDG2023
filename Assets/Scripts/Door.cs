using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }
    public void Unlock()
    {
        SoundManager.Instance.PlaySoundEffect("doorUnlock");
        gameObject.SetActive(false);
    }
}

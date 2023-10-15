using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Health : MonoBehaviour
{
    public int health = 1;
    private static Health instance;
    public static Health Instance { get { return instance; } }

    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void Damage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            RevivalScript.Instance.Rewind();
        }
    }

    public void addHealth()
    {
        health++;
    }    

    // Update is called once per frame
    void Update()
    {
       
    }
}

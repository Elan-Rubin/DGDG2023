using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class betterMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    private float movementX;
    private float movementY;
    public float speed = 0;
    public float accel = 1f;

    public float moveSpeed = 10f;

    public bool isGrounded = false;



    private void Start()
    {
        
    }
    void OnCollisionStay2D()
    {
        isGrounded = true;
    }

    void OnCollisionExit2D()
    {
        isGrounded = false;
    }



    void Update()
    {
       /* if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(up, ForceMode2D.Impulse);
            isGrounded = false;
        }*/

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            rb.AddForce(Vector2.left * moveSpeed);
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            rb.AddForce(Vector2.right * moveSpeed);

        }
        else if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            rb.AddForce(Vector2.up * moveSpeed);
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.DownArrow))
        {
            rb.AddForce(Vector2.down * moveSpeed);
        }



    }
}

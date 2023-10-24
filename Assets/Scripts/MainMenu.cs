using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject comic;

    private int clickCounter;
    private bool comicActive;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (clickCounter > comic.transform.childCount - 1)
            {
                comic.SetActive(false);
            }

            var c = comic.transform.GetChild(clickCounter).gameObject;
            c.SetActive(!c.activeSelf);
            if (c.activeSelf)
            {
                c.transform.DOPunchScale(Vector2.one * 0.05f, 0.1f);
                //c.transform.DOShakeRotation(0.1f);

            }
            clickCounter++;
        }
    }

    public void PlayGame()
    {
        //This is the line that loads up the next scene after pressing play. Feel free to revise/edit
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

    }

    public void QuitGame()
    {
        Debug.Log("This is for testing the quit function");
        Application.Quit();
    }


}

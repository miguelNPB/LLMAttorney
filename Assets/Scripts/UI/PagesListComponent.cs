using UnityEngine;
using System.Collections.Generic;
public class PagesListComponent : MonoBehaviour
{
    public GameObject rightButton;
    public GameObject leftButton;
    public GameObject pagesMenuContainer;
    public List<GameObject> pages = new List<GameObject>();

    protected bool _open = false;
    protected int _index = 0;
    public void GoRight()
    {
        if (_open)
            ChangePage(true);
    }

    public void GoLeft()
    {
        if (_open)
            ChangePage(false);
    }

    /// <summary>
    /// Activa el menu si estaba desactivado y lo desactiva si estaba activado
    /// </summary>
    public void ToggleMenu()
    {
        _open = !_open;

        ToggleMenu(_open);
    }


    /// <summary>
    /// Activa el menu segun el paraemtro bool on
    /// </summary>
    /// <param name="on"></param>
    public void ToggleMenu(bool on)
    {
        Debug.Log("Cambia ayuda");

        _open = on;

        pagesMenuContainer.SetActive(on);

        foreach (GameObject page in pages)
        {
            page.SetActive(false);
        }

        if (_open)
        {
            _index = 0;
            pages[0].SetActive(true);
            rightButton.SetActive(true);
        }
    }
    protected void ChangePage(bool right)
    {
        rightButton.SetActive(true);
        leftButton.SetActive(true);

        if (right)
        {
            _index = (_index + 1) % pages.Count;

            if (_index + 1 >= pages.Count)
                rightButton.SetActive(false);
        }
        else
        {
            _index--;
            if (_index < 0)
                _index = pages.Count - 1;

            if (_index - 1 < 0)
                leftButton.SetActive(false);
        }

        foreach (GameObject page in pages)
        {
            page.SetActive(false);
        }

        pages[_index].SetActive(true);
    }
    private void Start()
    {
        if (pages.Count == 0)
        {
            Debug.LogError("Lista de pages vacia");
            return;
        }
        
        if (rightButton != null)
            rightButton.SetActive(false);

        if (leftButton != null)
            leftButton.SetActive(false);

        foreach (GameObject page in pages)
        {
            page.SetActive(false);
        }
    }
}

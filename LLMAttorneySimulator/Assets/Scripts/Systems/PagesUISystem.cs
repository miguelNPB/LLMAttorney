using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Clase para gestionar un menu de paginas como los del menu de ayuda.
/// </summary>
public class PagesUISystem : MonoBehaviour
{
    public GameObject rightButton;
    public GameObject leftButton;
    public GameObject pagesMenuContainer;
    public List<GameObject> pages = new List<GameObject>();

    protected bool _open = false;
    protected int _index = 0;

    /// <summary>
    /// Cambia a la siguiente pagina
    /// </summary>
    public void GoRight()
    {
        if (_open)
            changePage(true);
    }

    /// <summary>
    /// Cambia hacia la pagina previa
    /// </summary>
    public void GoLeft()
    {
        if (_open)
            changePage(false);
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

            if (rightButton != null)
            {
                rightButton.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Metodo privado para cambiar de pagina
    /// </summary>
    /// <param name="right"></param>
    private void changePage(bool right)
    {
        if (rightButton != null)
        {
            rightButton.SetActive(true);
        }
        if (leftButton != null)
        {
            leftButton.SetActive(true);
        }

        if (right)
        {
            _index = (_index + 1) % pages.Count;

            if (_index + 1 >= pages.Count && rightButton != null)
            {
                rightButton.SetActive(false);
            }
        }
        else
        {
            _index--;
            if (_index < 0)
            {
                _index = pages.Count - 1;
            }

            if (_index - 1 < 0 && leftButton != null)
            {
                leftButton.SetActive(false);
            }
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

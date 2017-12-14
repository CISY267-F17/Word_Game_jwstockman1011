using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Letter : MonoBehaviour
{
    private char _c;    //the char shown on this letter
    public TextMesh tMesh;  //the TextMesh shows the char
    public Renderer tRend;  //renderer of 3d text. determines if char is visible
    public bool big = false;    //big letters act differently

    void Awake()
    {
        tMesh = GetComponentInChildren<TextMesh>();
        tRend = tMesh.GetComponent<Renderer>();
        visible = false;
    }

    //used to get/set _c and the letter shown by 3D text
    public char c
    {
        get
        {
            return (_c);
        }

        set
        {
            _c = value;
            tMesh.text = _c.ToString();
        }
    }

    //gets or sets _c as a string
    public string str
    {
        get
        {
            return (_c.ToString());
        }

        set
        {
            c = value[0];
        }
    }

    //enables/disables renderer for 3D text
    public bool visible
    {
        get
        {
            return (tRend.enabled);
        }

        set
        {
            tRend.enabled = value;
        }
    }

    //gets/sets color of rounded rectangle
    public Color color
    {
        get
        {
            return (GetComponent<Renderer>().material.color);
        }

        set
        {
            GetComponent<Renderer>().material.color = value;
        }
    }

    //Sets position of letter GO
    public Vector3 pos
    {
        set
        {
            transform.position = value;
        }
    }
}

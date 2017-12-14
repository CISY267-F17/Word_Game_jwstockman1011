using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wyrd : MonoBehaviour
{
    public string str;  //a string representation of the word
    public List<Letter> letters = new List<Letter>();
    public bool found = false;  //true if player found this word

    //a property to set the visibility of the 3D text of each word
    public bool visible
    {
        get
        {
            if (letters.Count == 0)
            {
                return (false);
            }

            return (letters[0].visible);
        }

        set
        {
            foreach (Letter lett in letters)
            {
                lett.visible = value;
            }
        }
    }

    //a property to set the color of the rounded rectangle of each letter
    public Color color
    {
        get
        {
            if (letters.Count == 0)
            {
                return (Color.black);
            }

            return (letters[0].color);
        }

        set
        {
            foreach (Letter lett in letters)
            {
                lett.color = value;
            }
        }
    }

    //adds a letter to letters
    public void Add(Letter lett)
    {
        letters.Add(lett);
        str += lett.c.ToString();
    }
}

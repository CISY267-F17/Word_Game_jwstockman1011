using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WordLevel
{
    public int levelNum;
    public int longWordIndex;
    public string word;

    //a dictionary of all letters in word
    public Dictionary<char, int> charDict;

    //all words that can be spelled with the letters in charDict
    public List<string> subWords;

    public static Dictionary<char, int> MakeCharDict(string w)
    {
        Dictionary<char, int> dict = new Dictionary<char, int>();
        char c;
        for (int i = 0; i < w.Length; i++)
        {
            c = w[i];
            if (dict.ContainsKey(c))
            {
                dict[c]++;
            }

            else
            {
                dict.Add(c, 1);
            }
        }
        return (dict);
    }

    public static bool CheckWordInLevel(string str, WordLevel level)
    {
        Dictionary<char, int> counts = new Dictionary<char, int>();
        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];

            //if the charDict contains char c
            if (level.charDict.ContainsKey(c))
            {
                //if counts doesn't already have char c as a key
                if (!counts.ContainsKey(c))
                {
                    //then add a new key with a value of 1
                    counts.Add(c, 1);
                }

                else
                {
                    //otherwise, add 1 to the current value
                    counts[c]++;
                }

                //additional minstances of char c means...
                if (counts[c] > level.charDict[c])
                {
                    return (false);
                }
            }

            else
            {
                //the char c isn't in the word
                return (false);
            }
        }
        return (true);
    }
}

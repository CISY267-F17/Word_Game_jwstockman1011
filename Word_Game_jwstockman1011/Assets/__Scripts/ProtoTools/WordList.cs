using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordList : MonoBehaviour
{
    public static WordList S;

    public TextAsset wordListText;
    public int numToParseBeforeYield = 10000;
    public int wordLengthMin = 3;
    public int wordLengthMax = 7;
    public bool ____________;

    public int currLine = 0;
    public int totalLines;
    public int longWordCount;
    public int wordCount;

    //following variables private to keep from inspector. would slow everything down because they are too long
    private string[] lines;
    private List<string> longWords;
    private List<string> words;

    void Awake()
    {
        S = this; //set up singleton
    }

	// Use this for initialization
	public void Init()
    {
        //split the text of wordListText into big string[]
        lines = wordListText.text.Split('\n');
        totalLines = lines.Length;

        StartCoroutine(ParseLines());
	}

    public IEnumerator ParseLines()
    {
        string word;
        longWords = new List<string>();
        words = new List<string>();

        for (currLine = 0; currLine < totalLines; currLine++)
        {
            word = lines[currLine];

            //if a word is as long as wordLengthMax...
            if (word.Length == wordLengthMax)
            {
                //...then store it in longWords
                longWords.Add(word);
            }

            //if a word is between wordLengthMin and wordLengthMax...
            if (word.Length >= wordLengthMin && word.Length <= wordLengthMax)
            {
                //then add it to the list of all valid words
                words.Add(word);
            }

            //should Coroutine yeild? make it
            if (currLine % numToParseBeforeYield == 0)
            {
                longWordCount = longWords.Count;
                wordCount = words.Count;

                //yield execution until next frame
                yield return null;
            }
        }

        //send a message to let this GO know the parse is done
        gameObject.SendMessage("WordListParseComplete");

    }

    //allow other cklasses to access private List<string>s
    public List<string> GetWords()
    {
        return (words);
    }

    public string GetWord(int ndx)
    {
        return (words[ndx]);
    }

    public List<string> GetLongWords()
    {
        return (longWords);
    }

    public string GetLongWord(int ndx)
    {
        return (longWords[ndx]);
    }
	
}

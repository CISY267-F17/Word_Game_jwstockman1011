using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum GameMode
{
    preGame,    //before the game starts
    loading,    //the word list is loading and being mparsed
    makeLevel,  //the individual wordLevel is being created
    levelPrep,  //the level visuals are instantiated 
    intLevel,   //the level is in progress
}

public class WordGame : MonoBehaviour
{
    public static WordGame S;

    public GameObject prefabLetter;
    public Rect wordArea = new Rect(-24, 19, 48, 28);
    public float letterSize = 1.5f;
    public bool showAllWyrds = true;
    public float bigLetterSize = 4f;

    public bool ____________;

    public GameMode mode = GameMode.preGame;
    public WordLevel currLevel;
    public List<Wyrd> wyrds;

    void Awake()
    {
        S = this; //Assign the singleton
    }

    void Start()
    {
        mode = GameMode.loading;

        //tell wordlist to start parsing all the words
        WordList.S.Init();
    }

    //called by the SendMessage() command from WordList
    public void WordListParseComplete()
    {
        mode = GameMode.makeLevel;
        //make a level and assign it to currLevel
        currLevel = MakeWordLevel();
    }

    public WordLevel MakeWordLevel(int levelNum = -1)
    {
        WordLevel level = new WordLevel();
        if (levelNum == -1)
        {
            //pick a random level
            level.longWordIndex = Random.Range(0, WordList.S.longWordCount);
        }

        else
        {
            //to be added later
        }

        level.levelNum = levelNum;
        level.word = WordList.S.GetLongWord(level.longWordIndex);
        level.charDict = WordLevel.MakeCharDict(level.word);

        //see if each word can be spelled with avaliable letters
        StartCoroutine(FindSubWordsCoroutine(level));

        //return level before coroutine finishes
        return (level);
    }

    public IEnumerator FindSubWordsCoroutine(WordLevel level)
    {
        level.subWords = new List<string>();
        string str;

        //very fast
        List<string> words = WordList.S.GetWords();

        //iterate throught all of the words in the wordlist
        for (int i = 0; i < WordList.S.wordCount; i++)
        {
            str = words[i];

            //check whether each one can be spelled using level.charDict
            if (WordLevel.CheckWordInLevel(str, level))
            {
                level.subWords.Add(str);
            }

            //Yield if we've parsed a lot of words this frame
            if (i % WordList.S.numToParseBeforeYield == 0)
            {
                //yield until the next frame
                yield return null;
            }
        }

        //sort alphabetically
        level.subWords.Sort();

        //now sort by length
        level.subWords = SortWordsByLength(level.subWords).ToList();

        //complete
        SubWordSearchComplete();
    }

    public static IEnumerable<string> SortWordsByLength(IEnumerable<string> e)
    {
        //use LINQ to sort array
        var sorted = from s in e
                     orderby s.Length ascending
                     select s;
        return sorted;
    }

    public void SubWordSearchComplete()
    {
        mode = GameMode.levelPrep;
        Layout(); //call the Layout() function after SubWordSearch
    }

    void Layout()
    {
        //place the letters for each subword of currLevel on screen
        wyrds = new List<Wyrd>();

        //Declare a lot of variables that will be used in this method
        GameObject go;
        Letter lett;
        string word;
        Vector3 pos;
        float left = 0;
        float columnWidth = 3;
        char c;
        Color col;
        Wyrd wyrd;

        //determineb how many rows of Letters will fit on screen
        int numRows = Mathf.RoundToInt(wordArea.height / letterSize);

        //Make a Wyrd of each level.subWord
        for (int i = 0; i < currLevel.subWords.Count; i++)
        {
            wyrd = new Wyrd();
            word = currLevel.subWords[i];

            //if the word is longer that column width, expand it
            columnWidth = Mathf.Max(columnWidth, word.Length);

            //Instantiate a prefabLetter for each letter in the word
            for (int j = 0; j < word.Length; j++)
            {
                c = word[j];    //grab the jth letter of the word
                go = Instantiate(prefabLetter) as GameObject;
                lett = go.GetComponent<Letter>();
                lett.c = c; //set the c of the letter

                //position the letter
                pos = new Vector3(wordArea.x + left + j * letterSize, wordArea.y, 0);

                //The % here makes multiple columns line up
                pos.y -= (i % numRows) * letterSize;
                lett.pos = pos;
                go.transform.localScale = Vector3.one * letterSize;
                wyrd.Add(lett);
            }

            if (showAllWyrds)
            {
                wyrd.visible = true;
            }

            wyrds.Add(wyrd);

            //If we've gotten to the numRows(th) row, start a new column
            if (i % numRows == numRows - 1)
            {
                left += (columnWidth + 0.5f) * letterSize;
            }
        }
    }
}

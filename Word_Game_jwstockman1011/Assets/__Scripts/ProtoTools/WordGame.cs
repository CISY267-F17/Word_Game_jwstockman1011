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
    public Color bigColorDim = new Color(0.8f, 0.8f, 0.8f);
    public Color bigColorSelected = Color.white;
    public Vector3 bigLetterCenter = new Vector3(0, -16, 0);
    public List<float> scoreFontSizes = new List<float> { 24, 36, 36, 1 };
    public Vector3 scoreMidPoint = new Vector3(1, 1, 0);
    public float scoreComboDelay = 0.5f;
    public Color[] wyrdPalette;

    public bool ____________;

    public GameMode mode = GameMode.preGame;
    public WordLevel currLevel;
    public List<Wyrd> wyrds;
    public List<Letter> bigLetters;
    public List<Letter> bigLettersActive;
    public string testWord;
    private string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

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

                //move lett above the screen
                lett.position = pos + Vector3.up * (20 + i % numRows);
                //set position to interpolate to
                lett.pos = pos;
                ////incriment lett.timeStart to move wyrds at different times
                lett.timeStart = Time.time + i * 0.05f;

                go.transform.localScale = Vector3.one * letterSize;
                wyrd.Add(lett);
            }

            if (showAllWyrds)
            {
                wyrd.visible = true;
            }

            //color the wyrd based on length
            wyrd.color = wyrdPalette[word.Length - WordList.S.wordLengthMin];

            wyrds.Add(wyrd);

            //If we've gotten to the numRows(th) row, start a new column
            if (i % numRows == numRows - 1)
            {
                left += (columnWidth + 0.5f) * letterSize;
            }
        }

        //place the big letters and instantiate their List<>s
        bigLetters = new List<Letter>();
        bigLettersActive = new List<Letter>();

        //create a big letter fopr each letter in the target word
        for (int i = 0; i < currLevel.word.Length; i++)
        {
            c = currLevel.word[i];
            go = Instantiate(prefabLetter) as GameObject;
            lett = go.GetComponent<Letter>();
            lett.c = c;
            go.transform.localScale = Vector3.one * bigLetterSize;

            //Set initial; position of big letters below screen
            pos = new Vector3(0, -100, 0);
            lett.pos = pos;

            //incriment lett.timeStart so bigLetters come in last
            lett.timeStart = Time.time + currLevel.subWords.Count * 0.05f;
            lett.easingCurve = Easing.Sin = "-0.18"; //bouncy easing

            col = bigColorDim;
            lett.color = col;
            lett.visible = true; //always true for big letters
            lett.big = true;
            bigLetters.Add(lett);
        }

        //shuffle the big letters
        bigLetters = ShuffleLetters(bigLetters);

        //Arrange them on screen
        ArrangeBigLetters();

        //set mode to be in game
        mode = GameMode.intLevel;
    }

    //shuffle that bitch up and return the result
    List<Letter> ShuffleLetters(List<Letter> letts)
    {
        List<Letter> newL = new List<Letter>();
        int ndx;
        while (letts.Count > 0)
        {
            ndx = Random.Range(0, letts.Count);
            newL.Add(letts[ndx]);
            letts.RemoveAt(ndx);
        }
        return (newL);
    }

    //arrange letters on screen
    void ArrangeBigLetters()
    {
        //halfwidth allows letters to be centered
        float halfWidth = ((float)bigLetters.Count) / 2f - 0.5f;
        Vector3 pos;
        for (int i = 0; i < bigLetters.Count; i++)
        {
            pos = bigLetterCenter;
            pos.x += (i - halfWidth) * bigLetterSize;
            bigLetters[i].pos = pos;
        }

        //bigLettersActive
        halfWidth = ((float)bigLettersActive.Count) / 2f - 0.5f;
        for (int i = 0; i < bigLettersActive.Count; i++)
        {
            pos = bigLetterCenter;
            pos.x += (i - halfWidth) * bigLetterSize;
            pos.y += bigLetterSize*1.25f;
            bigLettersActive[i].pos = pos;
        }
    }

    void Update()
    {
        Letter lett;
        char c;

        switch (mode)
        {
            case GameMode.intLevel:
                foreach (char cIt in Input.inputString)
                {
                    c = System.Char.ToUpperInvariant(cIt);

                    //check to see if it's an uppercase letter
                    if (upperCase.Contains(c))
                    {
                        lett = FindNextLetterByChar(c);
                        if (lett != null)
                        {
                            testWord += c.ToString();
                            bigLettersActive.Add(lett);
                            bigLettersActive.Remove(lett);
                            lett.color = bigColorSelected;
                            ArrangeBigLetters();
                        }
                    }

                    if (c == '\b')
                    {
                        if (bigLettersActive.Count == 0)
                        {
                            return;
                        }

                        if (testWord.Length > 1)
                        {
                            testWord = testWord.Substring(0, testWord.Length - 1);
                        }

                        else
                        {
                            testWord = "";
                        }

                        lett = bigLettersActive[bigLettersActive.Count - 1];
                        bigLettersActive.Remove(lett);
                        bigLetters.Add(lett);
                        lett.color = bigColorDim;
                        ArrangeBigLetters();
                    }

                    if (c == '\n' || c == '\r')
                    {
                        StartCoroutine(CheckWord());
                    }

                    if (c == ' ')
                    {
                        bigLetters = ShuffleLetters(bigLetters);
                        ArrangeBigLetters();
                    }
                }

                break;
        }
    }

    //finds avaliable letter char c in bigLetters
    Letter FindNextLetterByChar(char c)
    {
        foreach (Letter l in bigLetters)
        {
            if (l.c == c)
            {
                return (l);
            }
        }

        return (null);
    }

    public IEnumerator CheckWord()
    {
        string subWord;
        bool foundTestWord = false;

        //make list to hold indices of words in textWord
        List<int> containedWords = new List<int>();

        //Iterate through each word in currLevel.subWords
        for (int i = 0; i < currLevel.subWords.Count; i++)
        {
            //if the ith wyrd in screen has already been found, then continue and skip the rest of this iteration
            if (wyrds[i].found)
            {
                continue;
            }

            subWord = currLevel.subWords[i];

            //if this subWord is the testWord, highlight it
            if (string.Equals(testWord, subWord))
            {
                HighlightWyrd(i);
                Score(wyrds[i], 1); //score the testWord
                foundTestWord = true;
            }

            else if (testWord.Contains(subWord))
            {
                containedWords.Add(i);
            }
        }

        if (foundTestWord)
        {
            int numContained = containedWords.Count;
            int ndx;
            for (int i = 0; i < containedWords.Count; i++)
            {
                //yeild for a bit before highlighting each word
                yield return (new WaitForSeconds(scoreComboDelay));
                ndx = numContained - i - 1;
                HighlightWyrd(containedWords[ndx]);
                Score(wyrds[containedWords[ndx]], i + 2); //score other words
            }
        }

        //clear big active letters regardless of whether testWord was Contained
        ClearBigLettersActive();
    }

    //highlight a wyrd
    void HighlightWyrd(int ndx)
    {
        //activate the subWord
        wyrds[ndx].found = true;
        wyrds[ndx].color = (wyrds[ndx].color + Color.white) / 2f; //lighten it up
        wyrds[ndx].visible = true; //show it
    }

    //remove all letters from bigLettersActive
    void ClearBigLettersActive()
    {
        testWord = "";  //clear the testWord
        foreach (Letter l in bigLettersActive)
        {
            bigLetters.Add(l);
            l.color = bigColorDim;
        }
        bigLettersActive.Clear();
        ArrangeBigLetters();
    }

    //add to the score for this word. int combo is the number of this word in a combo
    void Score(Wyrd wyrd, int combo)
    {
        //get the position of the first letter of the word
        Vector3 pt = wyrd.letters[0].transform.position;

        //create a List<> of Bezier points for the FloatingScore
        List<Vector3> pts = new List<Vector3>();

        //convert the point to a ViewportPoint(range across the screen from 0 to 1 and are used for GUI coordinates)
        pt = Camera.main.WorldToViewportPoint(pt);
        pt.z = 0;

        //make the Bezier points
        pts.Add(pt);
        pts.Add(scoreMidPoint);
        pts.Add(Scoreboard.S.transform.position);

        //set the value of floating score
        int value = wyrd.letters.Count * combo;
        FloatingScore fs = Scoreboard.S.CreateFloatingScore(value, pts);

        fs.timeDuration = 2f;
        fs.fontSizes = scoreFontSizes;

        //double the InOut easing effect
        fs.easingCurve = Easing.InOut + Easing.InOut;

        //make text of fs
        string txt = wyrd.letters.Count.ToString();
        if (combo > 1)
        {
            txt += " x " + combo;
        }
        fs.GetComponent<GUIText>().text = txt;
    }
}

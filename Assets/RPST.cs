using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPST : MonoBehaviour {

    public Sprite[] sprites = new Sprite[21];                                   // loading all the sprites
    public int[] contestantNumbers = new int[8];                                // declaring their corresponding numbers
    public SpriteRenderer[] presentSprites = new SpriteRenderer[9];             // declaring the active sprites
    public int[] semiFin = new int[4];                                          // semifinalist's numbers
    public int[] fin = new int[2];                                              // finalist's numbers
    public int win = new int();                                                 // winner's number
    public KMAudio audiohandler;                                                // audioHandler
    public AudioClip[] audio = new AudioClip[21];                               // declaring audio 
    public KMSelectable[] cylinders = new KMSelectable[8];                      // declaring selectable cylinders    
    public bool ModuleSolved;                                                   // to track if the module is solved
    public TextMesh[] quaterHoriz = new TextMesh[8];                            // 
    public TextMesh[] quaterVert = new TextMesh[8];                             //
    public TextMesh[] semifinals = new TextMesh[4];                             //
    public TextMesh[] finals = new TextMesh[2];                                 //
    public bool[] isAWinQuaterfinalists = new bool[8];                          //
    public bool[] isAWinSemifinalists = new bool[4];                            //
    public bool[] isAWinFinalists = new bool[2];                                //
    private double[] textScale = new[] { 8, 8, 10.8, 11 };                      //
    static int ModuleIdCounter = 1;                                             // onward: for logging
    int ModuleId;
    void Awake()
    {
        ModuleId = ModuleIdCounter++;
    }

    void Start () {

        for(int i = 0; i < 8; i++)
        {
            int i1 = i;
            cylinders[i1].OnInteract += delegate { pressCylinder(i1); return false; };
        }

        contestantNumbers = GenerateUniqueRandomArray();
        string contestantsForDebug = ArrayToString(contestantNumbers);
        Debug.LogFormat("[RPS Tournament #{0}] Generated contestants: {1}", ModuleId, contestantsForDebug);

        for (int i = 0; i < 8; i++)                                             // initializing sprites
        {
            presentSprites[i].sprite = sprites[contestantNumbers[i] - 1];
        }

        for (int i = 0; i < 8; i = i + 2)                                       // calculating semifinalists
        {
            semiFin[i / 2] = pvp(contestantNumbers[i], contestantNumbers[i+1]);
            if (semiFin[i / 2] == contestantNumbers[i])
            {
                isAWinQuaterfinalists[i] = true;
                isAWinQuaterfinalists[i + 1] = false;
            }
            else
            {
                isAWinQuaterfinalists[i] = false;
                isAWinQuaterfinalists[i + 1] = true;
            }
        }

        for (int i = 0; i < 4; i = i + 2)                                       // calculating finalists
        {
            fin[i / 2] = pvp(semiFin[i], semiFin[i + 1]);
            if (fin[i / 2] == semiFin[i])
            {
                isAWinSemifinalists[i] = true;
                isAWinSemifinalists[i + 1] = false;
            }
            else
            {
                isAWinSemifinalists[i] = false;
                isAWinSemifinalists[i + 1] = true;
            }
        }

        win = pvp(fin[0], fin[1]);                                              // winner
        if (win == fin[0])
        {
            isAWinFinalists[0] = true;
            isAWinFinalists[1] = false;
        }
        else
        {
            isAWinFinalists[0] = false;
            isAWinFinalists[1] = true;
        }

        Debug.LogFormat("[RPS Tournament #{0}] Semifinalists: {1}, {2}, {3}, {4}", ModuleId, semiFin[0], semiFin[1], semiFin[2], semiFin[3]);
        Debug.LogFormat("[RPS Tournament #{0}] Finalists: {1}, {2}", ModuleId, fin[0], fin[1]);
        Debug.LogFormat("[RPS Tournament #{0}] Winner: {1}", ModuleId, win);

        lineColouring(quaterHoriz, isAWinQuaterfinalists);
        lineColouring(quaterVert, isAWinQuaterfinalists);
        lineColouring(semifinals, isAWinSemifinalists);
        lineColouring(finals, isAWinFinalists);

    }

    public void pressCylinder(int cylinderNumber)
    {
        if (ModuleSolved)
            return;

        if(win == contestantNumbers[cylinderNumber])
        {
            onSolve(win);
        }
        else
        {
            onStrike(contestantNumbers[cylinderNumber]);
        }
    }

    public int pvp (int contestant1, int contestant2) // winner of a single game
    {
        if (((contestant2 - contestant1 < 11) && (contestant2 - contestant1 > 0)) || (contestant2 - contestant1 < -10))
            return contestant1;
        else
            return contestant2;
    }

    public static int[] GenerateUniqueRandomArray() //array with 8 unique numbers [1-21]
    {
        int[] result = new int[8];
        bool[] used = new bool[21];

        for (int i = 0; i < 8; i++)
        {
            int randomNumber;
            do
            {
                randomNumber = Random.Range(1, 22);
            }
            while (used[randomNumber - 1]);

            used[randomNumber - 1] = true;
            result[i] = randomNumber;
        }
        return result;
    }

    public string ArrayToString(int[] array)                                   //making a string with numbers from the array (for debug)
    {
        string[] stringArray = new string[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            stringArray[i] = array[i].ToString();
        }
        return string.Join(", ", stringArray);
    }

    public void lineColouring(TextMesh[] lines, bool[] isAWin)
    {
        for(int i = 0; i < lines.Length; i++)
        {
            lines[i].color = isAWin[i] ? Color.green : Color.red;
        }
    }

    public void onStrike(int pressedNumber)
    {
        GetComponent<KMBombModule>().HandleStrike();
        Debug.LogFormat("[RPS Tournament #{0}] Wrong! You pressed {1}", ModuleId, pressedNumber);
    }

    public void onSolve(int win)
    {
        presentSprites[8].sprite = sprites[win - 1];                           //initializing winner sprite upon solve
        audiohandler.PlaySoundAtTransform(audio[win - 1].name, transform);         //function to play correspong sound
        ModuleSolved = true;
        Debug.LogFormat("[RPS Tournament #{0}] Correctly pressed the winner ({1})", ModuleId, win);
        GetComponent<KMBombModule>().HandlePass();
        StartCoroutine(linesAnimation());
    }

    public IEnumerator linesAnimation()
    {
        double xScale = 0;
        while (xScale < textScale[0])
        {
            xScale += 0.5;
            for (int i = 0; i < 8; i++)
                quaterHoriz[i].transform.localScale = new Vector3((float)xScale, 1, 1);
            yield return new WaitForSeconds(0.01f);
        }

        xScale = 0;
        while (xScale < textScale[1])
        {
            xScale += 0.5;
            for (int i = 0; i < 8; i++)
                quaterVert[i].transform.localScale = new Vector3((float)xScale, 1, 1);
            yield return new WaitForSeconds(0.01f);
        }

        xScale = 0;
        while (xScale < textScale[2])
        {
            xScale += 0.5;
            for (int i = 0; i < 4; i++)
                semifinals[i].transform.localScale = new Vector3((float)xScale, 1, 1);
            yield return new WaitForSeconds(0.01f);
        }

        xScale = 0;
        while (xScale < textScale[3])
        {
            xScale += 0.5;
            for (int i = 0; i < 2; i++)
                finals[i].transform.localScale = new Vector3((float)xScale, 1, 1);
            yield return new WaitForSeconds(0.01f);
        }
        yield return null;
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} <button position> (1-8: TTL, TBL, TTR, TBR, BTL, BBL, BTR, BBR (T/B/L/R = top/bottom/left/right)>";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string Command)
    {
        if (!Command.RegexMatch("[12345678]"))
        {
            yield return "sendtochaterror Сommand is not valid.";
        }

        int answeredNumber = Command[0] - '1';
        cylinders[answeredNumber].OnInteract();
        yield return null;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
        onSolve(win);
        yield return null;
    }
}

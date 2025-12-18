using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class RPST : MonoBehaviour
{

    public KMAudio AudioHandler;                                                // audioHandler
    public AudioClip[] Audio = new AudioClip[21];                               // declaring audio 
    public KMSelectable[] cylinders = new KMSelectable[8];                      // declaring selectable cylinders    

    public Sprite[] sprites = new Sprite[21];                                   // loading all the sprites
    public SpriteRenderer[] presentSprites = new SpriteRenderer[9];             // declaring the active sprites

    public TextMesh[] quaterHoriz = new TextMesh[8];                            // 
    public TextMesh[] quaterVert = new TextMesh[8];                             //
    public TextMesh[] semifinals = new TextMesh[4];                             //
    public TextMesh[] finals = new TextMesh[2];                                 //

    private int[] contestantNumbers = new int[8];                                // declaring their corresponding numbers
    private int[] semiFin = new int[4];                                          // semifinalist's numbers
    private int[] fin = new int[2];                                              // finalist's numbers
    private int win = new int();                                                 // winner's number
    private bool ModuleSolved;                                                   // to track if the module is solved
    private bool[] isAWinQuaterfinalists = new bool[8];                          //
    private bool[] isAWinSemifinalists = new bool[4];                            //
    private bool[] isAWinFinalists = new bool[2];                                //
    private double[] textScale = new[] { 8, 8, 10.8, 11 };                      //
    private static int ModuleIdCounter = 1;                                             // onward: for logging
    private int ModuleId;

    void Awake()
    {
        ModuleId = ModuleIdCounter++;
    }

    void Start()
    {

        for (int i = 0; i < 8; i++)
        {
            int i1 = i;
            cylinders[i1].OnInteract += delegate { pressCylinder(i1); return false; };
        }

        contestantNumbers = GenerateUniqueRandomArray();
        Debug.LogFormat("[RPS Tournament #{0}] Generated contestants: {1}", ModuleId, contestantNumbers.Select(i => Audio[i - 1].name).Join(", "));

        for (int i = 0; i < 8; i++)                                             // initializing sprites
        {
            presentSprites[i].sprite = sprites[contestantNumbers[i] - 1];
        }

        for (int i = 0; i < 8; i = i + 2)                                       // calculating semifinalists
        {
            semiFin[i / 2] = pvp(contestantNumbers[i], contestantNumbers[i + 1]);
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

        Debug.LogFormat("[RPS Tournament #{0}] Semifinalists: {1}", ModuleId, semiFin.Select(i => Audio[i - 1].name).Join(", "));
        Debug.LogFormat("[RPS Tournament #{0}] Finalists: {1}", ModuleId, fin.Select(i => Audio[i - 1].name).Join(", "));
        Debug.LogFormat("[RPS Tournament #{0}] Winner: {1}", ModuleId, Audio[win - 1].name);

        lineColouring(quaterHoriz, isAWinQuaterfinalists);
        lineColouring(quaterVert, isAWinQuaterfinalists);
        lineColouring(semifinals, isAWinSemifinalists);
        lineColouring(finals, isAWinFinalists);

    }

    public void pressCylinder(int cylinderNumber)
    {
        if (ModuleSolved)
            return;

        if (win == contestantNumbers[cylinderNumber])
        {
            onSolve(win);
        }
        else
        {
            onStrike(contestantNumbers[cylinderNumber]);
        }
    }

    public int pvp(int contestant1, int contestant2) // winner of a single game
    {
        if (((contestant2 - contestant1 < 11) && (contestant2 - contestant1 > 0)) || (contestant2 - contestant1 < -10))
            return contestant1;
        else
            return contestant2;
    }

    public static int[] GenerateUniqueRandomArray() //array with 8 unique numbers [1-21]
    {
        return Enumerable.Range(1, 21).ToArray().Shuffle().Take(8).ToArray();
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
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i].color = isAWin[i] ? Color.green : Color.red;
        }
    }

    public void onStrike(int pressedNumber)
    {
        GetComponent<KMBombModule>().HandleStrike();
        Debug.LogFormat("[RPS Tournament #{0}] Wrong! You pressed {1}.", ModuleId, Audio[pressedNumber - 1].name);
    }

    public void onSolve(int win)
    {
        presentSprites[8].sprite = sprites[win - 1];                           //initializing winner sprite upon solve
        AudioHandler.PlaySoundAtTransform(Audio[win - 1].name, transform);         //function to play correspong sound
        ModuleSolved = true;
        Debug.LogFormat("[RPS Tournament #{0}] Correctly pressed the winner {1}.", ModuleId, Audio[win - 1].name);
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
    private readonly string TwitchHelpMessage = @"Use !{0} <button position> [Press the button at that position.] | Positions: TTL, TBL, TTR, TBR, BTL, BBL, BTR, BBR (Top/Bottom half, Top/Bottom Half, Left/Right Half)";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.Trim().ToUpperInvariant();
        var m = Regex.Match(command, @"^\s*(?<pos>ttl|tbl|ttr|tbr|btl|bbl|btr|bbr)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!m.Success)
            yield break;

        yield return null;
        var strs = new string[] { "TTL", "TBL", "TTR", "TBR", "BTL", "BBL", "BTR", "BBR" };
        int ix = Array.IndexOf(strs, m.Groups["pos"].Value);
        cylinders[ix].OnInteract();
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
        cylinders[Array.IndexOf(contestantNumbers, win)].OnInteract();
        yield break;
    }
}

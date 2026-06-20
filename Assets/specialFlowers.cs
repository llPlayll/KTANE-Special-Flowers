using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;
using KeepCoding;

public class specialFlowers : MonoBehaviour
{
    [SerializeField] private KMAudio Audio;
    [SerializeField] private VideoPlayer Video;

    [SerializeField] FlowerManager FlowerManager;
    [SerializeField] AudioClip SongClip;
    [SerializeField] AudioClip ShortSolveSoundClip;
    [SerializeField] AudioClip LongSolveSoundClip;
    [SerializeField] GameObject VideoPlane;
    [SerializeField] MeshRenderer VideoRenderer;

    private static VideoClip FetchedVideo;
    VideoClip SolveClip;

    public bool animating;
    public bool focused;
    public bool tpActive;

    // For the Lenient Sequence Generation.
    Dictionary<int, int[]> ValidNextFlowers = new Dictionary<int, int[]>
    {
        { 1,  new int[] { 2, 3, 6, 7, 10, 11, 12 } },
        { 2,  new int[] { 1, 3, 4, 6, 7, 10, 11, 12 } },
        { 3,  new int[] { 1, 2, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 } },
        { 4,  new int[] { 2, 3, 5, 7, 8, 9, 12, 13 } },
        { 5,  new int[] { 3, 4, 7, 8, 9, 12, 13, 14 } },
        { 6,  new int[] { 1, 2, 3, 7, 8, 10, 11, 12, 13, 15 } },
        { 7,  new int[] { 1, 2, 3, 4, 5, 6, 8, 9, 10, 11, 12, 13 } },
        { 8,  new int[] { 3, 4, 5, 6, 7, 9, 11, 12, 13, 14, 16 } },
        { 9,  new int[] { 3, 4, 5, 7, 8, } },
        { 10, new int[] { 1, 2, 3, 6, 7, 11, 12, 15, 16, 18, 19 } },
        { 11, new int[] { 1, 2, 3, 6, 7, 8, 10, 12, 13, 15, 16, 18, 19 } },
        { 12, new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 10, 11, 13, 14, 15, 16, 17 } },
        { 13, new int[] { 3, 4, 5, 6, 7, 8, 11, 12, 14, 16, 17, 19, 20 } },
        { 14, new int[] { 3, 5, 8, 12, 13, 16, 17, 19, 20 } },
        { 15, new int[] { 6, 10, 11, 12, 16, 18, 19 } },
        { 16, new int[] { 8, 10, 11, 12, 13, 14, 15, 17, 18, 19, 20 } },
        { 17, new int[] { 12, 13, 14, 16, 19, 20 } },
        { 18, new int[] { 10, 11, 15, 16, 19 } },
        { 19, new int[] { 10, 11, 13, 14, 15, 16, 17, 18, 20 } },
        { 20, new int[] { 13, 14, 16, 17, 19 } },
    };
    List<int[]> PresetValidSequences = new List<int[]>
    {
        new int[] { 19, 15, 10, 3, 4, 7, 6, 12, 2, 1, 9, 5, 13, 20, 14, 8, 11, 18, 16, 17 },
        new int[] { 5, 3, 1, 2, 7, 12, 10, 11, 18, 19, 9, 4, 8, 13, 20, 6, 15, 16, 17, 14 },
        new int[] { 15, 19, 18, 10, 1, 20, 14, 13, 12, 5, 17, 16, 11, 6, 8, 7, 4, 2, 3, 9 },
        new int[] { 12, 2, 1, 6, 3, 14, 17, 19, 20, 13, 7, 8, 4, 9, 5, 15, 11, 10, 16, 18 },
        new int[] { 1, 2, 4, 5, 9, 12, 14, 8, 7, 6, 19, 13, 3, 11, 18, 15, 10, 16, 20, 17 },
        new int[] { 9, 8, 13, 3, 11, 16, 15, 6, 1, 7, 17, 12, 10, 2, 4, 5, 14, 20, 19, 18 },
        new int[] { 1, 11, 3, 6, 2, 17, 14, 19, 15, 12, 4, 9, 8, 13, 5, 7, 10, 18, 16, 20 },
        new int[] { 1, 12, 15, 16, 13, 8, 9, 4, 6, 11, 2, 3, 5, 14, 20, 7, 10, 18, 19, 17 },
        new int[] { 10, 1, 12, 6, 13, 20, 17, 16, 15, 11, 7, 4, 8, 5, 9, 2, 3, 14, 19, 18 },
        new int[] { 11, 1, 10, 12, 16, 3, 9, 8, 14, 20, 17, 13, 19, 15, 18, 4, 5, 7, 6, 2 }
    };

    public int[] sequence;
    public bool sequencePlayedOnce;

    public int[] TPInput;
    public bool TPShouldCheckInput;

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;

    SpecialFlowersSettings Settings = new SpecialFlowersSettings();

    void Awake()
    {
        ModuleId = ModuleIdCounter++;

        ModConfig<SpecialFlowersSettings> modConfig = new ModConfig<SpecialFlowersSettings>("SpecialFlowersSettings");
        Settings = modConfig.Settings;
        modConfig.Settings = Settings;

        GetComponent<KMSelectable>().OnFocus += delegate () { focused = true; if (!animating && !sequencePlayedOnce && !tpActive) StartCoroutine("PlaySequence"); };
        GetComponent<KMSelectable>().OnDefocus += delegate () { focused = false; };
        GetComponent<KMBombModule>().OnActivate += delegate () { tpActive = TwitchPlaysActive; };
        if (Application.isEditor) focused = true;

        if (!Application.isEditor && !FetchedVideo) FetchedVideo = PathManager.GetAssets<VideoClip>("specialflowersvideo").Single();
        if (!Application.isEditor)
        {
            SolveClip = FetchedVideo;
            Video.clip = SolveClip;
            Video.Prepare();
        }
    }

    void Start()
    {
        if (Settings.nonLenientGeneration)
        {
            sequence = Enumerable.Range(1, 20).ToArray();
            sequence = sequence.Shuffle();
            Log("The mod setting for non-lenient sequence generation is on - the module will generate a completely random sequence of flowers.");
        }
        else
        {
            bool generatedSuccesfully = true;
            for (int a = 0; a < 10000; a++)
            {
                generatedSuccesfully = true;
                sequence = new int[20];
                List<int> unused = Enumerable.Range(1, 20).ToList();
                for (int i = 0; i < 20; i++)
                {
                    int[] valid = i % 5 == 0 ? Enumerable.Range(1, 20).ToArray() : ValidNextFlowers[sequence[i - 1]];
                    valid = valid.Shuffle();
                    bool addedNum = false;
                    foreach (int n in valid)
                    {
                        if (unused.Contains(n))
                        {
                            sequence[i] = n;
                            unused.Remove(n);
                            addedNum = true;
                            break;
                        }
                    }
                    if (!addedNum)
                    {
                        generatedSuccesfully = false;
                        break;
                    }
                }
                if (generatedSuccesfully) break;
            }
            if (!generatedSuccesfully)
            {
                Log("(Note the module took too long to generate a valid sequence of flowers and resorted to using one of the preset valid sequences.)");
                sequence = PresetValidSequences.PickRandom();
            }
        }
        Log($"Generated sequence of flower appearances (referred to by their labels) is: {sequence.Join(", ")}.");
    }

    void Update()
    {
        if (sequencePlayedOnce && focused && !animating)
        {
            if (!animating)
            {
                if (Input.inputString.Contains('P') || Input.inputString.Contains('p')) StartCoroutine("PlaySequence");
                else if (Input.inputString.Contains('L') || Input.inputString.Contains('l')) StartCoroutine("ShowLabels");
            }
        }
    }

    public void SolveModule()
    {
        Log($"All 20 flowers were hovered in a row. Module solved!");
        StartCoroutine("SolveAnim");
    }

    void Log(object arg)
    {
        Debug.Log($"[Special Flowers #{ModuleId}] {arg}");
    }

    IEnumerator PlaySequence()
    {
        yield return null;
        animating = true;
        Audio.PlaySoundAtTransform(SongClip.name, transform);
        FlowerManager.StartCoroutine("Sequence", sequence);
    }

    IEnumerator ShowLabels()
    {
        yield return null;
        animating = true;
        FlowerManager.StartCoroutine("ShowLabels");
    }

    IEnumerator SolveAnim()
    {
        StartCoroutine("PlayVideo");
        Audio.PlaySoundAtTransform((Settings.shortSolveSound ? ShortSolveSoundClip : LongSolveSoundClip).name, transform);
        for (float t = 0; t < 1f; t += Time.deltaTime / (60 / 145.35f)) yield return null;
        GetComponent<KMBombModule>().HandlePass();
        ModuleSolved = true;
    }

    IEnumerator PlayVideo()
    {
        for (float t = 0; t < 1f; t += Time.deltaTime / (60 / 145.35f - 0.1f)) yield return null;
        Video.Play();
        for (float t = 0; t < 1f; t += Time.deltaTime / 0.1f) yield return null;
        VideoPlane.SetActive(true);
        VideoRenderer.material.color = Color.white;
        for (float t = 0; t < 1f; t += Time.deltaTime / 11f) yield return null;
        for (float t = 0; t < 1f; t += Time.deltaTime / 2f)
        {
            VideoRenderer.material.color = Color.Lerp(Color.white, Color.black, t);
            yield return null;
        }
        VideoPlane.SetActive(false);
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use <!{0} play> to play (or replay) the flower sequence, and <!{0} playfocus> to keep the module focused while the sequence is playing. | "
                                                + "Use <!{0} labels> to temporarily show the labels of the flowers, and <!{0} labelsfocus> to keep the module focused while the labels are being shown. | "
                                                + "Use <!{0} submit [20 numbers]> to play the sequence, then hover over the 20 specified flowers (referred to by their labels) as they appear.";
    private bool TwitchPlaysActive;
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string Command)
    {
		var commandArgs = Command.ToUpperInvariant().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
        if (commandArgs.Length < 1) yield return "sendtochaterror!h Empty command!";
        else
        {
            switch (commandArgs[0])
            {
                case "PLAY":
                case "PLAYFOCUS":
                    while (animating) yield return null;
                    yield return null;
                    TPShouldCheckInput = false;
                    StartCoroutine("PlaySequence");
                    yield return null;
                    if (commandArgs[0] == "PLAYFOCUS") while (animating) yield return null;
                    break;
                case "LABELS":
                case "LABELSFOCUS":
                    while (animating) yield return null;
                    yield return null;
                    StartCoroutine("ShowLabels");
                    yield return null;
                    if (commandArgs[0] == "LABELSFOCUS") while (animating) yield return null;
                    break;
                case "SUBMIT":
                    if (!sequencePlayedOnce) yield return $"sendtochaterror!h Unable to submit the flower sequence without fully playing it at least once (which, ???).";
                    if (commandArgs.Length != 21) yield return $"sendtochaterror!h Invalid number of flowers ({commandArgs.Length - 1}) specified!";
                    else
                    {
                        TPInput = new int[20];
                        for (int i = 0; i < 20; i++)
                        {
                            int f;
                            if (int.TryParse(commandArgs[i + 1], out f))
                            {
                                if (1 <= f && f <= 20 && !TPInput.Contains(f)) TPInput[i] = f;
                                else
                                {
                                    if (TPInput.Contains(f)) yield return $"sendtochaterror!h Duplicate flower ({commandArgs[i + 1]}) specified!";
                                    else yield return $"sendtochaterror!h Invalid flower ({commandArgs[i + 1]}) specified!";
                                }

                            }
                            else yield return $"sendtochaterror!h Invalid flower ({commandArgs[i + 1]}) specified!";
                        }
                        yield return null;
                        if (sequence.SequenceEqual(TPInput)) yield return "solve";
                        while (animating) yield return null;
                        TPShouldCheckInput = true;
                        StartCoroutine("PlaySequence");
                    }
                    break;
                default:
                    yield return $"sendtochaterror!h Invalid command (\"{commandArgs[0]}\")!";
                    break;
            }
        }
        yield return null;
    }

    void TwitchHandleForcedSolve()
    {
        StartCoroutine("TPForcedSolve");
    }

    IEnumerator TPForcedSolve()
    {
        yield return null;
        sequencePlayedOnce = true;
        TPInput = sequence;
        TPShouldCheckInput = true;
        while (animating) yield return null;
        StartCoroutine("PlaySequence");
    }

    class SpecialFlowersSettings
    {
        public bool shortSolveSound = false;
        public bool nonLenientGeneration = false;
    }

    static Dictionary<string, object>[] TweaksEditorSettings = new Dictionary<string, object>[]
    {
        new Dictionary<string, object>
        {
            { "Filename", "SpecialFlowersSettings.json" },
            { "Name", "Special Flowers Settings" },
            { "Listings", new List<Dictionary<string, object>>{
                new Dictionary<string, object>
                {
                    { "Key", "shortSolveSound" },
                    { "Text", "Short Solve Sound" },
                    { "Description", "Makes the module use a shorter version of its solve sound." }
                },
                new Dictionary<string, object>
                {
                    { "Key", "nonLenientGeneration" },
                    { "Text", "Non-Lenient Sequence Generation" },
                    { "Description", "Makes the module generate sequences in a completely random way.\n" +
                        "Refer to the Steam Workshop item description for a more in-depth explanation\nof this setting." }
                },
            } }
        }
    };
}

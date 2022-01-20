using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography; // ��������
using UnityEngine;
using UnityEngine.InputSystem;

using TMPro; // �X�N���v�g���� TextMeshPro �̕ύX

public class GameMainManager : MonoBehaviour
{
    // �Q�[����ԕϐ�
    enum GameState
    {
        Waiting,
        Countdown,
        TrialOn,
        Completed,
        Canceled,
        Failed
    }
    GameState gameState = GameState.Waiting;

    // ScriptableObject
    [SerializeField]
    KeyBind keyBind;
    KeyBindDicts dicts;

    // �����ƂȂ�V�[�����̃}�l�W���[�B
    TweeterManager tweeter;

    // �ȈՐݒ�I�u�W�F�N�g
    public GameObject MissLimitInput;

    // �ۑ蕶���\���p�v���n�u
    public GameObject AssignedCharTMPPrefab;
    GameObject[] assignedCharTMPs;

    // ���\���I�u�W�F�N�g
    public GameObject TotalTimeTMP;
    public GameObject TotalMissTMP;
    public GameObject[] LapTimeTMP;
    public GameObject[] LapMissTMP;
    public GameObject TotalCPSTMP;
    TextMeshProUGUI countDownTMPUGUI;


    // �g���C�A�����̃f�[�^�ۊ�
    TrialData nowTrialData;

    // nowTrialData.AllInputIDs �p�̒萔
    const ushort shiftdownInputID = 97;
    const ushort shiftupInputID = 98;

    // ���������{�ݒ�

    // �A�T�C�����ꂽ�Ō��p�^�[���̎�ސ� 26*2 + 22*2 + Space
    public const ushort NumOfKeyPatterns = 97;
    // �����퐔�i�����Ƀo�C���h����Ȃ��Ō��p�^�[���� 2 ����̂� - 2 �j
    public const ushort NumOfChars = NumOfKeyPatterns - 2;

    // Config�i�~�X�����Ȃǁj
    Config config = new Config();

    // �ۑ蕶���̍�����W
    // �A���J�[�� (0, 1) �܂� �e�I�u�W�F�N�g Assignment �̍��ォ��̑��΋����Ŏw��
    const int displayInitX = 12;
    const int displayInitY = -10;

    // �ۑ蕶���� Text Mesh Pro �\�����������Y������
    const int displayCharXdiff = 18;
    const int displayCharYdiff = -41;

    // TrialData �ɓn���Q�[�����[�h�ϐ��ƁA���ꂪ�����Ӗ�
    const int gameMode = 0;
    const int assignmentLength = 360;
    const int lapLength = 36;
    const int numOfLaps = 10;

    // �X�g�b�v�E�H�b�`
    System.Diagnostics.Stopwatch myStopwatch;

    void Awake()
    {
        keyBind = new KeyBind();
        keyBind.LoadFromJson(0);
        dicts = new KeyBindDicts(keyBind);

        // ������������
        tweeter = GameObject.Find("Tweeter").GetComponent<TweeterManager>();

        // ���䂷�ׂ��I�u�W�F�N�g�Ȃǂ̎擾�Ə�����
        countDownTMPUGUI = GameObject.Find("CountDownTMP").GetComponent<TextMeshProUGUI>();
        countDownTMPUGUI.text = "";

        // �\������
        tweeter.SetVisible(false);

        // Config �̓ǂݍ��݂ƕ\���ւ̔��f
        config.Load();
        GameObject.Find("MissLimiterInput").GetComponent<TMP_InputField>().text = config.MissLimit.ToString();

        myStopwatch = new System.Diagnostics.Stopwatch();

        // EventBus �ɃL�[�����C�x���g�������̃��\�b�h���s���˗�
        EventBus.Instance.SubscribeNormalKeyDown(OnNormalKeyDown);
        EventBus.Instance.SubscribeReturnKeyDown(OnGameStartButtonClick);
        EventBus.Instance.SubscribeEscKeyDown(OnBackButtonClick);
        EventBus.Instance.SubscribeShiftKeyDown(ShiftKeyDownHandler);
        EventBus.Instance.SubscribeShiftKeyUp(ShiftKeyUpHandler);
    }
    // Start is called before the first frame update
    void Start()
    {
    }
    // Update is called once per frame
    void Update()
    {
        // Completed, Canceled �ł� TrialInfo �͕K�v�����A(�L�����Z��|����) �ɕ`�悵�Ă��邩�炱���ł͕s�v
        if (gameState == GameState.Countdown)
        {
            long ms = myStopwatch.ElapsedMilliseconds;
            if (ms >= 3000) {
                countDownTMPUGUI.text = "";
                StartTrial();
            }
            else if (ms >= 2000)
            {
                countDownTMPUGUI.text = "1";
            }
            else if (ms >= 1000)
            {
                countDownTMPUGUI.text = "2";
            }
            else countDownTMPUGUI.text = "3";
        }
        else if (gameState == GameState.TrialOn) UpdateTrialInfo();
    }
    void OnDestroy()
    {
        EventBus.Instance.UnsubscribeNormalKeyDown(OnNormalKeyDown);
        EventBus.Instance.UnsubscribeReturnKeyDown(OnGameStartButtonClick);
        EventBus.Instance.UnsubscribeEscKeyDown(OnBackButtonClick);
        EventBus.Instance.UnsubscribeShiftKeyDown(ShiftKeyDownHandler);
        EventBus.Instance.UnsubscribeShiftKeyUp(ShiftKeyUpHandler);
    }

    // �����Ƃ̂����
    public System.Tuple<string, TrialData> GetTrialData()
    {
        if (gameState == GameState.TrialOn || gameState == GameState.Waiting)
            return new System.Tuple<string, TrialData>(gameState.ToString(), null);
        else
            return new System.Tuple<string, TrialData>(gameState.ToString(), nowTrialData);
    }

    // ���[�e�B���e�B���\�b�h
    /// <summary>
    /// long �ŕۑ����Ă���~���b���A�\���p�� X.XXX s �ɕς���
    /// </summary>
    /// <param name="ms"></param>
    /// <returns></returns>
    string ToFormattedTime(long ms)
    {
        return (ms / 1000).ToString() + "." + (ms % 1000).ToString("000");
    }

    // ��Ԑ��䃁�\�b�h
    void StartCountdown()
    {
        gameState = GameState.Countdown;

        // �J�E���g�_�E���X�^�[�g
        myStopwatch.Restart();

        // �ȈՐݒ蒆�͓������Ȃ� ���V�t�g���݂̃o�O�������ɖ����Ă邩��
        if (MissLimitInput.GetComponent<TMP_InputField>().isFocused) return;

        // �ȈՐݒ���~
        MissLimitInput.GetComponent<TMP_InputField>().readOnly = true;

        // ��ʕ\���̏�����
        TotalTimeTMP.GetComponent<TextMeshProUGUI>().text = $"0.000";
        TotalMissTMP.GetComponent<TextMeshProUGUI>().text = $"0";
        TotalCPSTMP.GetComponent<TextMeshProUGUI>().text = $"0.000";
        if (assignedCharTMPs != null)
            foreach (GameObject obj in assignedCharTMPs) Destroy(obj);
        tweeter.SetVisible(false);


        // �{�^���̕\���ύX
        GameObject.Find("StartButton").GetComponent<UnityEngine.UI.Button>().interactable = false;
        GameObject.Find("BackButtonTMP").GetComponent<TextMeshProUGUI>().text = "Cancel Trial [Esc]";

        // �ݒ�̍ēǂݍ��݁i�ȈՐݒ�ōX�V���Ă���ꍇ������̂ŁA�����ōēx�ǂݍ��ށj
        config.Load();

        // nowTrialData �̏�����
        nowTrialData = new TrialData(gameMode, config.MissLimit, keyBind);

        // �����𐶐����� nowTrialData �ɃZ�b�g
        RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        byte[] Seeds = new byte[assignmentLength];
        rng.GetBytes(Seeds);

        for (int i = 0; i < assignmentLength; i++)
        {
            System.Random rnd = new System.Random(Seeds[i]);
            ushort rnd_charID = (ushort)(rnd.Next(0, NumOfChars)); // 0 ~ 94

            // Null �������܂������� CharID �� ++ ����K�v������
            if (rnd_charID >= keyBind.NullKeyMap[0]) rnd_charID++;
            if (rnd_charID >= keyBind.NullKeyMap[1]) rnd_charID++;

            nowTrialData.TaskCharIDs[i] = rnd_charID;
        }

        // RNGCryptoServiceProvider �� IDisposable �C���^�[�t�F�C�X���������Ă��āA�g�p������������^��j�����Ȃ��Ⴂ���Ȃ��炵���i�����h�L�������g���j
        // IDisposable �C���^�[�t�F�C�X�ɂ� Dispose() �Ƃ������\�b�h��������������Ă���
        // ������p������N���X�́A�u���\�[�X���������ł邩��g���I�������iGC ��҂����jDispose() �Ŕj���������������v�ƍl����΂������낤
        rng.Dispose();
    }
    /// <summary>
    /// �g���C�A���J�n���\�b�h
    /// �J�E���g�_�E���I������ Update() ����Ăяo�����
    /// </summary>
    void StartTrial()
    {
        gameState = GameState.TrialOn;

        // TrialData.AllInputIDs[0] �ɏ����V�t�g��Ԃ��i�[
        nowTrialData.AllInputIDs.Add(MyInputManager.GetShiftState());
        nowTrialData.AllInputTime.Add(0);

        // �ۑ蕶���̕`��
        assignedCharTMPs = new GameObject[assignmentLength];

        // Assignment �I�u�W�F�N�g�̎q�Ƃ��ĕ`�悷�邽��
        GameObject asg = GameObject.Find("Assignment");
        assignedCharTMPs = new GameObject[assignmentLength];

        for (int i = 0; i < assignmentLength; i++)
        {
            assignedCharTMPs[i] = Instantiate(AssignedCharTMPPrefab, asg.transform);
            TextMeshProUGUI assignedCharTMPUGUI = assignedCharTMPs[i].GetComponent<TextMeshProUGUI>();
            assignedCharTMPUGUI.text = dicts.ToChar_FromCharID(nowTrialData.TaskCharIDs[i]).ToString();

            // �\���ꏊ�̎w��
            assignedCharTMPs[i].GetComponent<RectTransform>().localPosition = new Vector3(displayInitX + displayCharXdiff * (i % lapLength), displayInitY + displayCharYdiff * (i / lapLength), 0);
            //assignedCharTMPs[i].GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        }

        // �������I����Ă���Q�[���J�n���X�g�b�v�E�H�b�`���J�n
        myStopwatch.Restart();

        UpdateTrialInfo();
    }
    void CompleteLap()
    {
        int lap = nowTrialData.TypedKeys / lapLength;
        // nowTrialData �ւ̃��b�v�^�C���̓���
        // ���񏈗��̊֌W�ł����������� KeyTime ���Y����̂�������Ȃ��̂ŁA�����ł� .TotalTime ���g���Ă��Ȃ�
        nowTrialData.SetLapTime(lap, nowTrialData.CorrectKeyTime[nowTrialData.TypedKeys]);
        // nowTrialData �ւ̃��b�v�~�X�̓���
        nowTrialData.LapMiss[lap] = nowTrialData.TotalMiss;

        // �ŏI���b�v�łȂ����̂݁A���b�v�ԍ����X�V�i���݃��b�v�����E�O�ɏo�Ă��܂����Ƃ�h���j
        // �܂��A���̃��b�v���O�̃~�X���Ɠ����ɂ���i 0 �̂܂܂��ƕ\���������j
        if (lap < numOfLaps)
            nowTrialData.LapMiss[lap+1] = nowTrialData.LapMiss[lap];
    }
    /// <summary>
    /// �g���C�A���������ɌŗL�̏������s���B�g���C�A���I�����̋��ʏ����� OnEndTrial()
    /// </summary>
    void CompleteTrial()
    {
        // �X�g�b�v�E�H�b�`���~�߂邪�A�����̎��Ԃ͂��͂�֌W�����i�L�[�Ō����Ɍv�����Ă��邽�߁j
        myStopwatch.Stop();

        // nowTrialData �̍X�V
        nowTrialData.IsTerminated = false;

        Debug.Assert(gameState == GameState.TrialOn);
        gameState = GameState.Completed;

        // �I�������ʏ����̌Ăяo��
        OnEndTrial();
    }
    /// <summary>
    /// �g���C�A�����f���ɌŗL�̏������s���B�g���C�A���I�����̋��ʏ����� OnEndTrial()
    /// �~�X�����𒴂���� -TotalTime Failed, Esc -> Canceled
    /// </summary>
    void CancelTrial()
    {
        // �X�g�b�v�E�H�b�`���~�߂邪�A�����̎��Ԃ͂��͂�֌W�����iOnNormalKeyDwn() �Ōv�����Ă��邽�߁j
        myStopwatch.Stop();

        // nowTrialData �̍X�V
        nowTrialData.IsTerminated = true;

        // TrialData �ɍŏI���b�v�^�C�����Z�b�g
        int lap = nowTrialData.TypedKeys / lapLength + 1;
        nowTrialData.SetLapTime(lap, nowTrialData.TotalTime);

        Debug.Assert(gameState == GameState.TrialOn || gameState == GameState.Countdown);
        if (nowTrialData.TotalMiss <= nowTrialData.MissLimit) gameState = GameState.Canceled;
        else gameState = GameState.Failed;

        // �I�������ʏ����̌Ăяo��
        OnEndTrial();
    }
    /// <summary>
    /// CompleteTrial() �� CancelTrial() ����Ăяo���A�g���C�A���I�����̋��ʏ���
    /// </summary>
    void OnEndTrial()
    {
        // nowTrialData �̐ݒ�
        nowTrialData.DateTimeWhenFinished = System.DateTime.Now;

        // �f�[�^�̕ۑ�
        nowTrialData.SaveLog();

        // �g���C�A�����̕`��i�g���C�A�����ƌv���̎d�����Ⴄ���߁A�I��������ɌĂяo���j
        UpdateTrialInfo();

        // �{�^���̕\���ύX
        GameObject.Find("StartButton").GetComponent<UnityEngine.UI.Button>().interactable = true;
        GameObject.Find("BackButtonTMP").GetComponent<TextMeshProUGUI>().text = "Back To Title [Esc]";

        // �ȈՐݒ���ċN��
        MissLimitInput.GetComponent<TMP_InputField>().readOnly = false;
    }

    // �\������

    // �g���C�A�� Information �`�惁�\�b�h
    // �����ł� nowTrialData �ɋL�^�ς݂̃f�[�^��`�悷�邾���Ȃ̂ŁA�X�V�͊e�C�x���g���ɍς܂���K�v����
    void UpdateTrialInfo()
    {
        Debug.Assert(gameState != GameState.Waiting);

        int lap = nowTrialData.TypedKeys / lapLength + 1;
        // �g���C�A���������̂� lap > numOfLaps �ƂȂ��Ă��܂��̂ŁA����������
        if (lap > numOfLaps) lap = numOfLaps;

        long totalTime;
        // �Q�[�����ł������ꍇ�A���A���Ȍo�ߎ��Ԃ��g���Čv�Z
        if (gameState == GameState.TrialOn) totalTime = myStopwatch.ElapsedMilliseconds;
        // �Q�[��(����|�L�����Z��)��ł������ꍇ�A�Ō�ɃL�[��Ō��������Ԃ��g���Čv�Z
        else totalTime = nowTrialData.TotalTime;
        int keys = nowTrialData.TypedKeys;
        double cps = (double)(keys * 1000) / totalTime;

        // �e�L�X�g�X�V�p�̎g���̂Ċ֐�
        void _UpdateText(GameObject obj, string str) => obj.GetComponent<TextMeshProUGUI>().text = str;

        // �g�[�^���^�C���̕\��
        _UpdateText(TotalTimeTMP, ToFormattedTime(totalTime));
        // �g�[�^���~�X�̕\��
        _UpdateText(TotalMissTMP, $"{nowTrialData.TotalMiss}");
        // �g�[�^�� CPS �̕\��
        _UpdateText(TotalCPSTMP, $"{cps:F3}");
        // ���b�v�^�C���̕\��
        // 1 ~ lap-1 �܂ł� nowTrialData �ɏ������ݍς݁i1-indexed ���Ӂj
        long[] singleLapTime = new long[lap + 1];
        singleLapTime[0] = 0;
        for (int i = 1; i <= lap-1; i++)
        {
            singleLapTime[i] = nowTrialData.GetSingleLapTime(i);
        }
        // ���݃��b�v�� time ���g��
        // time : �Q�[�����Ȃ烊�A���ȑŌ����ԁAterminated �Ȃ�ŏI�L�[�Ō����Ԃ������Ă���
        singleLapTime[lap] = totalTime - nowTrialData.GetLapTime(lap-1);
        // LapTimeTMP [] �� 0-indexed �ł��邱�Ƃɂ����� -> [(i|lap)-1] �ŃA�N�Z�X
        for (int i = 1; i <= lap; i++)
        {
            _UpdateText(LapTimeTMP[i - 1], ToFormattedTime(singleLapTime[i]));
        }
        // ���b�v�~�X�̕\��
        for (int i = 1; i <= lap; i++)
        {
            int iLapMiss = nowTrialData.LapMiss[i] - nowTrialData.LapMiss[i - 1];
            _UpdateText(LapMissTMP[i - 1], iLapMiss == 0 ? "" : iLapMiss.ToString());
        }
        // �I�����Ă��Ȃ����b�v�� �� �Ŗ��߂�
        for (int i = lap + 1; i <= numOfLaps; i++)
        {
            _UpdateText(LapTimeTMP[i - 1], "");
            _UpdateText(LapMissTMP[i - 1], "");
        }

    }

    // �g���C�A������
    void OnCorrectKeyDown()
    {
        assignedCharTMPs[nowTrialData.TypedKeys].GetComponent<TextMeshProUGUI>().color = new UnityEngine.Color(0.25f, 0.15f, 0.15f, 0.1f);

        // nowTrialData �̍X�V����
        // �����Ńg�[�^���^�C���͓��͂��Ȃ��iOnNormalButtonClick() �œ��͂��Ă��邽�߁j
        nowTrialData.TypedKeys++;
        nowTrialData.CorrectKeyTime[nowTrialData.TypedKeys] = nowTrialData.TotalTime;

        // ���b�v����
        if (nowTrialData.TypedKeys % lapLength == 0) CompleteLap();
        // �g���C�A������
        if (nowTrialData.TypedKeys == assignmentLength) CompleteTrial();
    }
    void OnIncorrectKeyDown()
    {
        // �����Ńg�[�^���^�C���͓��͂��Ȃ��iOnNormalButtonClick() �œ��͂��Ă��邽�߁j

        // nowTrialData �Ƀg�[�^���~�X�̓���
        nowTrialData.TotalMiss++;
        // nowTrialData �Ƀ��b�v�~�X�̓���
        int lap = nowTrialData.TypedKeys / lapLength + 1;
        nowTrialData.LapMiss[lap] = nowTrialData.TotalMiss;

        if (nowTrialData.TotalMiss > nowTrialData.MissLimit)
        {
            Debug.Log($"�~�X���������܂��B�ŏ������蒼���Ă��������B");
            CancelTrial();
        }
    }

    // �C�x���g�n���h��
    // EventBus �ɓn���Ď��s���Ă��炤
    // OnBackButtonClick() �ł͎��Ԃ𑪂�Ȃ�
    // �L�����Z���iEsc�j���� TotalTime �� �L�����Z������ł͂Ȃ��A�Ō�̐���Ō�or�~�X�Ō������Ƃ邽�߁B
    public void OnBackButtonClick()
    {
        if (gameState == GameState.TrialOn || gameState == GameState.Countdown) CancelTrial();
        //else if (isCompleted) ;
        else MySceneManager.ChangeSceneRequest("TitleScene");
    }
    public void OnGameStartButtonClick()
    {
        if (gameState == GameState.TrialOn || gameState == GameState.Countdown) return;
        else StartCountdown();
    }
    public void OnTweeterButtonClick()
    {
        // �g���C�A�����ɂ킴�킴�}�E�X�G�邮�炢������A Space ����Ȃ��Ē��Ń{�^�����N���b�N�����ꍇ�́A�g���C�A�����ł��\�������Ă���������
        // if (gameState == GameState.Completed ||  gameState == GameState.Canceled ||  gameState == GameState.Failed)
        tweeter.ToggleVisible();
    }
    // OnNormalKeyDown() �Ŏ��Ԃ��v��
    // �g���C�A�����̏��\���� Update() ���Ń^�C�}�[���~�߂ĎG�ɑ���΂������A���b�v�E�g���C�A���������̎��Ԍv���i�L�[�����ɂ����������ԁj�͐��m�ɂƂ�K�v�����邽�߁B
    void OnNormalKeyDown(ushort charID)
    {
        // �Q�[���O
        if (gameState == GameState.Completed || gameState == GameState.Canceled || gameState == GameState.Failed) {
            if (charID == 0) // default: Space
                OnTweeterButtonClick();
            else return;
        }
        else if (gameState == GameState.TrialOn)
        {
            nowTrialData.TotalTime = myStopwatch.ElapsedMilliseconds;
            // nowTrialData �̍X�V
            nowTrialData.AllInputIDs.Add(charID);
            nowTrialData.AllInputTime.Add(nowTrialData.TotalTime);
            // ���Ō�
            if (nowTrialData.TaskCharIDs[nowTrialData.TypedKeys] == charID)
            {
                OnCorrectKeyDown();
            }
            // �Q�[�����Ō�Ō�
            else
            {
                OnIncorrectKeyDown();
            }
        }
        // if (Countdown || Waiting), do nothing
    }
    /// <summary>
    /// �V�t�g��Ԃ����ɓ����镶���𔻒肷�鏈���� MyInputManager ���s���Ă��邪�AGameMainManager �ł̓V�t�g�̏㉺���� TrialData �ɏ������ނ��߁A���̃n���h����v����B
    /// </summary>
    void ShiftKeyDownHandler()
    {
        if (gameState == GameState.TrialOn)
        {
            nowTrialData.AllInputIDs.Add(shiftdownInputID);
            nowTrialData.AllInputTime.Add(myStopwatch.ElapsedMilliseconds);
        }
    }
    /// <summary>
    /// �V�t�g��Ԃ����ɓ����镶���𔻒肷�鏈���� MyInputManager ���s���Ă��邪�AGameMainManager �ł̓V�t�g�̏㉺���� TrialData �ɏ������ނ��߁A���̃n���h����v����B
    /// </summary>
    void ShiftKeyUpHandler()
    {
        if (gameState == GameState.TrialOn)
        {
            nowTrialData.AllInputIDs.Add(shiftupInputID);
            nowTrialData.AllInputTime.Add(myStopwatch.ElapsedMilliseconds);
        }
    }

}

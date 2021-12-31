using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography; // ��������
using UnityEngine;
using UnityEngine.InputSystem;

using System.Linq; // �z�񏉊����p
using TMPro; // �X�N���v�g���� TextMeshPro �̕ύX


public class GameMainManager : MonoBehaviour
{
    // �Q�[����ԕϐ�
    enum GameState
    {
        Waiting,
        TrialOn,
        Completed,
        Canceled
    }
    GameState gameState = GameState.Waiting;

    // �ۑ蕶���\���p�v���n�u
    public GameObject AssignedCharTMPPrefab;
    GameObject[] assignedCharTMPs;

    // ���\���I�u�W�F�N�g
    public GameObject TotalTimeTMP;
    public GameObject TotalMissTMP;
    public GameObject[] LapTimeTMP;
    public GameObject[] LapMissTMP;
    public GameObject TotalCPSTMP;

    // �g���C�A�����̃f�[�^�ۊ�
    trialData nowTrialData;

    // ���������{�ݒ�

    // �A�T�C�����ꂽ�Ō��p�^�[���̎�ސ� 26*2 + 22*2 + Space
    public const ushort NumOfKeyPatterns = 97;
    // �A�T�C�����ꂽ�L�[�̐��i�X�y�[�X�ƃV�t�g���ݕ��������������́j
    public const ushort NumOfUniqueChars = 48;
    // �����퐔�i�����Ƀo�C���h����Ȃ��Ō��p�^�[���� 2 ����̂� - 2 �j
    public const ushort NumOfChars = NumOfKeyPatterns - 2;

    // �~�X����
    public ushort MissLimit = 50;

    // �ۑ蕶���̍�����W
    // �A���J�[�� (0, 1) �܂� �e�I�u�W�F�N�g Assignment �̍��ォ��̑��΋����Ŏw��
    const int displayInitX = 12;
    const int displayInitY = -10;

    // �ۑ蕶���� Text Mesh Pro �\�����������Y������
    const int displayCharXdiff = 18;
    const int displayCharYdiff = -41;

    // �ۑ蕶���̕������A�\���̍s���Ȃ�
    const int assignmentLength = 360;
    const int lapLength = 36;
    const int numOfLaps = 10;


    class trialData
    {
        // �������ł����L�[�� = ���łL�[ID�B0 ����n�܂� assignmentLength �őŐ�
        public int typedKeys = 0;
        // ���݉����b�v�ڂ� 1-indexed
        public int nowLap = 1;
        // �g�[�^���^�C���E���b�v�^�C���E�e����Ō��^�C��
        // ���ꂼ��̃^�C���E�~�X�͑S�āu���v�^�C���v�œ����̂ŁA�Ō����Ԃ��o���ɂ͈����Z���K�v
        // lap, key �̔z��� [0] �͔ԕ�
        public long totalTime = 0;
        public long[] lapTime = Enumerable.Repeat<long>(0, numOfLaps + 1).ToArray();
        public long [] keyTime = Enumerable.Repeat<long>(0, assignmentLength + 1).ToArray();
        // �g�[�^���~�X�E���b�v�~�X�E�e�L�[�ӂ�~�X�i�v�邩�H�j
        // ���ꂼ��̃^�C���E�~�X�͑S�āu���v�^�C���v�œ����̂ŁA�Ō����Ԃ��o���ɂ͈����Z���K�v
        // lap, key �̔z��� [0] �͔ԕ�
        public int totalMiss = 0;
        public int[] lapMiss = Enumerable.Repeat<int>(0, numOfLaps + 1).ToArray();
        public int[] keyMiss = Enumerable.Repeat<int>(0, assignmentLength + 1).ToArray();

        public ushort[] trialAssignment_CharID;
        public char[] trialAssignment_Char;

        public trialData()
        {
            Debug.Log("New trialData was instantiated.");

            trialAssignment_CharID = new ushort[assignmentLength];
            trialAssignment_Char = new char[assignmentLength];
        }
    }

    // �X�g�b�v�E�H�b�`
    System.Diagnostics.Stopwatch myStopwatch;

    void Awake()
    {
        myStopwatch = new System.Diagnostics.Stopwatch();

        // EventBus �ɃL�[�����C�x���g�������̃��\�b�h���s���˗�
        EventBus.Instance.SubscribeNormalKeyDown(OnNormalKeyDown);
        EventBus.Instance.SubscribeReturnKeyDown(OnGameStartButtonClick);
        EventBus.Instance.SubscribeEscKeyDown(OnBackButtonClick);
    }
    // Start is called before the first frame update
    void Start()
    {
    }
    // Update is called once per frame
    void Update()
    {
        // Completed, Canceled �ł� TrialInfo �͕K�v�����A(�L�����Z��|����) �ɕ`�悵�Ă��邩�炱���ł͕s�v
        if (gameState == GameState.TrialOn) UpdateTrialInfo();
    }
    void OnDestroy()
    {
        EventBus.Instance.UnsubscribeNormalKeyDown(OnNormalKeyDown);
        EventBus.Instance.UnsubscribeReturnKeyDown(OnGameStartButtonClick);
        EventBus.Instance.UnsubscribeEscKeyDown(OnBackButtonClick);
    }

    // ��Ԑ��䃁�\�b�h
    void StartTrial()
    {
        // ��ʕ\���̏�����
        TotalTimeTMP.GetComponent<TextMeshProUGUI>().text = $"0.000";
        TotalMissTMP.GetComponent<TextMeshProUGUI>().text = $"0";
        TotalCPSTMP.GetComponent<TextMeshProUGUI>().text = $"0.000";
        if (gameState != GameState.Waiting)
            foreach (GameObject obj in assignedCharTMPs) Destroy(obj);

        // nowTrialData �̏�����
        nowTrialData = new trialData();

        // �����𐶐����� nowTrialData �ɃZ�b�g
        RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        byte[] Seeds = new byte[assignmentLength];
        rng.GetBytes(Seeds);

        for (int i = 0; i < assignmentLength; i++)
        {
            System.Random rnd = new System.Random(Seeds[i]);
            ushort rnd_charID = (ushort)(rnd.Next(0, NumOfChars - 1));
            nowTrialData.trialAssignment_CharID[i] = rnd_charID;
            nowTrialData.trialAssignment_Char[i] = MyInputManager.ToChar_FromCharID(rnd_charID);
        }

        Debug.Log(string.Join(" ", nowTrialData.trialAssignment_CharID));
        Debug.Log(string.Join(" ", nowTrialData.trialAssignment_Char));


        // �ۑ蕶���̕`��
        assignedCharTMPs = new GameObject[assignmentLength];

        // Assignment �I�u�W�F�N�g�̌Ƃ��ĕ`�悷�邽��
        GameObject asg = GameObject.Find("Assignment");
        assignedCharTMPs = new GameObject[assignmentLength];

        for (int i = 0; i < assignmentLength; i++)
        {
            assignedCharTMPs[i] = Instantiate(AssignedCharTMPPrefab, asg.transform);
            TextMeshProUGUI assignedCharTMPUGUI = assignedCharTMPs[i].GetComponent<TextMeshProUGUI>();
            assignedCharTMPUGUI.text = nowTrialData.trialAssignment_Char[i].ToString();

            // �\���ꏊ�̎w��
            assignedCharTMPs[i].GetComponent<RectTransform>().localPosition = new Vector3(displayInitX + displayCharXdiff * (i % lapLength), displayInitY + displayCharYdiff * (i / lapLength), 0);
            //assignedCharTMPs[i].GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        }

        // RNGCryptoServiceProvider �� IDisposable �C���^�[�t�F�C�X���������Ă��āA�g�p������������^��j�����Ȃ��Ⴂ���Ȃ��炵���i�����h�L�������g���j
        // IDisposable �C���^�[�t�F�C�X�ɂ� Dispose() �Ƃ������\�b�h��������������Ă���
        // ������p������N���X�́A�u���\�[�X���������ł邩��g���I�������iGC ��҂����jDispose() �Ŕj���������������v�ƍl����΂������낤
        rng.Dispose();

        // �������I����Ă���Q�[���J�n���X�g�b�v�E�H�b�`���J�n
        gameState = GameState.TrialOn;
        myStopwatch.Restart();

        UpdateTrialInfo();
    }
    void CompleteLap()
    {
        int lap = nowTrialData.nowLap;
        // nowTrialData �ւ̃��b�v�^�C���̓���
        nowTrialData.lapTime[lap] = nowTrialData.totalTime;
        // nowTrialData �ւ̃��b�v�~�X�̓���
        nowTrialData.lapMiss[lap] = nowTrialData.totalMiss;

        // �ŏI���b�v�łȂ����̂݁A���b�v�ԍ����X�V�i���݃��b�v�����E�O�ɏo�Ă��܂����Ƃ�h���j
        // �܂��A���̃��b�v���O�̃~�X���Ɠ����ɂ���i 0 �̂܂܂��ƕ\���������j
        if (lap < numOfLaps)
        {
            nowTrialData.lapMiss[lap+1] = nowTrialData.lapMiss[lap];
            nowTrialData.nowLap++;
        }
    }
    void CompleteTrial()
    {
        // �X�g�b�v�E�H�b�`���~�߂邪�A�����̎��Ԃ͂��͂�֌W�����i�L�[�Ō����Ɍv�����Ă��邽�߁j
        myStopwatch.Stop();

        Debug.Assert(gameState == GameState.TrialOn);
        gameState = GameState.Completed;

        // �������Ă���g���C�A�����̕`��i�g���C�A�����ƌv���̎d�����Ⴄ���߁j
        UpdateTrialInfo();
    }
    void CancelTrial()
    {
        // �X�g�b�v�E�H�b�`���~�߂邪�A�����̎��Ԃ͂��͂�֌W�����i�L�[�Ō����Ɍv�����Ă��邽�߁j
        myStopwatch.Stop();

        long time = nowTrialData.totalTime;
        int keys = nowTrialData.typedKeys;
        // nowTrialData �Ƀ��b�v�^�C�������
        int lap = nowTrialData.nowLap;
        nowTrialData.lapTime[lap] = time;
        // nowTrialData �Ƀg�[�^���^�C��

        Debug.Assert(gameState == GameState.TrialOn);
        gameState = GameState.Canceled;

        // �L�����Z�����Ă���g���C�A�����̕`��i�g���C�A�����ƌv���̎d�����Ⴄ���߁j
        UpdateTrialInfo();
    }

    // �g���C�A������
    void OnCorrectKeyDown()
    {
        assignedCharTMPs[nowTrialData.typedKeys].GetComponent<TextMeshProUGUI>().color = new UnityEngine.Color(0.25f, 0.15f, 0.15f, 0.1f);
        // nowTrialData �Ƀg�[�^���^�C���̓���
        nowTrialData.typedKeys++;
        // ���b�v����
        if (nowTrialData.typedKeys % lapLength == 0) CompleteLap();
        // �g���C�A������
        if (nowTrialData.typedKeys == assignmentLength) CompleteTrial();
    }
    void OnIncorrectKeyDown()
    {
        // nowTrialData �Ƀg�[�^���~�X�̓���
        nowTrialData.totalMiss++;
        // nowTrialData �Ƀ��b�v�~�X�̓���
        int lap = nowTrialData.nowLap;
        nowTrialData.lapMiss[lap] = nowTrialData.totalMiss;

        if (nowTrialData.totalMiss >= MissLimit)
        {
            Debug.Log($"�~�X���������܂��B�ŏ������蒼���Ă��������B");
            CancelTrial();
        }

    }

    // �g���C�A�� Information �`�惁�\�b�h
    // �����ł� nowTrialData �ɋL�^�ς݂̃f�[�^��`�悷�邾���Ȃ̂ŁA�X�V�͊e�C�x���g���ɍς܂���K�v����
    void UpdateTrialInfo()
    {
        Debug.Assert(gameState != GameState.Waiting);

        int lap = nowTrialData.nowLap;
        long time;
        // �Q�[�����ł������ꍇ�A���A���Ȍo�ߎ��Ԃ��g���Čv�Z
        if (gameState == GameState.TrialOn) time = myStopwatch.ElapsedMilliseconds;
        // �Q�[��(����|�L�����Z��)��ł������ꍇ�A�Ō�ɃL�[��Ō��������Ԃ��g���Čv�Z
        else time = nowTrialData.totalTime;
        int keys = nowTrialData.typedKeys;
        double cps = (double)(keys * 1000) / time;

        // �g�[�^���^�C���̕\��
        TotalTimeTMP.GetComponent<TextMeshProUGUI>().text = $"{((double)time / 1000):F3}";
        // �g�[�^���~�X�̕\��
        TotalMissTMP.GetComponent<TextMeshProUGUI>().text = $"{nowTrialData.totalMiss}";
        // �g�[�^�� CPS �̕\��
        TotalCPSTMP.GetComponent<TextMeshProUGUI>().text = $"{cps:F3}";
        // ���b�v�^�C���̕\��
        // 1 ~ lap �̃��[�v�Ȃ̂Œ��Ӂilap �� 1-indexed �̂��߁j
        // �X�ɁALapTimeTMP [] �� 0-indexed �ł��邱�Ƃɂ����� -> [(i|lap)-1] �ŃA�N�Z�X
        for (int i = 1; i <= lap; i++)
        {
            LapTimeTMP[i-1].GetComponent<TextMeshProUGUI>().text = $"{(double)(nowTrialData.lapTime[i] - nowTrialData.lapTime[i-1]) / 1000:F3}";
        }
        // �Q�[�����ł������ꍇ�A���݃��b�v�̓��A���Ȍo�ߎ��Ԃ��g���Čv�Z
        if (gameState == GameState.TrialOn) LapTimeTMP[lap-1].GetComponent<TextMeshProUGUI>().text = $"{(double)(time - nowTrialData.lapTime[lap-1]) / 1000:F3}";
        // ���b�v�~�X�̕\��
        for (int i = 1; i <= lap; i++)
        {
            int iLapMiss = nowTrialData.lapMiss[i] - nowTrialData.lapMiss[i - 1];
            LapMissTMP[i-1].GetComponent<TextMeshProUGUI>().text = (iLapMiss == 0 ? "" : iLapMiss.ToString());
        }
        // �I�����Ă��Ȃ����b�v�� �� �Ŗ��߂�
        for (int i = lap+1; i <= numOfLaps; i++)
        {
            LapTimeTMP[i - 1].GetComponent<TextMeshProUGUI>().text = "";
            LapMissTMP[i - 1].GetComponent<TextMeshProUGUI>().text = "";
        }

    }

    // �C�x���g�n���h��
    // EventBus �ɓn���Ď��s���Ă��炤
    // OnBackButtonClick() �ł͎��Ԃ𑪂�Ȃ�
    // �L�����Z���iEsc�j���� totalTime �� �L�����Z������ł͂Ȃ��A�Ō�̐���Ō�or�~�X�Ō������Ƃ邽�߁B
    void OnBackButtonClick()
    {
        if (gameState == GameState.TrialOn) CancelTrial();
        //else if (isCompleted) ;
        else MySceneManager.ChangeSceneRequest("TitleScene");
    }
    void OnGameStartButtonClick()
    {
        if (gameState == GameState.TrialOn) return;
        else StartTrial();
    }
    // OnNormalKeyDown() �Ŏ��Ԃ��v��
    // �g���C�A�����̏��\���� Update() ���Ń^�C�}�[���~�߂ĎG�ɑ���΂������A���b�v�E�g���C�A���������̎��Ԍv���i�L�[�����ɂ����������ԁj�͐��m�ɂƂ�K�v�����邽�߁B
    void OnNormalKeyDown(ushort charID)
    {
        if (gameState != GameState.TrialOn) return;
        if (nowTrialData.trialAssignment_CharID[nowTrialData.typedKeys] == charID)
        {
            nowTrialData.totalTime = myStopwatch.ElapsedMilliseconds;
            Debug.Log($"Correct Key {charID}:{MyInputManager.ToChar_FromCharID(charID)} was Down @ {nowTrialData.totalTime} ms");
            OnCorrectKeyDown();
        }
        else
        {
            nowTrialData.totalTime = myStopwatch.ElapsedMilliseconds;
            Debug.Log($"Incorrect Key {charID}:{MyInputManager.ToChar_FromCharID(charID)} was Down @ {nowTrialData.totalTime} ms");
            OnIncorrectKeyDown();
        }
    }

}

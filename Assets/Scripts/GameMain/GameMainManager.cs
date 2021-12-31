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
        TrialOn,
        ResultOn
    }
    GameState gameState = GameState.Waiting;

    // �ۑ蕶���\���p�v���n�u
    public GameObject AssignedCharTMPPrefab;
    GameObject[] assignedCharTMPs;

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
    const int displayInitX = -375;
    const int displayInitY = 210;

    // �ۑ蕶���� Text Mesh Pro �\�����������Y������
    const int displayCharXdiff = 19;
    const int displayCharYdiff = -39;

    // �ۑ蕶���̕������A�\���̍s���Ȃ�
    const int assignmentLength = 360;
    const int displayRowLength = 36;


    class trialData
    {
        public long totalTime;
        public long [] eachTime;
        // �������ł����L�[�� = ���łL�[ID�B0 ����n�܂� assignmentLength �őŐ�
        public int typedKeys;
        // �~�X�L�[���BMissLimit �𒴂���� Esc
        public int missedKeys;

        public ushort[] trialAssignment_CharID;
        public char[] trialAssignment_Char;

        public trialData(int assignmentLength)
        {
            Debug.Log("New trialData was instantiated.");
            totalTime = 0;
            eachTime = new long[assignmentLength];
            typedKeys = 0;

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
        
    }
    void OnDestroy()
    {
        EventBus.Instance.UnsubscribeNormalKeyDown(OnNormalKeyDown);
        EventBus.Instance.UnsubscribeReturnKeyDown(OnGameStartButtonClick);
        EventBus.Instance.UnsubscribeEscKeyDown(OnBackButtonClick);
    }

    // �g���C�A������
    void OnCorrectKeyDown()
    {
        assignedCharTMPs[nowTrialData.typedKeys].SetActive(false);
        nowTrialData.typedKeys++;
    }
    void OnIncorrectKeyDown()
    {
        nowTrialData.missedKeys++;
        if (nowTrialData.missedKeys >= MissLimit)
        {
            Debug.Log($"�~�X���������܂��B�ŏ������蒼���Ă��������B");
            CancelTrial();
        }
    }

    // ��Ԑ��䃁�\�b�h
    void StartTrial()
    {
        nowTrialData = new trialData(assignmentLength);

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
            assignedCharTMPs[i].GetComponent<RectTransform>().localPosition = new Vector3(displayInitX + displayCharXdiff * (i % displayRowLength), displayInitY + displayCharYdiff * (i / displayRowLength), 0);
            //assignedCharTMPs[i].GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        }

        // RNGCryptoServiceProvider �� IDisposable �C���^�[�t�F�C�X���������Ă��āA�g�p������������^��j�����Ȃ��Ⴂ���Ȃ��炵���i�����h�L�������g���j
        // IDisposable �C���^�[�t�F�C�X�ɂ� Dispose() �Ƃ������\�b�h��������������Ă���
        // ������p������N���X�́A�u���\�[�X���������ł邩��g���I�������iGC ��҂����jDispose() �Ŕj���������������v�ƍl����΂������낤
        rng.Dispose();

        // �������I����Ă���Q�[���J�n���X�g�b�v�E�H�b�`���J�n
        gameState = GameState.TrialOn;
        myStopwatch.Restart();
    }
    void CancelTrial()
    {
        Debug.Assert(gameState == GameState.TrialOn);

        Debug.Log("Quitting Trial...");
        gameState = GameState.Waiting;
        myStopwatch.Stop();
        nowTrialData = null;

        foreach (GameObject obj in assignedCharTMPs)
        {
            Destroy(obj);
        }        
    }

    // �C�x���g�n���h��
    // EventBus �ɓn���Ď��s���Ă��炤
    void OnBackButtonClick()
    {
        if (gameState == GameState.TrialOn) CancelTrial();
        //else if (isResultOn) ;
        else MySceneManager.ChangeSceneRequest("TitleScene");
    }
    void OnGameStartButtonClick()
    {
        if (gameState == GameState.TrialOn) return;
        else StartTrial();
    }
    void OnNormalKeyDown(ushort charID)
    {
        if (gameState != GameState.TrialOn) return;
        if (nowTrialData.trialAssignment_CharID[nowTrialData.typedKeys] == charID)
        {
            Debug.Log($"Correct Key {charID}:{MyInputManager.ToChar_FromCharID(charID)} was Down @ {myStopwatch.ElapsedMilliseconds} ms");
            OnCorrectKeyDown();
        }
        else
        {
            Debug.Log($"Incorrect Key {charID}:{MyInputManager.ToChar_FromCharID(charID)} was Down @ {myStopwatch.ElapsedMilliseconds} ms");
            OnIncorrectKeyDown();
        }
    }

}

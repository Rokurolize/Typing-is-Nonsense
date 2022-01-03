using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_STANDALONE_WIN
/// https://github.com/Elringus/UnityRawInput
using UnityRawInput;
#endif

/// <summary>
///  �v���b�g�t�H�[���ˑ��̃L�[�{�[�h���͎󂯎����s���AEventBus �ɒʒm�𔭍s����
///  ���̌�A���̃N���X����A�ʒm�����ꂽ�ۂɎ��s���Ăق������\�b�h�� EventBus �Ƀf���Q�[�g�Ƃ��ēo�^����
/// </summary>
public class MyInputManager : MonoBehaviour
{
    // �V�t�g�Ǘ��B�V�t�g�L�[�� 2 ����̂ŁAbool �łȂ� int
    // ���\�b�h���ϐ��i���͎󂯎���̊m�肵�� shifted�j�Ƌ�ʂ��邽�߁A�t�B�[���h�Ƃ��Ă� shifted �ɂ� _ �t�^
    [System.Obsolete("��������Ԃɂ���ăo�O����������\������B�v����")]
    private int _shifted = 0;

    // KeyID �� CharID�iKey �ɃA�T�C�����ꂽ�����j�Ԃ̕ϊ�
    private static Dictionary<ushort, ushort> dictToKeyID_FromCharID = new Dictionary<ushort, ushort>();
    private static Dictionary<ushort, ushort> dictToCharID_FromKeyID = new Dictionary<ushort, ushort>();

    // Char �� CharID �Ԃ̕ϊ�
    private static Dictionary<ushort, char> dictToChar_FromCharID = new Dictionary<ushort, char>();
    private static Dictionary<char, ushort> dictToCharID_FromChar = new Dictionary<char, ushort>();

#if UNITY_STANDALONE_WIN
    private static Dictionary<RawKey, ushort> dictToKeyID_FromRawKey = new Dictionary<RawKey, ushort>();
#endif


    void Awake()
    {
#if UNITY_STANDALONE_WIN
        Debug.Log("standalone win");
        var workInBackground = false;
        RawKeyInput.Start(workInBackground);

        RawKeyInput.OnKeyDown += KeyDownHandlerForWin;
        RawKeyInput.OnKeyUp += KeyUpHandlerForWin;

        // �L�[�o�C���h�@�\��������폜����\��́A�f�t�H���g Rawkey:KeyID Dictionary �ݒ�
        dictToKeyID_FromRawKey.Add(RawKey.Space, 0);
        for (int i = 0; i <= 25; i++) dictToKeyID_FromRawKey.Add(RawKey.A + (ushort)i, (ushort)(i + 1));
        dictToKeyID_FromRawKey.Add((RawKey)0x31, 27);
        dictToKeyID_FromRawKey.Add((RawKey)0x32, 28);
        dictToKeyID_FromRawKey.Add((RawKey)0x33, 29);
        dictToKeyID_FromRawKey.Add((RawKey)0x34, 30);
        dictToKeyID_FromRawKey.Add((RawKey)0x35, 31);
        dictToKeyID_FromRawKey.Add((RawKey)0x36, 32);
        dictToKeyID_FromRawKey.Add((RawKey)0x37, 33);
        dictToKeyID_FromRawKey.Add((RawKey)0x38, 34);
        dictToKeyID_FromRawKey.Add((RawKey)0x39, 35);
        dictToKeyID_FromRawKey.Add((RawKey)0x30, 36);
        dictToKeyID_FromRawKey.Add(RawKey.OEMMinus, 37);
        dictToKeyID_FromRawKey.Add(RawKey.OEM7, 38);
        dictToKeyID_FromRawKey.Add(RawKey.OEM5, 39);
        dictToKeyID_FromRawKey.Add(RawKey.OEM3, 40);
        dictToKeyID_FromRawKey.Add(RawKey.OEM4, 41);
        dictToKeyID_FromRawKey.Add(RawKey.OEMPlus, 42);
        dictToKeyID_FromRawKey.Add(RawKey.OEM1, 43);
        dictToKeyID_FromRawKey.Add(RawKey.OEM6, 44);
        dictToKeyID_FromRawKey.Add(RawKey.OEMComma, 45);
        dictToKeyID_FromRawKey.Add(RawKey.OEMPeriod, 46);
        dictToKeyID_FromRawKey.Add(RawKey.OEM2, 47);
        dictToKeyID_FromRawKey.Add(RawKey.OEM102, 48);
        dictToKeyID_FromRawKey.Add(RawKey.Shift, 100);
        dictToKeyID_FromRawKey.Add(RawKey.Return, 101);
        dictToKeyID_FromRawKey.Add(RawKey.Escape, 102);
#endif

        // �e�X�g�p�A�Ƃ肠���� KeyID �� CharID �� Char �ɓK���ȑΉ�������
        string defaultKeyCharMap = " abcdefghijklmnopqrstuvwxyz1234567890-^\0@[;:],./\\ABCDEFGHIJKLMNOPQRSTUVWXYZ!\"#$%&'()\0=~|`{+*}<>?_";
        ushort idx = 0;
        ushort charidx = 0;
        foreach (char c in defaultKeyCharMap)
        {
            if (c == '\0')
            {
                idx++;
            }
            else
            {
                dictToKeyID_FromCharID[charidx] = idx;
                dictToCharID_FromKeyID[idx] = charidx;
                dictToChar_FromCharID[charidx] = c;
                dictToCharID_FromChar[c] = charidx;
                idx++;
                charidx++;
            }

        }
}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDisable()
    {
#if UNITY_STANDALONE_WIN
        RawKeyInput.OnKeyDown -= KeyDownHandlerForWin;
        RawKeyInput.OnKeyUp -= KeyUpHandlerForWin;

        RawKeyInput.Stop(); // ����Ȃ��� Unity Editor ��������
#endif
    }

    // �v���b�g�t�H�[���ˑ��̃L�[���̓C�x���g�n���h��
    // �v���b�g�t�H�[���ˑ��̃L�[�R�[�h���A���[�U�ŗL�� KeyID �ɕϊ����ăv���b�g�t�H�[����ˑ��������Ă�
#if UNITY_STANDALONE_WIN
    private void KeyDownHandlerForWin(RawKey key)
    {
        if (!dictToKeyID_FromRawKey.ContainsKey(key)) {
            Debug.Log($"No KeyID for RawKey : {key.ToString()}");
            return;
        }
        else OnKeyDown(ToKeyID_FromRawKey(key));
    }
    private void KeyUpHandlerForWin(RawKey key)
    {
        if (key.ToString() == "Shift") OnShiftKeyUp();
    }
    private ushort ToKeyID_FromRawKey(RawKey key)
    {
        return dictToKeyID_FromRawKey[key];
    }
#endif

    // ��������A�v���b�g�t�H�[���Ɉˑ����Ȃ�����
    // ���̎��_�܂łŁA���͂͑S�� KeyID �ɕϊ�����Ă���

    // KeyID �� CharID�iKey �ɃA�T�C�����ꂽ�����j�Ԃ̕ϊ�
    public static ushort ToKeyID_FromCharID(ushort charID)
    {
        return dictToKeyID_FromCharID[charID];
    }
    public static ushort ToCharID_FromKeyID(ushort keyID)
    {
        return dictToCharID_FromKeyID[keyID];
    }

    // Char �� CharID �Ԃ̕ϊ�
    public static char ToChar_FromCharID(ushort charID)
    {
        return dictToChar_FromCharID[charID];
    }
    public static ushort ToCharID_FromChar(char c)
    {
        return dictToCharID_FromChar[c];
    }


    // �C�x���g�n���h��

    /// KeyID �����ꕶ�� 100 ~ �ł���΁A���ꂼ��̃C�x���g�����s
    ///         �ʏ핶�� 0 ~ 96 �ł���΁AOnNormalKeyDown() �����s
    ///         �� �������ACharID ������U���Ă��Ȃ� 2 �L�[�̏ꍇ�́A���������s���Ȃ�
    private void OnKeyDown(ushort keyID)
    {
        ushort retkey = keyID;
        // Space �ɂ̓V�t�g�֌W�����Ȃ̂ŁA�V�t�g�����Ɉڍs�����Ȃ�
        if (retkey == 0) OnNormalKeyDown(0);
        else if (retkey == 100) OnShiftKeyDown();
        else if (retkey == 101) OnReturnKeyDown();
        else if (retkey == 102) OnEscKeyDown();
        else
        {
            bool shifted = false;
            if (_shifted > 0) shifted = true;
            if (shifted) retkey += GameMainManager.NumOfUniqueChars;

            if (!dictToCharID_FromKeyID.ContainsKey(retkey))
                Debug.Log($"KeyID {retkey} �� CharID ���A�T�C������Ă��܂���B");
            else OnNormalKeyDown(ToCharID_FromKeyID(retkey));
        }

    }
    /// CharID �̃A�T�C������Ă��Ȃ��L�[�����͖�������΂����̂ŁA
    private void OnNormalKeyDown(ushort charID)
    {
        EventBus.Instance.NotifyNormalKeyDown(charID);
    }
    private void OnShiftKeyDown()
    {
        _shifted++;
    }
    private void OnShiftKeyUp()
    {
        _shifted--;
    }
    private void OnReturnKeyDown()
    {
        EventBus.Instance.NotifyReturnKeyDown();
    }
    private void OnEscKeyDown()
    {
        EventBus.Instance.NotifyEscKeyDown();
    }

    /// �L�[�o�C���h�p
#if UNITY_STANDALONE_WIN
    private void OnRawKeyDown(RawKey key) {
        EventBus.Instance.NotifyRawKeyDown(key);
    }
#endif

}

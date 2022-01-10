/// �Q�[���S�̂Ŏg�p����N���X��\���̂��`
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq; // �z�񏉊����p
using System.IO; // �t�@�C������

#if UNITY_STANDALONE_WIN
/// https://github.com/Elringus/UnityRawInput
using UnityRawInput;
#endif


[System.Serializable]
public class TrialData
{
    // �~�X���~�b�g
    public ushort missLimit = 50;
    // �������ł����L�[�� = ���łL�[ID�B0 ����n�܂� assignmentLength �őŐ�
    public int typedKeys = 0; // �{��
    // ���݉����b�v�ڂ� 1-indexed
    public int nowLap = 1; // ��{���itypedKeys����Z�o�j
    // �g�[�^���^�C���E���b�v�^�C���E�e����Ō��^�C��
    // ���ꂼ��̃^�C���E�~�X�͑S�āu���v�^�C���v�œ����̂ŁA�Ō����Ԃ��o���ɂ͈����Z���K�v
    // lap, key �̔z��� [0] �͔ԕ�
    public long totalTime = 0; // �����itypedKeys��keyTime����Z�o�͂ł���j
    public long[] lapTime; // ��{���ikeyTime����Z�o�j
    public long[] keyTime; // �{��
    // �g�[�^���~�X�E���b�v�~�X�E�e�L�[�ӂ�~�X�i�v�邩�H�j
    // ���ꂼ��̃^�C���E�~�X�͑S�āu���v�^�C���v�œ����̂ŁA�Ō����Ԃ��o���ɂ͈����Z���K�v
    // lap, key �̔z��� [0] �͔ԕ�
    public int totalMiss = 0; // ���{���@keyMiss ������Ȃ�{���ł͂Ȃ����A���v���o���̂Ɏ��Ԃ������邩�炠������������
    public int[] lapMiss; // ��{���@��L�Ɠ��l���Ԃ��������ł��Ȃ����c�c
    public int[] keyMiss; // �{���H�@�K�v���͒u���Ă����āA���̋@�\������Ȃ炱�ꂪ�{��

    public ushort[] trialAssignment_CharID; // �{��
    //public char[] trialAssignment_Char; // ��{��

    public TrialData(ushort lim, int assignmentLength, int numOfLaps)
    {
        Debug.Log("New trialData was instantiated.");

        missLimit = lim;

        lapTime = Enumerable.Repeat<long>(0, numOfLaps + 1).ToArray();
        keyTime = Enumerable.Repeat<long>(0, assignmentLength + 1).ToArray();

        lapMiss = Enumerable.Repeat<int>(0, numOfLaps + 1).ToArray();
        keyMiss = Enumerable.Repeat<int>(0, assignmentLength + 1).ToArray();

        trialAssignment_CharID = new ushort[assignmentLength];
    }
}

/// <summary>
/// ���jNullKeyMap �̓L�[�o�C���h���ɍX�V���ꂸ�AKeyBind.SaveToJson() ���ɍX�V�����
/// </summary>
[System.Serializable]
public class KeyBind
{
    public ushort[] RawKeyMap; // 0 ~ 50
    public ushort[] NullKeyMap; // 0 ~ 2
    public char[] CharMap; // 0 ~ 96

    public KeyBind()
    {
        // �f�t�H���g���Z�b�g
        SetToDefault("JIS");
    }
    public KeyBind(string def)
    {
        SetToDefault(def);
    }

    public void SetToDefault(string def)
    {
        switch (def)
        {
            case "JIS":
                // ���͌Œ�� JIS �z��̃f�[�^�������Ă��邪�A���� US �� UK �Ȃǂ�����ăA�Z�b�g�ɂ��Ă���
                RawKeyMap = new ushort[] { 16, 16, 32, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 49, 50, 51, 52, 53, 54, 55, 56, 57, 48, 189, 222, 220, 192, 219, 187, 186, 221, 188, 190, 191, 226 };
                NullKeyMap = new ushort[] { 39, 84 };
                CharMap = " abcdefghijklmnopqrstuvwxyz1234567890-^\0@[;:],./\\ABCDEFGHIJKLMNOPQRSTUVWXYZ!\"#$%&'()\0=~|`{+*}<>?_".ToCharArray();
                break;
            case "US":
                KeyBind loaded = JsonUtility.FromJson<KeyBind>("{ \"RawKeyMap\":[16,16,32,65,66,67,68,69,70,71,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86,87,88,89,90,49,50,51,52,53,54,55,56,57,48,189,187,192,219,221,220,186,222,188,190,191,35],\"NullKeyMap\":[48,96],\"CharMap\":[32,97,98,99,100,101,102,103,104,105,106,107,108,109,110,111,112,113,114,115,116,117,118,119,120,121,122,49,50,51,52,53,54,55,56,57,48,45,61,96,91,93,92,59,39,44,46,47,0,65,66,67,68,69,70,71,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86,87,88,89,90,33,64,35,36,37,94,38,42,40,41,95,43,126,123,125,124,58,34,60,62,63,0]}");

                // �ǂݍ��� KeyBind ���o���f�[�V������ʂ�΍̗p
                // MyKeyBind0.json �ւ̕ۑ��̓��[�U�� SAVE ����܂ł͍s��Ȃ�
                string errMsg = "";
                if (loaded.ValidationCheck(ref errMsg))
                {
                    RawKeyMap = loaded.RawKeyMap;
                    NullKeyMap = loaded.NullKeyMap;
                    CharMap = loaded.CharMap;
                }
                else
                {
                    Debug.Log($"Validation failed. KeyBind not changed.\n{errMsg}");
                }
                break;
            default:
                Debug.Log($"{def} is Invalid Default Keyboard Layout name.");
                break;
        }
    }

    // validation
    public bool ValidationCheck(ref string ErrorMsg)
    {
        bool ret = true;
        // RawKeyMap ���`�F�b�N
        Debug.Assert(RawKeyMap.Length == 51);
        HashSet<ushort> rawKeys = new HashSet<ushort>();
        for (int i = 0; i < RawKeyMap.Length; i++)
        {
            // RawKey �̏d���͋����Ȃ����AKeyID:1 �ŏd�����o��̂� OK�i�V�t�g�L�[�̏d��������j
            // KeyID: 0 -> 1 �̏��Ō��Ă��邩��A1 �ŏd�����o�遁�V�t�g�L�[�̏d���ƒf��\
            if (rawKeys.Contains(RawKeyMap[i]) && i != 1)
            {
                ErrorMsg += $"Duplicated Raw Key: {RawKeyMap[i]} (KeyID: {i})\n";
                ret = false;
            }
            if (!System.Enum.IsDefined(typeof(RawKey), RawKeyMap[i]))
            {
                ErrorMsg += $"Invalid Raw Key: {RawKeyMap[i]} (KeyID: {i})\n";
                ret = false;
            }
            rawKeys.Add(RawKeyMap[i]);
        }

        // NullKeyMap �̓`�F�b�N���Ȃ��iCharMap ����������� Save ���ɐ��������������j

        // CharMap ���`�F�b�N
        Debug.Assert(CharMap.Length == 97);
        // �d���`�F�b�N
        HashSet<ushort> chars = new HashSet<ushort>();
        for (int i = 0; i < 97; i++)
        {
            if (chars.Contains(CharMap[i]) && CharMap[i] != '\0')
            {
                ErrorMsg += $"Duplicated Char: {CharMap[i]} (CharID: {i})\n";
                ret = false;
            }
            chars.Add(CharMap[i]);
        }
        // Unshifted: 0 ~ 48 �� null �� 1 �K�v
        int l_null = 0;
        for (int i = 0; i < 49; i++) if (CharMap[i] == '\0') l_null++;
        if (l_null != 1)
        {
            ErrorMsg += $"You need 1 (NULL) in Char (Unshifted). Now: {l_null}\n";
            ret = false;
        }
        // Unshifted: 49 ~ 96 �� null �� 1 �K�v
        int r_null = 0;
        for (int i = 49; i < 97; i++) if (CharMap[i] == '\0') r_null++;
        if (r_null != 1)
        {
            ErrorMsg += $"You need 1 (NULL) in Char (Shifted). Now: {r_null}\n";
            ret = false;
        }

        return ret;
    }

    /// <summary>
    /// �Z�[�u�E���[�h slot 0 �́u���̗p���Ă���L�[�o�C���h�v������
    /// </summary>
    /// <param name="slot"></param>
    public void SaveToJson(ushort slot)
    {
        Debug.Log($"save to slot {slot}");
        string ErrMsg = "";
        if (ValidationCheck(ref ErrMsg))
        {
            int nullCnt = 0;
            for (int i = 0; i < CharMap.Length; i++)
            {
                if (CharMap[i] == '\0')
                {
                    NullKeyMap[nullCnt] = (ushort)i;
                    nullCnt++;
                }
            }
            string jsonstr = JsonUtility.ToJson(this);
            if (!Directory.Exists(Application.dataPath + "/SaveData"))
                Directory.CreateDirectory(Application.dataPath + "/SaveData");
            string datapath = Application.dataPath + "/SaveData/MyKeyBind" + slot.ToString() + ".json";
            StreamWriter writer = new StreamWriter(datapath, false);
            writer.WriteLine(jsonstr);
            writer.Flush();
            writer.Close();
        }
        else
        {
            Debug.Log($"Validation failed. KeyBind not saved to slot {slot}.\n{ErrMsg}");

        }
    }
    /// <summary>
    /// MyKeyBind{slot}.json ���� KeyBind �N���X�Ƀf�[�^�����[�h����
    /// ���̎��_�ł́A���ۂɎg�p���Ă���L�[�o�C���h�iMyKeyBind0.json�j�ɂ͔��f���Ȃ�
    /// </summary>
    /// <param name="slot"></param>
    public void LoadFromJson(ushort slot)
    {
        Debug.Log($"load from slot {slot}");
        if (!Directory.Exists(Application.dataPath + "/SaveData"))
            Directory.CreateDirectory(Application.dataPath + "/SaveData");
        string loaddatapath = Application.dataPath + "/SaveData/MyKeyBind" + slot.ToString() + ".json";
        if (File.Exists(loaddatapath))
        {
            StreamReader reader = new StreamReader(loaddatapath);
            string loadedjson = reader.ReadToEnd();
            reader.Close();
            KeyBind loaded = JsonUtility.FromJson<KeyBind>(loadedjson);

            // �ǂݍ��� KeyBind ���o���f�[�V������ʂ�΍̗p���� slot 0 �ɕۊ�
            string errMsg = "";
            if (loaded.ValidationCheck(ref errMsg))
            {
                RawKeyMap = loaded.RawKeyMap;
                NullKeyMap = loaded.NullKeyMap;
                CharMap = loaded.CharMap;
            }
            else {
                Debug.Log($"Validation failed. KeyBind not changed.\n{errMsg}");
            }
        }
        else
        {
            Debug.Log($"MyKeyBind{slot}.json Not Found. KeyBind not changed.");
        }
    }
}

/// <summary>
/// PlayerPrefs �ɓ����ݒ�
/// </summary>
[System.Serializable]
public class Config
{
    public ushort MissLimit = 50;

    public void Save()
    {
        PlayerPrefs.SetInt("MissLimit", MissLimit);
    }
    public void Load()
    {
        int misslim = PlayerPrefs.GetInt("MissLimit", 50);
        Debug.Assert(0 <= misslim && misslim <= 360);
        MissLimit = (ushort)misslim;
    }
}

[System.Obsolete("When implementing UtilKeyBinding, changes needed.")]
public class KeyBindDicts
{
    // KeyID �� CharID�iKey �ɃA�T�C�����ꂽ�����j�Ԃ̕ϊ�
    public Dictionary<ushort, ushort> dictToKeyID_FromCharID;
    public Dictionary<ushort, ushort> dictToCharID_FromKeyID;

    // Char �� CharID �Ԃ̕ϊ�
    public Dictionary<ushort, char> dictToChar_FromCharID;
    public Dictionary<char, ushort> dictToCharID_FromChar;

#if UNITY_STANDALONE_WIN
    public Dictionary<RawKey, ushort> dictToKeyID_FromRawKey = new Dictionary<RawKey, ushort>();
#endif

    // �L�[�o�C���h�@�\��������������Ȃ��̃R���X�g���N�^�͕s�v�B�ĂׂȂ��悤�ɂ���
    private KeyBindDicts() { }
    public KeyBindDicts(KeyBind keyBind)
    {
        dictToKeyID_FromCharID = new Dictionary<ushort, ushort>();
        dictToCharID_FromKeyID = new Dictionary<ushort, ushort>();
        dictToChar_FromCharID = new Dictionary<ushort, char>();
        dictToCharID_FromChar = new Dictionary<char, ushort>();
        dictToKeyID_FromRawKey = new Dictionary<RawKey, ushort>();

        // KeyID <- RawKey
        for (int i = 0; i < 51; i++)
        {
            // 0 == null
            dictToKeyID_FromRawKey[(RawKey)(keyBind.RawKeyMap[i])] = (ushort)i;
        }
        // Char <-> CharID
        for (int i = 0; i < 97; i++)
        {
            if (keyBind.CharMap[i] == '\0') continue;
            dictToChar_FromCharID[(ushort)i] = keyBind.CharMap[i];
            dictToCharID_FromChar[keyBind.CharMap[i]] = (ushort)i;
        }
        // KeyID <-> CharID
        for (int i = 0; i < 97; i++)
        {
            if (keyBind.NullKeyMap[0] == i || keyBind.NullKeyMap[1] == i) continue;
            else
            {
                dictToKeyID_FromCharID[(ushort)i] = (ushort)(i + 2);
                dictToCharID_FromKeyID[(ushort)(i + 2)] = (ushort)i;
            }
        }
        // ����L�[�̏����i���̂����o�C���h�� Config �Ȃǂ��痬�����ދ@�\������j
        dictToKeyID_FromRawKey[RawKey.Return] = 100; // 13
        dictToKeyID_FromRawKey[RawKey.Escape] = 101; // 27
    }

    // ���\�b�h
#if UNITY_STANDALONE_WIN
    public ushort ToKeyID_FromRawKey(RawKey key)
    {
        return dictToKeyID_FromRawKey[key];
    }
#endif

    // ��������A�v���b�g�t�H�[���Ɉˑ����Ȃ�����
    // ���̎��_�܂łŁA���͂͑S�� KeyID �ɕϊ�����Ă���

    // KeyID �� CharID�iKey �ɃA�T�C�����ꂽ�����j�Ԃ̕ϊ�
    public ushort ToKeyID_FromCharID(ushort charID)
    {
        return dictToKeyID_FromCharID[charID];
    }
    public ushort ToCharID_FromKeyID(ushort keyID)
    {
        return dictToCharID_FromKeyID[keyID];
    }

    // Char �� CharID �Ԃ̕ϊ�
    public char ToChar_FromCharID(ushort charID)
    {
        return dictToChar_FromCharID[charID];
    }
    public ushort ToCharID_FromChar(char c)
    {
        return dictToCharID_FromChar[c];
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

#if UNITY_STANDALONE_WIN
/// https://github.com/Elringus/UnityRawInput
using UnityRawInput;
#endif

public class KeybindingManager : MonoBehaviour
{
    // �v���n�u�ݒ�A�y�уv���n�u�i�[�z��
    public GameObject KeyBinderPrefab; 
    GameObject[] keyBinder = new GameObject[104];

    public GameObject CharBinderPrefab;
    GameObject[] charBinder = new GameObject[97];

    // ���ۂɂ��̃V�[���œ��͂𓾂邽�߂̃L�[�o�C���h�f�[�^
    [SerializeField]
    KeyBind keyBind;
    KeyBindDicts dicts;

    // �ݒ蒆�̃L�[�o�C���h�f�[�^
    [SerializeField]
    KeyBind nowBindingKeyBind;

    // ��ԕϐ�
    // �L�[���͂̎�t�҂����
    // -1 : notListening, 0 ~ 50 : Listening KeyID 0~50 
    int nowListening = -1;

    private void Awake()
    {
        // ����p�L�[�o�C���h�̓ǂݍ���
        keyBind = new KeyBind();
        keyBind.LoadFromJson(0);
        dicts = new KeyBindDicts(keyBind);

        // �ݒ�p�L�[�o�C���h�̐���
        nowBindingKeyBind = keyBind;

        // �G���[���b�Z�[�W�̏�����
        GameObject.Find("ErrorTMP").GetComponent<TextMeshProUGUI>().text = "";

        // �C�x���g�n���h���̓o�^�i�������̓C���X�y�N�^����o�^�ł��Ȃ����߁j
        GameObject.Find("UseThisButton").GetComponent<Button>().onClick.AddListener(
            () =>
            {
                if (ValidateNowBinding())
                {
                    nowBindingKeyBind.SaveToJson(0);
                    keyBind = nowBindingKeyBind;
                    // MyKeyBind0.json �̕ύX�� EventBus �ɒʒm -> MyInputManager �����m
                    EventBus.Instance.NotifyKeyBindChanged();
                }
            });
        GameObject.Find("Save1Button").GetComponent<Button>().onClick.AddListener(() => OnSaveButtonClick(1));
        GameObject.Find("Save2Button").GetComponent<Button>().onClick.AddListener(() => OnSaveButtonClick(2));
        GameObject.Find("Save3Button").GetComponent<Button>().onClick.AddListener(() => OnSaveButtonClick(3));
        GameObject.Find("Load1Button").GetComponent<Button>().onClick.AddListener(() => OnLoadButtonClick(1));
        GameObject.Find("Load2Button").GetComponent<Button>().onClick.AddListener(() => OnLoadButtonClick(2));
        GameObject.Find("Load3Button").GetComponent<Button>().onClick.AddListener(() => OnLoadButtonClick(3));

        // KeyID : 000 ~ 050
        for (int i = 0; i < 51; i++) InstantiateKeyBinder(i, 190, -2 - i * 40);

        // CharBinderID : 00 ~ 48 (Unshifted)
        for (int i = 0; i < 49; i++) InstantiateCharBinder(i, 0, -82 - i * 40);

        // CharBinderID : 49 ~ 96 (Shifted)
        for (int i = 49; i < 97; i++)
        {
            InstantiateCharBinder(i, 380, -122 - (i-49) * 40);
            // InputField.colors.normalColor �ɒ��� Color �����ł��Ȃ����߁A�܂� ColorBlock ���܂邲�ƍ���ēn��
            ColorBlock cb = charBinder[i].GetComponent<RectTransform>().Find("CharInputField").GetComponent<InputField>().colors;
            cb.normalColor = new Color(0.85f, 0.7f, 0.5f, 1f);
            charBinder[i].GetComponent<RectTransform>().Find("CharInputField").GetComponent<InputField>().colors = cb;
        }

        // EventBus �ɓo�^
        EventBus.Instance.SubscribeRawKeyDown(RawKeyDownEventHandler);
    }

    // Start is called before the first frame update
    void Start()
    {
        // �`��̍X�V
        UpdateDisplay();
    }

    // Update is called once per frame
    void Update()
    {
    }
    void OnDestroy()
    {
        // EventBus ����E��
        EventBus.Instance.UnsubscribeRawKeyDown(RawKeyDownEventHandler);
    }

    bool ValidateNowBinding()
    {
        string errMsg = "";
        if (nowBindingKeyBind.ValidationCheck(ref errMsg))
        {
            GameObject.Find("ErrorTMP").GetComponent<TextMeshProUGUI>().text = "";
            return true;
        }
        else
        {
            GameObject.Find("ErrorTMP").GetComponent<TextMeshProUGUI>().text = errMsg;
            return false;
        }

    }

    // �v���n�u����
    void InstantiateKeyBinder(int id, int x, int y)
    {
        keyBinder[id] = Instantiate(KeyBinderPrefab, GameObject.Find("Content").GetComponent<RectTransform>());
        keyBinder[id].GetComponent<RectTransform>().localPosition = new Vector3(x, y, 0);
        keyBinder[id].GetComponent<RectTransform>().Find("KeyIDText").GetComponent<Text>().text = $"{id:#000}";

        // �C�x���g�n���h���̐ݒ�
        // ������ id �����̂܂܈����ɓn���ƁA�X�R�[�v�̊֌W�Ńo�O��
        ushort buttonNum = (ushort)id;
        keyBinder[id].GetComponent<RectTransform>().Find("KeyBindButton").GetComponent<Button>().onClick.AddListener(() => OnKeyBindButtonClick(buttonNum));
    }
    void InstantiateCharBinder(int id, int x, int y)
    {
        charBinder[id] = Instantiate(CharBinderPrefab, GameObject.Find("Content").GetComponent<RectTransform>());
        charBinder[id].GetComponent<RectTransform>().localPosition = new Vector3(x, y, 0);
        charBinder[id].GetComponent<RectTransform>().Find("CharIDText").GetComponent<Text>().text = $"{id:#00}";

        // ������ id �����̂܂܈����ɓn���ƁA�X�R�[�v�̊֌W�Ńo�O��
        ushort buttonNum = (ushort)id;
        charBinder[id].GetComponent<RectTransform>().Find("CharInputField").GetComponent<InputField>().onEndEdit.AddListener((str) => OnCharEdited(buttonNum, str));
    }

    // �C�x���g�n���h��
    public void OnKeyBindButtonClick(ushort keyID)
    {
        Debug.Log("keyBinder clicked. keyID: " + keyID);
        nowListening = keyID;
        //keyBinder[keyID].Find("KeyBindButton").GetComponent<Button>().colors = 
    }
    public void OnCharEdited(ushort charBinderID, string str)
    {
        char c;
        Debug.Log("charBinder edited. charBinderID: " + charBinderID + "\nstr : " + str);
        if (str == "" || str == "\0") c = '\0';
        else c = str[0];
        nowBindingKeyBind.CharMap[charBinderID] = c;
        UpdateDisplay();
        ValidateNowBinding();
    }
    public void OnSaveButtonClick(ushort slot)
    {
        if (ValidateNowBinding()) nowBindingKeyBind.SaveToJson(slot);
    }
    public void OnLoadButtonClick(ushort slot)
    {
        nowBindingKeyBind.LoadFromJson(slot);
        UpdateDisplay();
        ValidateNowBinding();
    }
    public void OnLoadJISButtonClick()
    {
        nowBindingKeyBind.SetToDefault("JIS");
        UpdateDisplay();
        ValidateNowBinding();
    }
    public void OnLoadUSButtonClick()
    {
        nowBindingKeyBind.SetToDefault("US");
        UpdateDisplay();
        ValidateNowBinding();
    }

    public void OnBackToTitleButtonClick()
    {
        MySceneManager.ChangeSceneRequest("TitleScene");
    }
    public void RawKeyDownEventHandler(RawKey key)
    {
        if (nowListening == -1) return;
        nowBindingKeyBind.RawKeyMap[nowListening] = (ushort)key;
        nowListening = -1;
        UpdateDisplay();
        ValidateNowBinding();
    }


    // �`�搧��
    void UpdateDisplay()
    {
        // KeyID : 000 ~ 050
        for (int i = 0; i < 51; i++)
        {
            keyBinder[i].GetComponent<RectTransform>().Find("KeyBindButton").Find("RawKeyText").GetComponent<Text>().text = ((RawKey)(nowBindingKeyBind.RawKeyMap[i])).ToString();
        }

        // CharBinderID : 00 ~ 48 (Unshifted)
        for (int i = 0; i < 49; i++)
        {
            string s = nowBindingKeyBind.CharMap[i].ToString();
            if (s == "\0" || s == "") s = "(NULL)";
            if (s == " ") s = "(Space)";
            charBinder[i].GetComponent<RectTransform>().Find("CharInputField").GetComponent<InputField>().text = s;
        }

        // CharBinderID : 49 ~ 96 (Shifted)
        for (int i = 49; i < 97; i++)
        {
            string s = nowBindingKeyBind.CharMap[i].ToString();
            if (s == "\0" || s == "") s = "(NULL)";
            if (s == " ") s = "(Space)";
            charBinder[i].GetComponent<RectTransform>().Find("CharInputField").GetComponent<InputField>().text = s;
        }
    }

}

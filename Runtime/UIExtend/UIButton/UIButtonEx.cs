using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UISystem
{
    /// <summary>
    /// ��ť��չ
    /// </summary>
    public class UIButtonEx : Button
    {
        private TextMeshProUGUI buttonTxt;
        private UIAtlasImage uiAtlasImage;
        private Material oldImageMaterial;

        [SerializeField, LabelText("��ť������ʱ��(��)")]
        private float clickCd = 0.5f;
        //�´ε����ʱ��
        private float nextClickTime;

        /// <summary>
        /// �ûҲ�����
        /// </summary>
        private bool _gray = false;

        [SerializeField]
        public bool Gray
        {
            get { return _gray; }
            set
            {
                if (_gray == value) return;
                _gray = value;
                if (image != null)
                {
                    if (image is UIAtlasImage uiAtlasImage)
                    {
                        uiAtlasImage.IsGray = _gray;
                    }
                    else
                    {
                        if (_gray)
                        {
                            UIShaderEffect.SetUIGray(image);
                            image.SetMaterialDirty();
                        }
                        else
                        {
                            image.material = oldImageMaterial;
                        }
                    }
                }
            }
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            targetGraphic = GetComponentInChildren<Image>();
            buttonTxt = GetComponentInChildren<TextMeshProUGUI>();
        }
#endif

        protected override void Awake()
        {
            base.Awake();

            if(image != null)
                uiAtlasImage = image as UIAtlasImage;
        }

        protected override void OnDestroy()
        {
            onClick.RemoveAllListeners();
            base.OnDestroy();
        }

        /// <summary>
        /// ��ӵ���¼�
        /// </summary>
        /// <param name="action">�ص��¼�</param>
        /// <param name="clearAll">�Ƿ�������֮ǰ�����м���</param>
        public void AddListener(UnityAction action,bool clearAll = true)
        {
            if (clearAll)
                onClick.RemoveAllListeners();

            if (action != null)
                onClick.AddListener(action);
        }

        /// <summary>
        /// �Ƴ�����¼�
        /// </summary>
        /// <param name="action"></param>
        public void RemoveListener(UnityAction action)
        {
            if (action != null)
                onClick.RemoveListener(action);
        }

        /// <summary>
        /// �����ı�����
        /// </summary>
        /// <param name="text"></param>
        public void SetText(string text)
        {
            buttonTxt?.SetText(text);
        }

        /// <summary>
        /// ����ͼƬ
        /// </summary>
        /// <param name="icon"></param>
        public void SetImage(string icon)
        {
            uiAtlasImage?.SetSprite(icon);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            var curTime = Time.unscaledTime;
            if (nextClickTime > curTime)
            {
                return;
            }
            nextClickTime = curTime + clickCd;
            base.OnPointerClick(eventData);
        }

        /// <summary>
        /// �ûҲ�����
        /// </summary>
        /// <param name="enable"></param>
        public void SetExButtonEnable(bool enable)
        {
            Gray = !enable;
            this.interactable = enable;
        }

    }
}
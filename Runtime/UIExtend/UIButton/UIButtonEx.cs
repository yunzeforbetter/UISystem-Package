using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UISystem
{
    /// <summary>
    /// 按钮扩展
    /// </summary>
    public class UIButtonEx : Button
    {
        private TextMeshProUGUI buttonTxt;
        private UIAtlasImage uiAtlasImage;
        private Material oldImageMaterial;

        [SerializeField, LabelText("按钮点击间隔时间(秒)")]
        private float clickCd = 0.5f;
        //下次点击的时间
        private float nextClickTime;

        /// <summary>
        /// 置灰不禁用
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
        /// 添加点击事件
        /// </summary>
        /// <param name="action">回调事件</param>
        /// <param name="clearAll">是否先清理之前的所有监听</param>
        public void AddListener(UnityAction action,bool clearAll = true)
        {
            if (clearAll)
                onClick.RemoveAllListeners();

            if (action != null)
                onClick.AddListener(action);
        }

        /// <summary>
        /// 移除点击事件
        /// </summary>
        /// <param name="action"></param>
        public void RemoveListener(UnityAction action)
        {
            if (action != null)
                onClick.RemoveListener(action);
        }

        /// <summary>
        /// 设置文本内容
        /// </summary>
        /// <param name="text"></param>
        public void SetText(string text)
        {
            buttonTxt?.SetText(text);
        }

        /// <summary>
        /// 设置图片
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
        /// 置灰并禁用
        /// </summary>
        /// <param name="enable"></param>
        public void SetExButtonEnable(bool enable)
        {
            Gray = !enable;
            this.interactable = enable;
        }

    }
}
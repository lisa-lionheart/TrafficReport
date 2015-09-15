using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrafficReport.Util;
using UnityEngine;


namespace TrafficReport.Assets.Source.UI
{
    class ReportButton : UIButton
    {

        private bool toggleState;

        public bool ToggleState
        {
            get { return toggleState; }
            set {

                if (toggleState != value)
                {
                    normalBgSprite = toggleState ? "BgActive" : "BgNormal";
                    hoveredBgSprite = toggleState ? "BgActive" : "BgHover";
                    toggleState = value;
                }
            }
        }

        public static ReportButton Create()
        {

            GameObject go = new GameObject();
            
            go.transform.localPosition = Vector3.zero;


            ReportButton report =  go.AddComponent<ReportButton>();


            UIView.GetAView().AttachUIComponent(go);

            return report;
        }


        public void Start()
        {
            
            transform.localPosition = Config.instance.newButtonPos;

            atlas = ScriptableObject.CreateInstance<UITextureAtlas>();

            atlas.material = new Material(Shader.Find("UI/Default UI Shader"));
            Texture2D texture = ResourceLoader.loadTexture("Materials/Sprite.png");
            atlas.material.mainTexture = texture;

            atlas.AddSprites( new UITextureAtlas.SpriteInfo[]{

                new UITextureAtlas.SpriteInfo() {
                    name = "BgHover",
                    region = new Rect(0,0,0.5f,0.5f),
                    texture = texture
                },
                new UITextureAtlas.SpriteInfo() {
                    name = "BgActive",
                    region = new Rect(0.5f,0.5f,0.5f,0.5f),
                    texture = texture
                },
                new UITextureAtlas.SpriteInfo() {
                    name = "BgNormal",
                    region = new Rect(0,0.5f,0.5f,0.5f),
                    texture = texture
                },
                new UITextureAtlas.SpriteInfo() {
                    name = "Icon",
                    region = new Rect(0.5f,0,0.5f,0.5f),
                    texture = texture
                }
            });
      
            normalBgSprite = "BgNormal";
            hoveredBgSprite = "BgHover";
            pressedBgSprite = "BgActive";
            normalFgSprite = "Icon";
            width = 50f;
            height = 50f;

            var dragGo = new GameObject("DragHandler");
            dragGo.transform.parent = transform;
            dragGo.transform.localPosition = Vector3.zero;
            var drag = dragGo.AddComponent<UIDragHandle>();
            drag.width = width;
            drag.height = height;
        }

        protected override void OnPositionChanged()
        {
            Debug.Log("Position changed: " + transform.localPosition);
            Config.instance.newButtonPos = transform.localPosition;
            Config.instance.Save();
            base.OnPositionChanged();
        }

    }
}

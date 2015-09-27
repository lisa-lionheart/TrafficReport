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
                    normalBgSprite = !toggleState ? "BgActive" : "BgNormal";
                    hoveredBgSprite = !toggleState ? "BgActive" : "BgHover";
                    toggleState = value;
                }
            }
        }

        public static ReportButton Create()
        {

            GameObject go = new GameObject("TrafficReportButton");
            ReportButton report =  go.AddComponent<ReportButton>();

            UIView.GetAView().AttachUIComponent(go);

            return report;
        }


        public override void Start()
        {
            
            absolutePosition = Config.instance.pos;

            atlas = ScriptableObject.CreateInstance<UITextureAtlas>();

            atlas.material = new Material(Shader.Find("UI/Default UI Shader"));
            Texture2D texture = ResourceLoader.loadTexture("Materials/Sprite.png");
            atlas.material.mainTexture = texture;


            UITextureAtlas.SpriteInfo[] sprites = {

                new UITextureAtlas.SpriteInfo() {
                    name = "BgHover",
                    region = new Rect(0.0f,0.0f,0.5f,0.5f)
                },
                new UITextureAtlas.SpriteInfo() {
                    name = "BgActive",
                    region = new Rect(0.5f,0.5f,0.5f,0.5f)
                },
                new UITextureAtlas.SpriteInfo() {
                    name = "BgNormal",
                    region = new Rect(0.0f,0.5f,0.5f,0.5f)
                },
                new UITextureAtlas.SpriteInfo() {
                    name = "Icon",
                    region = new Rect(0.5f,0.0f,0.5f,0.5f)
                }
            };

            foreach (UITextureAtlas.SpriteInfo s in sprites)
            {
                Texture2D t = new Texture2D(50, 50);
                t.SetPixels(texture.GetPixels((int)(100 * s.region.x), (int)(100 * s.region.y), (int)(100 * s.region.width), (int)(100 * s.region.height)));
                s.texture = t;
                atlas.AddSprite(s);
            }

            playAudioEvents = true;
            tooltip = "Traffic Report Tool";
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

            base.Start();
        }

        protected override void OnPositionChanged()
        {
            Log.debug("Position changed: " + absolutePosition);
            Config.instance.pos = absolutePosition;
            Config.instance.NotifyChange();
            base.OnPositionChanged();
        }

    }
}

using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ImmersiveOpening
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotSwappableAttribute : Attribute
    {
    }
    [HotSwappable]
    public class Window_ImmersiveOpening : Window
    {
        public override Vector2 InitialSize => new Vector2(UI.screenWidth, UI.screenHeight);
        public override float Margin => 0f;
        private List<string> sentences;
        private int currentSentenceIndex = -1;
        private float sentenceStartTime;
        private bool waitingForClick;
        private float allDoneTime;
        private SoundDef closeSound;
        private IntVec3 mapCenter;
        private float simulatedTickTimer;

        public Window_ImmersiveOpening(List<string> sentences, SoundDef closeSound)
        {
            this.sentences = sentences;
            this.closeSound = closeSound;
            drawInScreenshotMode = true;
            forcePause = true;
            preventCameraMotion = true;
            doWindowBackground = false;
            drawShadow = false;
            doCloseX = false;
            closeOnAccept = false;
            closeOnCancel = false;
            absorbInputAroundWindow = true;
        }

        public override void PreOpen()
        {
            base.PreOpen();
            Root_OnGUI_Patch.isImmersiveOpeningActive = true;
            Find.MusicManagerPlay.disabled = true;
            mapCenter = Find.CurrentMap.Center;
            Find.ScreenshotModeHandler.Active = true;
            NextSentence();
        }

        private void NextSentence()
        {
            currentSentenceIndex++;
            if (currentSentenceIndex >= sentences.Count)
            {
                waitingForClick = true;
                allDoneTime = Time.realtimeSinceStartup;
                return;
            }

            sentenceStartTime = Time.realtimeSinceStartup;

            IntVec3 start = mapCenter + new IntVec3(Rand.Range(-30, 30), 0, Rand.Range(-30, 30));
            start.ClampInsideMap(Find.CurrentMap);
            IntVec3 end = mapCenter + new IntVec3(Rand.Range(-30, 30), 0, Rand.Range(-30, 30));
            end.ClampInsideMap(Find.CurrentMap);

            var startPos = start.ToVector3Shifted();
            var endPos = end.ToVector3Shifted();

            Find.CameraDriver.SetRootPosAndSize(startPos, CameraDriver.MinAltitude);
            Find.CameraDriver.panner.PanTo(
                new CameraPanner.Interpolant(startPos, CameraDriver.MinAltitude),
                new CameraPanner.Interpolant(endPos, CameraDriver.MinAltitude),
                ImmersiveOpeningMod.settings.timeBetweenSentences
            );
        }

        public override void WindowUpdate()
        {
            base.WindowUpdate();

            simulatedTickTimer += Time.deltaTime;
            while (simulatedTickTimer >= 1f / 60f)
            {
                Find.CurrentMap.windManager.WindManagerTick();
                simulatedTickTimer -= 1f / 60f;
            }

            if (!waitingForClick)
            {
                float elapsed = Time.realtimeSinceStartup - sentenceStartTime;
                if (elapsed >= ImmersiveOpeningMod.settings.timeBetweenSentences)
                {
                    NextSentence();
                }
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (!waitingForClick && Event.current.type == EventType.MouseDown)
            {
                NextSentence();
                Event.current.Use();
            }

            if (waitingForClick)
            {
                Widgets.DrawBoxSolid(inRect, Color.black);

                if (Time.realtimeSinceStartup - allDoneTime > 2f)
                {
                    Text.Font = GameFont.Medium;
                    Text.Anchor = TextAnchor.MiddleCenter;
                    GUI.color = Color.white;
                    Widgets.Label(inRect, "IO_ClickToStart".Translate());
                    Text.Anchor = TextAnchor.UpperLeft;
                }

                if (Event.current.type == EventType.MouseDown)
                {
                    Close();
                    Event.current.Use();
                }
                return;
            }

            float elapsed = Time.realtimeSinceStartup - sentenceStartTime;
            float fadeTime = 1f;
            float blackScreenAlpha = 0f;

            if (elapsed < fadeTime)
            {
                blackScreenAlpha = 1f - (elapsed / fadeTime);
            }

            if (blackScreenAlpha > 0f)
            {
                GUI.color = new Color(0f, 0f, 0f, blackScreenAlpha);
                Widgets.DrawBoxSolid(inRect, GUI.color);
            }

            if (ImmersiveOpeningMod.settings.letterboxing)
            {
                GUI.color = Color.black;
                float barHeight = inRect.height * 0.15f;
                Widgets.DrawBoxSolid(new Rect(0f, 0f, inRect.width, barHeight), GUI.color);
                Widgets.DrawBoxSolid(new Rect(0f, inRect.height - barHeight, inRect.width, barHeight), GUI.color);
            }

            GUI.color = new Color(1f, 1f, 1f, 1f - blackScreenAlpha);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;

            var textRect = new Rect(inRect.width / 6f, inRect.height * 0.8f, inRect.width * 2f / 3f, inRect.height * 0.2f);
            Widgets.Label(textRect, sentences[currentSentenceIndex]);

            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        public override void OnCancelKeyPressed()
        {
            Close();
            Event.current.Use();
        }

        public override void PostClose()
        {
            base.PostClose();
            Root_OnGUI_Patch.isImmersiveOpeningActive = false;
            Find.CameraDriver.SetRootPosAndSize(mapCenter.ToVector3Shifted(), CameraDriver.StartingSize);
            Find.MusicManagerPlay.ForceSilenceFor(7f);
            Find.MusicManagerPlay.disabled = false;
            Find.WindowStack.Notify_GameStartDialogClosed();
            Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
            TutorSystem.Notify_Event("GameStartDialogClosed");
            (closeSound ?? SoundDefOf.GameStartSting).PlayOneShotOnCamera();
            Find.ScreenshotModeHandler.Active = false;
        }
    }
}

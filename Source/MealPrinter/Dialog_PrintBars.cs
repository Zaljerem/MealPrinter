// Verse.Dialog_Slider

using System;
using UnityEngine;
using Verse;

public class Dialog_PrintBars : Window
{
    private const float BotAreaHeight = 30f;

    private const float TopPadding = 15f;

    private readonly Action<int, bool, bool> confirmAction;

    public readonly int from;

    public readonly float roundTo;
    public readonly Func<int, string> textGetter;

    public readonly int to;

    private int curValue;

    private bool forbid;

    private bool rear;

    public Dialog_PrintBars(Func<int, string> textGetter, int from, int to, Action<int, bool, bool> confirmAction,
        int startingValue = int.MinValue, float roundTo = 1f)
    {
        this.textGetter = textGetter;
        this.from = from;
        this.to = to;
        this.confirmAction = confirmAction;
        this.roundTo = roundTo;
        forbid = false;
        rear = false;
        forcePause = true;
        closeOnClickedOutside = true;
        curValue = startingValue == int.MinValue ? from : startingValue;
    }

    public Dialog_PrintBars(string text, int from, int to, Action<int, bool, bool> confirmAction,
        int startingValue = int.MinValue, float roundTo = 1f)
        : this(val => string.Format(text, val), from, to, confirmAction, startingValue, roundTo)
    {
    }

    public override Vector2 InitialSize => new Vector2(300f, 230f);

    //Defines the layout of the printing window
    public override void DoWindowContents(Rect inRect)
    {
        var rect = new Rect(inRect.x, inRect.y + 30f, inRect.width, 30f);
        var forbidCheckbox = new Rect(inRect.x, inRect.y + 90f, inRect.width, 30f);
        var rearCheckbox = new Rect(inRect.x, inRect.y + 120f, inRect.width, 30f);
        curValue = (int)Widgets.HorizontalSlider(rect, curValue, from, to, true, textGetter(curValue), null, null,
            roundTo);
        var sepY = inRect.y - 5f;
        var sepY2 = inRect.y + 60f;
        Widgets.ListSeparator(ref sepY, 280, "BarQuantity".Translate());
        Widgets.ListSeparator(ref sepY2, 280, "PrintSettings".Translate());
        Widgets.CheckboxLabeled(forbidCheckbox, "PrintForbidden".Translate(), ref forbid);
        Widgets.CheckboxLabeled(rearCheckbox, "PrintRear".Translate(), ref rear);
        Text.Font = GameFont.Small;
        if (Widgets.ButtonText(new Rect(inRect.x, inRect.yMax - 32f, inRect.width / 2f, 30f),
                "CancelButton".Translate()))
        {
            Close();
        }

        if (!Widgets.ButtonText(new Rect(inRect.x + (inRect.width / 2f), inRect.yMax - 32f, inRect.width / 2f, 30f),
                "ConfirmPrint".Translate()))
        {
            return;
        }

        Close();
        confirmAction(curValue, forbid, rear);
    }
}
﻿using System.Drawing;

namespace CSharpMath.Rendering {
  public abstract class TextPainter<TCanvas, TColor> : Painter<TCanvas, TextSource, TColor> {
    public TextPainter(float fontSize = DefaultFontSize, float lineWidth = DefaultFontSize * 100) : base(fontSize) { }

    //display maths should always be center-aligned regardless of parameter for Draw()
    protected Display.MathListDisplay<Fonts, Glyph> _absolutePositionDisplay;
    protected IDisplay<Fonts, Glyph> _alignedDisplay;

    public TextAtom Atom { get => Source.Atom; set => Source = new TextSource(value); }
    public string Text { get => Source.Text; set => Source = new TextSource(value); }

    public RectangleF? Measure(float canvasWidth) => MeasureCore(ref _alignedDisplay, canvasWidth);

    protected override void UpdateDisplay(ref IDisplay<Fonts, Glyph> returnDisplay, float canvasWidth) {
      if (Atom == null) return;
      float accumulatedHeight = 0, lineWidth = 0, lineHeight = 0;
      void AddDisplaysWithLineBreaks(TextAtom atom,
        System.Collections.Generic.List<IDisplay<Fonts, Glyph>> displayList,
        System.Collections.Generic.List<IDisplay<Fonts, Glyph>> displayMathList) {
        IDisplay<Fonts, Glyph> display;
        switch (atom) {
          case TextAtom.List list:
            foreach (var a in list.Content) AddDisplaysWithLineBreaks(a, displayList, displayMathList);
            break;
          case TextAtom.Newline n:
            accumulatedHeight += lineHeight;
            lineWidth = lineHeight = 0;
            break;
          case TextAtom.Math m when m.DisplayStyle:
            accumulatedHeight += lineHeight;
            accumulatedHeight += lineHeight;
            display = atom.ToDisplay(Fonts, default);
            display.Position = new PointF(
              IPainterExtensions.GetDisplayPosition(display.Width, display.Ascent, display.Descent, Fonts.PointSize, false, canvasWidth, float.NaN, TextAlignment.Top, default, default, default).X,
              display.Position.Y - accumulatedHeight);
            accumulatedHeight += display.Ascent + display.Descent;
            lineWidth = lineHeight = 0;
            displayMathList.Add(display);
            break;
          default:
            display = atom.ToDisplay(Fonts, default);
            var bounds = display.ComputeDisplayBounds();
            if (lineWidth + display.Width > canvasWidth) {
              accumulatedHeight += lineHeight;
              //canvas inverted, so minus accumulatedHeight instead of plus
              display.Position = new PointF(0, display.Position.Y - accumulatedHeight);
              lineWidth = bounds.Width;
              lineHeight = bounds.Height;
            } else {
              lineHeight = System.Math.Max(lineHeight, bounds.Height);
              //canvas inverted, so negate accumulatedHeight
              display.Position = new PointF(lineWidth, -accumulatedHeight);
              lineWidth += bounds.Width;
            }
            displayList.Add(display);
            break;
        }
      }
      var returnList = new System.Collections.Generic.List<IDisplay<Fonts, Glyph>>();
      var absolutePositionList = new System.Collections.Generic.List<IDisplay<Fonts, Glyph>>();
      AddDisplaysWithLineBreaks(Atom, returnList, absolutePositionList);
      _absolutePositionDisplay = new Display.MathListDisplay<Fonts, Glyph>(absolutePositionList);
      returnDisplay = new Display.MathListDisplay<Fonts, Glyph>(returnList);
    }

    public void Draw(TCanvas canvas) {
      var c = WrapCanvas(canvas);
      UpdateDisplay(ref _alignedDisplay, c.Width);
      _absolutePositionDisplay = new Display.MathListDisplay<Fonts, Glyph>()
      Draw(c, _alignedDisplay);
    }
    public override void Draw(TCanvas canvas, TextAlignment alignment = TextAlignment.Center, Thickness padding = default, float offsetX = 0, float offsetY = 0) {
      var c = WrapCanvas(canvas);
      if (!Source.IsValid) DrawError(c);
      else {
        UpdateDisplay(ref _alignedDisplay, c.Width);
        Draw(c, _display, IPainterExtensions.GetDisplayPosition(_display.Width, _display.Ascent, _display.Descent, FontSize, CoordinatesFromBottomLeftInsteadOfTopLeft, c.Width, c.Height, alignment, padding, offsetX, offsetY));
      }
    }
  }
}

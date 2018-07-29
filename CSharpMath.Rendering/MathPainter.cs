﻿using System.Drawing;
using CSharpMath.Display;
using CSharpMath.FrontEnd;
using CSharpMath.Interfaces;

namespace CSharpMath.Rendering {
  public abstract class MathPainter<TCanvas, TColor> : Painter<TCanvas, MathSource, TColor> {
    public MathPainter(float fontSize = DefaultFontSize) : base(fontSize) { }

    public IMathList MathList { get => Source.MathList; set => Source = new MathSource(value); }
    public string LaTeX { get => Source.LaTeX; set => Source = new MathSource(value); }

    public RectangleF? Measure => MeasureCore();

    protected override IDisplay<Fonts, Glyph> CreateDisplay(float canvasWidth = 0) =>
      TypesettingContext.Instance.CreateLine(MathList, Fonts, LineStyle);

    public void Draw(TCanvas canvas, TextAlignment alignment = TextAlignment.Center, Thickness padding = default, float offsetX = 0, float offsetY = 0) {
      var c = WrapCanvas(canvas);
      if (!Source.IsValid) DrawError(c);
      else {
        if (_displayChanged) UpdateDisplay(c.Width);
        Draw(c, IPainterExtensions.GetDisplayPosition(_display.Width, _display.Ascent, _display.Descent, FontSize, CoordinatesFromBottomLeftInsteadOfTopLeft, c.Width, c.Height, alignment, padding, offsetX, offsetY));
      }
    }

    public void Draw(TCanvas canvas, float x, float y) =>
      Draw(WrapCanvas(canvas), new PointF(x, CoordinatesFromBottomLeftInsteadOfTopLeft ? y : -y));

    public void Draw(TCanvas canvas, PointF position) {
      if (CoordinatesFromBottomLeftInsteadOfTopLeft) position.Y *= -1;
      Draw(WrapCanvas(canvas), position);
    }
  }
}

using System.Text;

namespace CSharpMath.Atom.Atoms {
  public class Radical: MathAtom {
    public MathList? Degree { get; private set; }
    /// <summary>Whatever is under the square root sign</summary>
    public MathList Radicand { get; private set; }
    public Radical(MathList? degree, MathList radicand) : base(string.Empty) =>
      (Degree, Radicand) = (degree, radicand);
    public new Radical Clone(bool finalize) => (Radical)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
      new Radical(Degree?.Clone(finalize), Radicand.Clone(finalize));
    public override bool ScriptsAllowed => true;
    public override string DebugString =>
      new StringBuilder(@"\sqrt")
      .AppendInBracketsOrNothing(Degree?.DebugString)
      .AppendInBracesOrLiteralNull(Radicand.DebugString)
      .AppendDebugStringOfScripts(this).ToString();
  }
}
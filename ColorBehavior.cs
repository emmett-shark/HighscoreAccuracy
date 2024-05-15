namespace HighscoreAccuracy;

public enum ColorBehavior
{
    /// <summary>
    /// Color depends on how close you are to your PB
    /// </summary>
    /// <remarks>
    /// <ul>
    ///     <li>Green: Above PB</li>
    ///     <li>Yellow: Up to 10% below PB</li>
    ///     <li>Red: More than 10% below PB</li>
    /// </ul>
    /// </remarks>
    Closeness,

    /// <summary>
    /// Color depends on whether a PB is possible or not
    /// </summary>
    /// <remarks>
    /// <ul>
    ///     <li>Dark green: Above PB and cannot avoid setting a new PB</li>
    ///     <li>Light green: Above PB</li>
    ///     <li>Yellow: PB is still possible with the remaining notes left</li>
    ///     <li>Red: PB is impossible</li>
    /// </ul>
    /// </remarks>
    PbPossibility,

    /// <summary>
    /// A combination of the above two
    /// </summary>
    /// <remarks>
    /// <ul>
    ///     <li>Dark green: Above PB and cannot avoid setting a new PB</li>
    ///     <li>Light green: Above PB</li>
    ///     <li>Yellow: PB is still possible with the remaining notes left</li>
    ///     <li>Orange: PB is still possible with the remaining notes left, but you're more than 10% below PB</li>
    ///     <li>Red: PB is impossible</li>
    /// </ul>
    /// </remarks>
    Hybrid,
}

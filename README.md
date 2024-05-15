# HighscoreAccuracy
Adds accuracy information to level select and when playing

## Dependencies:
- [BaboonAPI](https://github.com/tc-mods/BaboonAPI)

## Changelog
v1.3.8
- Add option to change text color based on how close you are to a PB (old behavior), whether a PB is possible anymore, or a combination of the two (the new default)

v.1.3.7
- Fix for new scoring system in TC 1.20
- Fix % sometimes not showing on points screen

v1.3.6
- Don't calculate highscore if tmb is over 2m bytes
- Default real accuracy

v1.3.5
- Fix for TC 1.18B

v1.3.4
- Use new toottally settings

v1.3.3
- Fix for <= 0 length notes
- Priority higher for showing % in score screen

v1.3.2
- Made settings words make more sense

v1.3.1
- Don't use TrombSettings at all

v1.3.0
- Add increasing and decreasing accuracy options
- Add optional toottally settings
- Fix wrong % on skipping through replays
- Fix max score calc (again)

v1.2.1
- Fix S rank calculation and true max %
- Make stuff public just for Electro

v1.2.0
- Trombloader v2 compatibility

v1.1.52
- Fix for TC 1.098B

v1.1.5
- Update font size for TC 1.09, trombsettings is now optional

v1.1.4
- Fix max score calculation for TC 1.088

v1.1.3
- Fix bug where custom song trackref does not match folder name

v1.1.2
- Fix for TC 1.086

v1.1.1
- Fix bug that displays wrong score after sorting
- Fix bug that doesn't search for all the songs
- Fix bug where realMax is used for calculation when you want gameMax

v1.1.0
- Initial release

## Features
- Shows accuracy on highscores in the level select menu
- Shows accuracy, letter score, and personal best while playing a track
- Shows accuracy on end screen
- Accuracy Types:
  - Base Game
    - Uses the internal calculations for the letter where >100% = S
  - Real
    - Calculates the actual maximum score for a track, here an S ≈ 60%
    - A 100% on this is practically impossible just because of how the game calculates scores
  - Decreasing: Uses real accuracy, but your % will always decrease or stay the same.
    - For example, ignoring multipliers, completely missing the first note of a 100 note song will give you 99%.
  - Increasing: Uses real accuracy, but your % will always increase or stay the same.
    - For example, ignoring multipliers, perfectly hitting the first note of a 100 note song will give you 1%.
- Most of the features can be enabled/disabled in the settings

<img src="https://i.imgur.com/LWLTWFz.jpg"/>
<img src="https://i.imgur.com/EDYfzlU.jpg"/>
<img src="https://i.imgur.com/gspIepv.jpg"/>

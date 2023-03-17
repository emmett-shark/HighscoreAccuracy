# HighscoreAccuracy
Adds accuracy information to level select and when playing

## Dependencies:
- [TrombLoader](https://github.com/NyxTheShield/TrombLoader)
- [TrombSettings](https://github.com/HypersonicSharkz/TrombSettings) (optional)

## Changelog
v1.1.6
- Trombloader v2 compatibility

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
- Most of the features can be enabled/disabled in the settings

<img src="https://i.imgur.com/LWLTWFz.jpg"/>
<img src="https://i.imgur.com/EDYfzlU.jpg"/>
<img src="https://i.imgur.com/gspIepv.jpg"/>

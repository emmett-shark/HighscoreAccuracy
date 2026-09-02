namespace HighscoreAccuracy.Enums;

public enum ModSpecificHighscoreMode
{
    Global, // Use baboon highscores only
    ModSpecific, // Use mod highscores only
    Hybrid // Use mod highscores if found, else use global highscores with asterisk
}

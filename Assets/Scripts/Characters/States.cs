namespace UnityPlatformer {
  /// <summary>
  /// States in wich the Character can be.
  /// Can be combine
  /// </summary>
  public enum States {
    None =                0,
    OnGround =            1,
    OnMovingPlatform =    3,
    OnSlope =             1 << 2 | OnGround,
    Jumping =             1 << 3,
    Hanging =             1 << 4 | Jumping,
    Falling =             1 << 5,
    FallingFast =         1 << 6 | Falling,
    Ladder =              1 << 7,
    WallSliding =         1 << 8,
    WallSticking =        1 << 9,
    MeleeAttack =         1 << 10,
    //Dashing,
    //Frozen,
    //Slipping,
    //FreedomState
  }
}
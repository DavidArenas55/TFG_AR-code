
public static class GameResults
{
    public static ConnectionConfig.Scenario Scenario = ConnectionConfig.Scenario.None;

    // Tiempo total que duró la partida (en segundos).
    public static float CompletionTime = 0f;

    // true si el escenario usaba cuenta atrás (countdown).
    public static bool TimeWasCountdown = false;

    // true si la partida terminó porque se agotó el tiempo (no se completó).
    public static bool RanOutOfTime = false;

    // Puntos / gemas de puntuación recogidas y total disponibles.
    public static int Score = 0;
    public static int ScoreTotal = 0;

    // gates atravesadas y total.
    public static int TargetsPassed = 0;
    public static int TargetsTotal = 0;

    // Información extra relevante en Drone Flow.
    public static int DroneFlowLevel = 0;
    public static bool DroneFlowSequential = false;

    // Niveles completados en Drone Flow secuencial.
    public static int DroneFlowLevelsCompleted = 0;
    public static int DroneFlowTotalLevels = 5;

    // Opcional: limpiar los datos al empezar una partida nueva.
    // CaptureResults() de RaceTimer los sobreescribe igualmente, así que no es estrictamente necesario, pero ayudaría a evitar datos viejos.
    public static void Reset()
    {
        Scenario = ConnectionConfig.Scenario.None;
        CompletionTime = 0f;
        TimeWasCountdown = false;
        RanOutOfTime = false;
        Score = 0;
        ScoreTotal = 0;
        TargetsPassed = 0;
        TargetsTotal = 0;
        DroneFlowLevel = 0;
        DroneFlowSequential = false;
        DroneFlowLevelsCompleted = 0;
        DroneFlowTotalLevels = 5;
    }
}
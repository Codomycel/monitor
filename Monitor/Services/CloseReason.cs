namespace SystemActivityTracker.Services
{
    public enum CloseReason
    {
        Unknown = 0,
        Graceful = 1,
        UserInitiatedExit = 2,
        ShutdownOrLogoff = 3,
        UnhandledUIException = 4,
        UnhandledBackgroundException = 5,
        UnobservedTaskException = 6,
        HangSuspected = 7,
        PreviousRunUnexpectedTermination = 8,
        ProcessKilled = 9,
        PowerLossOrHardShutdown = 10
    }
}

using System;

namespace SystemActivityTracker.Services.Abstractions
{
    public interface ICrashLogReader
    {
        bool TryReadLatestNonGracefulEvent(out CrashLogEvent evt, out string sourcePath);
        bool TryReadLastRun(out LastRunRecord record);
    }

    public sealed class LastRunRecord
    {
        public Guid RunId { get; set; }
        public DateTime StartUtc { get; set; }
        public bool IsRunning { get; set; }
        public DateTime? EndUtc { get; set; }
        public string? EndType { get; set; }
        public string? CloseReason { get; set; }
        public DateTime LastHeartbeatUtc { get; set; }
    }

    public sealed class CrashLogEvent
    {
        public DateTime TimestampUtc { get; set; }
        public Guid RunId { get; set; }
        public DateTime StartUtc { get; set; }
        public string? EventName { get; set; }
        public string? CloseReason { get; set; }

        public bool IsUserInitiatedExit { get; set; }
        public bool ShutdownOrLogoffDetected { get; set; }
        public bool HangSuspected { get; set; }

        public string? ExceptionType { get; set; }
        public string? ExceptionMessage { get; set; }
        public string? ExceptionStackTrace { get; set; }

        public LastRunRecord? PreviousRun { get; set; }
        public string? PreviousRunClassification { get; set; }
    }
}

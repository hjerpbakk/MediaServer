namespace MediaServer.ViewModels {
    public interface ITalksViewModel {   
        TalkSummary[] Talks { get; }
        bool ShowConference { get; }
    }
}

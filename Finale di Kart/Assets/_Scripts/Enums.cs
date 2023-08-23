namespace _Scripts
{
    public enum NodeType
    {
        None,
        Server,
        Client,
    }


    public enum ChallengeState {
        Requested,
        Rejected,
        Accepted,
        InProgress,
    }

    public enum ChallengeResponse {
        Accept,
        Reject,
    }
}
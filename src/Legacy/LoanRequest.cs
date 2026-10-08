namespace LibraryDesk.Legacy;

public sealed record LoanRequest(
    int DocumentId,
    Reader Reader,
    IReadOnlyList<LoanLine>? Items,
    LoanState State,
    string Currency,
    bool SendMail);

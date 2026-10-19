namespace Web.Application.Contracts;

public  record ChannelResult(string Channel, bool IsSuccess, string? Error = null);

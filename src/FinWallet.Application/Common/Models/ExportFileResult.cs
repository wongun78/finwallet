namespace FinWallet.Application.Common.Models;

public sealed record ExportFileResult(byte[] Content, string ContentType, string FileName);
